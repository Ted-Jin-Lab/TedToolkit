using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TedToolkit.Quantities.Data;
using TedToolkit.RoslynHelper.Names;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.Quantities.Analyzer;

public class UnitPropertyExtensionGenerator(bool isPublic, TypeName typeName, DataCollection data)
    : UnitExtensionGenerator(isPublic, data)
{
    protected override string FileName => "_UnitPropertyExtension";

    protected override ClassDeclarationSyntax ModifyClass(ClassDeclarationSyntax classDeclaration)
    {
        return classDeclaration.WithMembers([
                ConstructorDeclaration(Identifier("extension"))
                    .WithParameterList(ParameterList(
                    [
                        Parameter(Identifier("value"))
                            .WithType(IdentifierName(typeName.FullName))
                    ]))
                    .WithBody(Block().WithCloseBraceToken(MissingToken(SyntaxKind.CloseBraceToken))),
                ..CreateMembers().Select(pair => PropertyDeclaration(
                        IdentifierName(pair.quantity),
                        Identifier(pair.memberName))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitMethodExtensionGenerator))])
                    .WithXmlCommentInheritDoc($"{pair.quantity}.From{pair.unit}")
                    .WithExpressionBody(ArrowExpressionClause(InvocationExpression(
                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("Quantities." + pair.quantity),
                                        IdentifierName($"From{pair.unit}")))
                                .WithArgumentList(ArgumentList(
                                    [
                                        Argument(IdentifierName("value"))
                                    ]))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))),
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
                                        Token(SyntaxKind.CloseBraceToken)))))));
    }
}