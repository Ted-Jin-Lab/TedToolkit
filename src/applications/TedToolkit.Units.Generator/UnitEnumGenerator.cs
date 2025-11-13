using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TedToolkit.RoslynHelper.Extensions;
using TedToolkit.Units.Json;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.Units.Generator;

internal sealed class UnitEnumGenerator(Quantity quantity)
{
    private static string GetXmlComment(string summary, string remark)
    {
        var result = $"/// <summary>{summary}</summary>";
        if (string.IsNullOrEmpty(remark))  return result;
        result += $"\n/// <remarks>{remark}</remarks>";
        return result;
    }
    
    public void GenerateCode(string path)
    {
        var namescape = NamespaceDeclaration("TedToolkit.Units")
            .WithMembers([
                EnumDeclaration(quantity.Name + "Unit")
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithBaseList(BaseList(
                    [
                        SimpleBaseType(PredefinedType(Token(SyntaxKind.ByteKeyword)))
                    ]))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitEnumGenerator))])
                    .WithXmlComment(GetXmlComment(quantity.XmlDocSummary, quantity.XmlDocRemarks))
                    .WithMembers(
                    [
                        ..quantity.Units.SelectMany(u =>
                        {
                            return (IEnumerable<EnumMemberDeclarationSyntax>)
                            [
                                EnumMemberDeclaration(Identifier(u.SingularName))
                                    .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitEnumGenerator))])
                                    .WithXmlComment(GetXmlComment(u.XmlDocSummary, u.XmlDocRemarks)),
                                ..u.Prefixes.Select(p =>
                                    EnumMemberDeclaration(Identifier(p.ToString() +char.ToLowerInvariant(u.SingularName[0]) + u.SingularName[1..]))
                                        .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitEnumGenerator))])
                                        .WithXmlComment($"""
                                                        /// <summary>
                                                        /// Represents the <see cref="{u.SingularName}"/> unit scaled by the {p.GetInfo().Type} prefix "{p}" (×{(p.GetInfo().Type is PrefixType.SI ? 10 : 2)}{p.GetInfo().Factor.ToSuperscript()}).
                                                        /// </summary>
                                                        """))
                            ];
                        }),
                    ])
            ]);

        File.WriteAllText(Path.Combine(path, quantity.Name + ".g.cs"), namescape.NodeToString());
    }
}