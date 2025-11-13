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
        if (string.IsNullOrEmpty(remark)) return result;
        result += $"\n/// <remarks>{remark}</remarks>";
        return result;
    }

    public void GenerateCode(string path)
    {
        var namescape = NamespaceDeclaration("TedToolkit.Units")
            .WithMembers([
                EnumDeclaration(quantity.UnitName)
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithBaseList(BaseList(
                    [
                        SimpleBaseType(PredefinedType(Token(SyntaxKind.ByteKeyword)))
                    ]))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitEnumGenerator))])
                    .WithXmlComment(GetXmlComment(quantity.XmlDocSummary, quantity.XmlDocRemarks))
                    .WithMembers(
                    [
                        ..quantity.UnitsInfos.Select((u, i) => EnumMemberDeclaration(Identifier(u.Name))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitEnumGenerator))])
                            .WithXmlComment(
                                u.Prefix is Prefix.None
                                    ? GetXmlComment(u.Unit.XmlDocSummary, u.Unit.XmlDocRemarks)
                                    : $"""
                                       /// <summary>
                                       /// Represents the <see cref="{u.Unit.SingularName}"/> unit scaled by the {u.PrefixInfo.Type} prefix "{u.Prefix}" (×{(u.PrefixInfo.Type is PrefixType.SI ? 10 : 2)}{u.PrefixInfo.Factor.ToSuperscript()}).
                                       /// </summary>
                                       """)
                            .WithEqualsValue(EqualsValueClause(
                                LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(i + 1)))))
                    ])
            ]);

        File.WriteAllText(Path.Combine(path, quantity.Name + ".g.cs"), namescape.NodeToString());
    }
}