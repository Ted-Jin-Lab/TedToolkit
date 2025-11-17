using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TedToolkit.Quantities.Data;
using TedToolkit.RoslynHelper.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.Quantities.Analyzer;

public sealed class QuantityUnitEnumGenerator(DataCollection data, Quantity quantity)
{
    public MethodDeclarationSyntax GenerateToString()
    {
        return MethodDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)),
                Identifier("ToString"))
            .WithModifiers(
                TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
            .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitEnumGenerator))])
            .WithXmlComment()
            .WithParameterList(ParameterList(
            [
                Parameter(Identifier("unit"))
                    .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)))
                    .WithType(IdentifierName(quantity.UnitName)),
                Parameter(Identifier("isSymbol"))
                    .WithType(PredefinedType(Token(SyntaxKind.BoolKeyword))),
                Parameter(Identifier("formatProvider"))
                    .WithType(NullableType(IdentifierName("global::System.IFormatProvider")))
            ]))
            .WithBody(Block(
                ReturnStatement(SwitchExpression(IdentifierName("unit"))
                    .WithArms(
                    [
                        ..quantity.Units.Select(u =>
                        {
                            var unit = data.Units[u];
                            var unitName = unit.GetUnitName(data.Units.Values);
                            return SwitchExpressionArm(ConstantPattern(MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName(quantity.UnitName),
                                    IdentifierName(unitName))),
                                InvocationExpression(
                                        IdentifierName("global::TedToolkit.Quantities.Internal.GetUnitString"))
                                    .WithArgumentList(ArgumentList(
                                    [
                                        Argument(IdentifierName("isSymbol")),
                                        Argument(IdentifierName("formatProvider")),
                                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                            Literal(unit.Symbol))),
                                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                            Literal(unit.Name))),
                                        ..unit.Labels.Select(pair => Argument(
                                            TupleExpression(
                                            [
                                                Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                                    Literal(pair.Key))),
                                                Argument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                                    Literal(pair.Value)))
                                            ])))
                                    ])));
                        }),
                        SwitchExpressionArm(DiscardPattern(),
                            InvocationExpression(MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("unit"), IdentifierName("ToString")))),
                    ]))));
    }

    public void GenerateCode(SourceProductionContext context)
    {
        var namescape = NamespaceDeclaration("TedToolkit.Quantities")
            .WithMembers([
                EnumDeclaration(quantity.UnitName)
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithBaseList(BaseList(
                    [
                        SimpleBaseType(PredefinedType(Token(SyntaxKind.UShortKeyword)))
                    ]))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityUnitEnumGenerator))])
                    .WithXmlComment(Helpers.CreateSummary(quantity.Description, quantity.Links, quantity.Dimension))
                    .WithMembers(
                    [
                        ..quantity.Units.Select(u =>
                        {
                            var unit = data.Units[u];
                            var unitName = unit.GetUnitName(data.Units.Values);
                            return EnumMemberDeclaration(Identifier(unitName))
                                .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityUnitEnumGenerator))])
                                .WithXmlCommentInheritDoc($"AllUnit.{unitName}")
                                .WithEqualsValue(EqualsValueClause(
                                    IdentifierName($"AllUnit.{unitName}")));
                        })
                    ])
            ]);

        context.AddSource(quantity.UnitName + ".g.cs", namescape.NodeToString());
    }
}