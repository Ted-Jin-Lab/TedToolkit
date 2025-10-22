using TedToolkit.RoslynHelper.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.Grasshopper.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public class ResourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider, (spc, compilation) =>
        {
            var rootNamespace = compilation.AssemblyName ?? "DefaultNamespace";
            var resourceName = rootNamespace + ".l10n.TedToolkit.Resources";

            var root = NamespaceDeclaration("TedToolkit.Grasshopper")
                .AddMembers(GetResourceClass(resourceName));
            spc.AddSource("TedToolkit.Resources.g.cs", root.NodeToString());
        });
    }

    private static ClassDeclarationSyntax GetResourceClass(string resourceName)
    {
        return ClassDeclaration("TedToolkitResources")
            .WithModifiers(
                TokenList(Token(SyntaxKind.PartialKeyword)))
            .WithMembers(
            [
                FieldDeclaration(VariableDeclaration(NullableType(
                            IdentifierName("global::System.Resources.ResourceManager")))
                        .WithVariables([VariableDeclarator(Identifier("_resourceManager"))]))
                    .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(ResourceGenerator))]),
                PropertyDeclaration(
                        IdentifierName("global::System.Resources.ResourceManager"),
                        Identifier("ResourceManager"))
                    .WithModifiers(
                        TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(ResourceGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithExpressionBody(ArrowExpressionClause(AssignmentExpression(
                        SyntaxKind.CoalesceAssignmentExpression,
                        IdentifierName("_resourceManager"),
                        ObjectCreationExpression(IdentifierName("global::System.Resources.ResourceManager"))
                            .WithArgumentList(ArgumentList(
                            [
                                Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(resourceName))),
                                Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    TypeOfExpression(IdentifierName("TedToolkitResources")),
                                    IdentifierName("Assembly")))
                            ])))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                PropertyDeclaration(NullableType(IdentifierName("global::System.Globalization.CultureInfo")),
                        Identifier("Culture"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(ResourceGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithAccessorList(AccessorList(
                    [
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                        AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                    ])),
                MethodDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)), Identifier("Get"))
                    .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(ResourceGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithParameterList(ParameterList(
                        [Parameter(Identifier("name")).WithType(PredefinedType(Token(SyntaxKind.StringKeyword)))]))
                    .WithExpressionBody(ArrowExpressionClause(BinaryExpression(SyntaxKind.CoalesceExpression,
                        InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("ResourceManager"), IdentifierName("GetString")))
                            .WithArgumentList(ArgumentList(
                            [
                                Argument(IdentifierName("name")),
                                Argument(IdentifierName("Culture"))
                            ])),
                        IdentifierName("name"))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),

                MethodDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)), Identifier("Loc"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword),
                        Token(SyntaxKind.PartialKeyword)))
                    .WithParameterList(
                        ParameterList(
                        [
                            Parameter(Identifier("value"))
                                .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)))
                                .WithType(PredefinedType(Token(SyntaxKind.StringKeyword))),
                            Parameter(Identifier("key"))
                                .WithType(PredefinedType(Token(SyntaxKind.StringKeyword)))
                                .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                    Literal(""))))
                        ]))
                    .WithExpressionBody(ArrowExpressionClause(InvocationExpression(IdentifierName("Get"))
                        .WithArgumentList(ArgumentList(
                        [
                            Argument(InvocationExpression(IdentifierName("GetKey"))
                                .WithArgumentList(ArgumentList(
                                [
                                    Argument(IdentifierName("key")),
                                    Argument(IdentifierName("value"))
                                ])))
                        ]))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            ]);
    }
}