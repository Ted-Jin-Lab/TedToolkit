using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TedToolkit.RoslynHelper.Extensions;
using TedToolkit.Units.Json;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.Units.Analyzer;

public sealed class ToleranceGenerator(UnitSystem unitSystem, IReadOnlyList<Quantity> quantities)
{
    public void Generate(SourceProductionContext context)
    {
        var nameSpace = NamespaceDeclaration("TedToolkit.Units")
            .WithMembers([
                ClassDeclaration("Tolerance")
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword)))
                    .WithBaseList(BaseList(
                    [
                        SimpleBaseType(GenericName(Identifier("global::TedToolkit.Scopes.ScopeBase"))
                            .WithTypeArgumentList(TypeArgumentList([IdentifierName("Tolerance")])))
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
                        CreateToleranceProperty("AmountOfSubstance"),
                        CreateToleranceProperty("ElectricCurrent"),
                        CreateToleranceProperty("Length"),
                        CreateToleranceProperty("LuminousIntensity"),
                        CreateToleranceProperty("Mass"),
                        CreateToleranceProperty("Temperature"),
                        CreateToleranceProperty("Duration"),
                        ..quantities.Select(q => CreateToleranceProperty(q.Name))
                    ])
            ]);

        context.AddSource("_Tolerance.g.cs", nameSpace.NodeToString());
    }

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
                LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1)))))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }
}