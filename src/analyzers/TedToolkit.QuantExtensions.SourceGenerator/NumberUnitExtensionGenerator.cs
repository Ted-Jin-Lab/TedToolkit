using TedToolkit.RoslynHelper.Extensions;

#pragma warning disable RS1036

namespace TedToolkit.QuantExtensions.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class NumberUnitExtensionGenerator : IIncrementalGenerator

{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyAttributes = context.CompilationProvider
            .Select((compilation, _) =>
            {
                return compilation.Assembly.GetAttributes()
                    .Where(attr =>
                    {
                        if (attr.AttributeClass is not { IsGenericType: true } classType) return false;
                        return classType.ConstructUnboundGenericType().GetName().FullName is
                            "global::TedToolkit.QuantExtensions.NumberExtensionAttribute<,>";
                    });
            });


        context.RegisterSourceOutput(assemblyAttributes, Generate);
    }

    private static void Generate(SourceProductionContext context, IEnumerable<AttributeData> attributes)
    {
        //TODO: Extension Declarations.
        foreach (var attribute in attributes)
        {
            var attributeType = attribute.AttributeClass;
            if (attributeType is null) continue;
            var numberType = attributeType.TypeArguments[0].GetName();
            var unitType = attributeType.TypeArguments[1].GetName();

            var className = numberType.MiniName + "To" + unitType.MiniName + "Extensions";
            var nameSpace = NamespaceDeclaration("TedToolkit.QuantExtensions")
                .WithMembers([
                    ClassDeclaration(className)
                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                        .WithAttributeLists([GeneratedCodeAttribute(typeof(NumberUnitExtensionGenerator))])
                        .WithXmlComment()
                        .WithMembers([
                            ConstructorDeclaration(Identifier("extension"))
                                .WithParameterList(ParameterList(
                                [
                                    Parameter(Identifier("value"))
                                        .WithType(IdentifierName(numberType.FullName))
                                ]))
                                .WithBody(Block().WithCloseBraceToken(MissingToken(SyntaxKind.CloseBraceToken))),
                            ..unitType.Symbol.GetMembers()
                                .OfType<IMethodSymbol>()
                                .Where(m => m.IsStatic)
                                .Where(m => m.Name.StartsWith("From"))
                                .Where(m => m.Parameters.Length is 1)
                                .Select(GenerateProperty),
                        ])
                        .WithCloseBraceToken(
                            Token(
                                TriviaList(),
                                SyntaxKind.CloseBraceToken,
                                TriviaList(
                                    Trivia(
                                        SkippedTokensTrivia()
                                            .WithTokens(
                                                TokenList(
                                                    Token(SyntaxKind.CloseBraceToken)))))))
                ]);

            context.AddSource(className + ".g.cs", nameSpace.NodeToString());
            continue;

            PropertyDeclarationSyntax GenerateProperty(IMethodSymbol fromMethod)
            {
                var unitName = fromMethod.Name.Substring(4);
                return PropertyDeclaration(IdentifierName(unitType.FullName), Identifier(unitName))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(NumberUnitExtensionGenerator))])
                    .WithXmlCommentInheritDoc(fromMethod.GetName())
                    .WithExpressionBody(ArrowExpressionClause(
                        InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName(unitType.FullName),
                                    IdentifierName(fromMethod.Name)))
                            .WithArgumentList(ArgumentList(
                            [
                                Argument(IdentifierName("value"))
                            ]))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
            }
        }
    }
}