using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TedToolkit.RoslynHelper.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.Quantities.Generator;

public static class QuantitySystemGenerator
{
    public static void GenerateQuantitySystem(string folder, IEnumerable<(string name, string description)> systems)
    {
        var nameSpace = NamespaceDeclaration("TedToolkit.Quantities")
            .WithMembers([
                ClassDeclaration("QuantitySystems")
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantitySystemGenerator))])
                    .WithMembers(
                    [
                        ..systems.Select(system =>
                            FieldDeclaration(
                                    VariableDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)))
                                        .WithVariables(
                                        [
                                            VariableDeclarator(Identifier(system.name.Replace('-', '_')))
                                                .WithInitializer(EqualsValueClause(LiteralExpression(
                                                    SyntaxKind.StringLiteralExpression, Literal(system.name))))
                                        ]))
                                .WithModifiers(
                                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.ConstKeyword)))
                                .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantitySystemGenerator))])
                                .WithXmlComment($"/// <summary>{system.description}</summary>")
                            )
                    ])
            ]);
        
        File.WriteAllText(Path.Combine(folder, "QuantitySystems.g.cs"), nameSpace.NodeToString());
    }
}