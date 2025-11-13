using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TedToolkit.RoslynHelper.Extensions;
using TedToolkit.RoslynHelper.Names;
using TedToolkit.Units.Json;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.Units.Analyzer;

internal class UnitStructGenerator(Quantity quantity, TypeName typeName, UnitSystem unitSystem, bool isPublic, bool simplify)
{
    public void GenerateCode(SourceProductionContext context)
    {
        var nameSpace = NamespaceDeclaration("TedToolkit.Units")
            .WithMembers([
                StructDeclaration(quantity.Name)
                    .WithModifiers(TokenList(Token(isPublic ? SyntaxKind.PublicKeyword : SyntaxKind.InternalKeyword),
                        Token(SyntaxKind.ReadOnlyKeyword), Token(SyntaxKind.PartialKeyword)))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
                    .WithXmlComment($"/// <inheritdoc cref=\"{quantity.UnitName}\"/>")
                    .WithMembers(
                    [
                        FieldDeclaration(
                                VariableDeclaration(IdentifierName(typeName.FullName))
                                    .WithVariables(
                                    [
                                        VariableDeclarator(Identifier("_value"))
                                    ]))
                            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword),
                                Token(SyntaxKind.ReadOnlyKeyword)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))]),

                        ConstructorDeclaration(Identifier(quantity.Name))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("value"))
                                    .WithType(IdentifierName(typeName.FullName)),
                                Parameter(Identifier("unit"))
                                    .WithType(IdentifierName(quantity.UnitName))
                            ]))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
                            .WithXmlComment()
                            .WithBody(Block(
                                CreateSwitchStatement(info =>
                                [
                                    ExpressionStatement(AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        IdentifierName("_value"),
                                        info.GetUnitToSystem(unitSystem, quantity.BaseDimensions)
                                            .ToExpression("value", simplify, typeName.Symbol))),
                                    ReturnStatement()
                                ])
                            )),

                        MethodDeclaration(IdentifierName(typeName.FullName),
                                Identifier("As"))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("unit"))
                                    .WithType(IdentifierName(quantity.UnitName))
                            ]))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
                            .WithXmlComment()
                            .WithBody(Block(
                                CreateSwitchStatement(info =>
                                [
                                    ReturnStatement(info.GetSystemToUnit(unitSystem, quantity.BaseDimensions)
                                        .ToExpression("_value", simplify, typeName.Symbol))
                                ])
                            )),

                        ..quantity.UnitsInfos.Select(info =>
                        {
                            var parameterName = "@" + char.ToLowerInvariant(info.Name[0]) + info.Name[1..];
                            return MethodDeclaration(
                                    IdentifierName(quantity.Name),
                                    Identifier("From" + info.Names))
                                .WithModifiers(
                                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                                .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
                                .WithXmlComment(
                                    $"/// <summary> From <see cref=\"{quantity.UnitName}.{info.Name}\"/> </summary>")
                                .WithParameterList(ParameterList(
                                [
                                    Parameter(Identifier(parameterName))
                                        .WithType(IdentifierName(typeName.FullName))
                                ]))
                                .WithBody(Block(
                                    ReturnStatement(ObjectCreationExpression(IdentifierName(quantity.Name))
                                        .WithArgumentList(ArgumentList(
                                        [
                                            Argument(IdentifierName(parameterName)),
                                            Argument(
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    IdentifierName(quantity.UnitName),
                                                    IdentifierName(info.Name)))
                                        ])))));
                        }),
                    ])
            ]);
        context.AddSource(quantity.Name + ".g.cs", nameSpace.NodeToString());
    }

    private SwitchStatementSyntax CreateSwitchStatement(
        Func<UnitInfo, IEnumerable<StatementSyntax>> getStatements)
    {
        return SwitchStatement(IdentifierName("unit"))
            .WithSections(
            [
                ..quantity.UnitsInfos.Select(info =>
                    SwitchSection().WithLabels(
                        [
                            CaseSwitchLabel(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName(quantity.UnitName),
                                    IdentifierName(info.Name)))
                        ])
                        .WithStatements(
                        [
                            ..getStatements(info),
                        ])),

                SwitchSection().WithLabels([DefaultSwitchLabel()])
                    .WithStatements(
                    [
                        ThrowStatement(ObjectCreationExpression(
                                IdentifierName("global::System.ArgumentOutOfRangeException"))
                            .WithArgumentList(
                                ArgumentList(
                                [
                                    Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("unit"))),
                                    Argument(IdentifierName("unit")),
                                    Argument(LiteralExpression(SyntaxKind.NullLiteralExpression))
                                ])))
                    ])
            ]);
    }
}