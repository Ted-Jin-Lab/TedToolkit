using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TedToolkit.RoslynHelper.Extensions;
using TedToolkit.Units.Data;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.Units.Analyzer;

public sealed class UnitEnumGenerator(IReadOnlyList<Unit> units)
{
    public void GenerateCode(SourceProductionContext context)
    {
        var nameSpace = NamespaceDeclaration("TedToolkit.Units")
            .WithMembers([
                EnumDeclaration("AllUnit")
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithBaseList(BaseList(
                    [
                        SimpleBaseType(PredefinedType(Token(SyntaxKind.UShortKeyword)))
                    ]))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitEnumGenerator))])
                    .WithXmlComment()
                    .WithMembers(
                    [
                        ..units.Select((u, i) => EnumMemberDeclaration(Identifier(u.GetUnitName(units)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitEnumGenerator))])
                            .WithXmlComment(Helpers.CreateSummary(u.Description, u.Links))
                            .WithEqualsValue(EqualsValueClause(
                                LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(i + 1)))))
                    ])
            ]);
        context.AddSource("_AllUnits.g.cs", nameSpace.NodeToString());
    }
}