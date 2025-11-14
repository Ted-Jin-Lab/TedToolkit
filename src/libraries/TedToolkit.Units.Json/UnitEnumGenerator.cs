using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TedToolkit.RoslynHelper.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.Units.Json;

public sealed class UnitEnumGenerator(Quantity quantity)
{
    private static string GetXmlComment(string summary, string remark)
    {
        var result = $"/// <summary>{summary}</summary>";
        if (string.IsNullOrEmpty(remark)) return result;
        result += $"\n/// <remarks>{remark}</remarks>";
        return result;
    }

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
                Parameter(Identifier("index"))
                    .WithType(PredefinedType(Token(SyntaxKind.IntKeyword))),
                Parameter(Identifier("formatProvider"))
                    .WithType(NullableType(IdentifierName("global::System.IFormatProvider")))
            ]))
            .WithBody(Block(
                LocalDeclarationStatement(
                    VariableDeclaration(IdentifierName("var"))
                        .WithVariables(
                        [
                            VariableDeclarator(Identifier("culture"))
                                .WithInitializer(EqualsValueClause(
                                    InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("global::TedToolkit.Units.Internal"),
                                            IdentifierName("GetCulture")))
                                        .WithArgumentList(ArgumentList(
                                        [
                                            Argument(
                                                IdentifierName("formatProvider"))
                                        ]))))
                        ])),
                ReturnStatement(SwitchExpression(IdentifierName("unit"))
                    .WithArms(
                    [
                        ..quantity.UnitsInfos.Select(i =>
                            SwitchExpressionArm(ConstantPattern(MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName(quantity.UnitName),
                                    IdentifierName(i.Name))),
                                SwitchExpression(IdentifierName("culture"))
                                    .WithArms(
                                    [
                                        ..i.LocalNames.OrderBy(i => i.Key == "en-US").Select(k =>
                                        {
                                            var getStringExpr = InvocationExpression(
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        IdentifierName("global::TedToolkit.Units.Internal"),
                                                        IdentifierName("GetString")))
                                                .WithArgumentList(ArgumentList(
                                                [
                                                    Argument(IdentifierName("index")),
                                                    ..k.Value.Select(n => Argument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal(n))))
                                                ]));

                                            return SwitchExpressionArm(
                                                k.Key == "en-US"
                                                    ? DiscardPattern()
                                                    : ConstantPattern(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal(k.Key))),
                                                getStringExpr);
                                        }),
                                    ]))),
                        SwitchExpressionArm(DiscardPattern(),
                            InvocationExpression(MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("unit"), IdentifierName("ToString")))),
                    ]))));
    }

    public string GenerateCode()
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

        return namescape.NodeToString();
    }

    public string FileName => quantity.UnitName + ".g.cs";
}