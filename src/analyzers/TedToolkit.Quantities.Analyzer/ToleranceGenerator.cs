using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TedToolkit.Quantities.Data;
using TedToolkit.RoslynHelper.Extensions;
using TedToolkit.RoslynHelper.Names;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.Quantities.Analyzer;

public sealed class ToleranceGenerator(
    UnitSystem unitSystem,
    IReadOnlyList<Quantity> quantities,
    bool isPublic,
    TypeName typeName)
{
    public void Generate(SourceProductionContext context)
    {
        var nameSpace = NamespaceDeclaration("TedToolkit.Quantities")
            .WithMembers([
                ClassDeclaration("Tolerance")
                    .WithModifiers(TokenList(Token(isPublic ? SyntaxKind.PublicKeyword : SyntaxKind.InternalKeyword),
                        Token(SyntaxKind.PartialKeyword)))
                    .WithBaseList(BaseList(
                    [
                        SimpleBaseType(GenericName(Identifier("global::TedToolkit.Scopes.ScopeBase"))
                            .WithTypeArgumentList(TypeArgumentList([IdentifierName("Tolerance")]))),
                        ..quantities.Select(q => SimpleBaseType(
                            GenericName(Identifier("global::System.Collections.Generic.IEqualityComparer"))
                                .WithTypeArgumentList(TypeArgumentList([IdentifierName(q.Name)])))),
                        ..quantities.Select(q => SimpleBaseType(
                            GenericName(Identifier("global::System.Collections.Generic.IComparer"))
                                .WithTypeArgumentList(TypeArgumentList([IdentifierName(q.Name)])))),
                    ]))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(ToleranceGenerator))])
                    .WithXmlComment()
                    .WithMembers([
                        PropertyDeclaration(IdentifierName("Tolerance"), Identifier("CurrentDefault"))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                            .WithExpressionBody(ArrowExpressionClause(BinaryExpression(
                                SyntaxKind.CoalesceExpression, IdentifierName("Current"),
                                ObjectCreationExpression(IdentifierName("Tolerance"))
                                    .WithArgumentList(ArgumentList()))))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(ToleranceGenerator))])
                            .WithXmlComment()
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                        ..quantities.Select(q => CreateToleranceProperty(q.Name)),
                        ..quantities.Select(q =>
                            MethodDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), Identifier("Equals"))
                                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                                .WithAttributeLists([GeneratedCodeAttribute(typeof(ToleranceGenerator))])
                                .WithParameterList(ParameterList(
                                [
                                    Parameter(Identifier("x")).WithType(IdentifierName(q.Name)),
                                    Parameter(Identifier("y")).WithType(IdentifierName(q.Name))
                                ]))
                                .WithBody(Block(
                                    ReturnStatement(BinaryExpression(SyntaxKind.LessThanExpression,
                                        CastExpression(
                                            IdentifierName(typeName.FullName), InvocationExpression(
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        IdentifierName("global::System.Math"), IdentifierName("Abs")))
                                                .WithArgumentList(ArgumentList(
                                                [
                                                    Argument(BinaryExpression(SyntaxKind.SubtractExpression,
                                                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("x"), IdentifierName("Value")),
                                                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("y"), IdentifierName("Value"))))
                                                ]))),
                                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName(q.Name), IdentifierName("Value"))))))),
                        ..quantities.Select(q => MethodDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)),
                                Identifier("GetHashCode"))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(ToleranceGenerator))])
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("obj")).WithType(IdentifierName(q.Name))
                            ]))
                            .WithExpressionBody(ArrowExpressionClause(InvocationExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("obj"), IdentifierName("Value")),
                                    IdentifierName("GetHashCode")))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))),
                        ..quantities.Select(q =>
                            MethodDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)), Identifier("Compare"))
                                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                                .WithAttributeLists([GeneratedCodeAttribute(typeof(ToleranceGenerator))])
                                .WithParameterList(ParameterList(
                                [
                                    Parameter(Identifier("x")).WithType(IdentifierName(q.Name)),
                                    Parameter(Identifier("y")).WithType(IdentifierName(q.Name))
                                ]))
                                .WithBody(Block(
                                    ReturnStatement(ConditionalExpression(InvocationExpression(IdentifierName("Equals"))
                                            .WithArgumentList(ArgumentList(
                                            [
                                                Argument(IdentifierName("x")),
                                                Argument(IdentifierName("y"))
                                            ])),
                                        LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)),
                                        InvocationExpression(MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                    IdentifierName("x"), IdentifierName("Value")),
                                                IdentifierName("CompareTo")))
                                            .WithArgumentList(
                                                ArgumentList(
                                                [
                                                    Argument(MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        IdentifierName("y"),
                                                        IdentifierName("Value")))
                                                ]))))))),
                        ..quantities.Select(q => ConversionOperatorDeclaration(
                                Token(SyntaxKind.ImplicitKeyword),
                                IdentifierName(q.Name))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(ToleranceGenerator))])
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("tolerance"))
                                    .WithType(IdentifierName("Tolerance"))
                            ]))
                            .WithExpressionBody(ArrowExpressionClause(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("tolerance"), IdentifierName(q.Name))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))),
                    ])
            ]);

        context.AddSource("_Tolerance.g.cs", nameSpace.NodeToString());
    }

    //TODO: Quantity Default Tolerance by units.
    private static PropertyDeclarationSyntax CreateToleranceProperty(string name)
    {
        return PropertyDeclaration(IdentifierName(name), Identifier(name))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
            .WithAttributeLists([GeneratedCodeAttribute(typeof(ToleranceGenerator))])
            .WithAccessorList(AccessorList(
            [
                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                AccessorDeclaration(SyntaxKind.InitAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            ]))
            .WithInitializer(EqualsValueClause(CastExpression(
                IdentifierName(name),
                LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1E-6)))))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }
}