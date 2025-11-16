using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TedToolkit.Quantities.Data;
using TedToolkit.RoslynHelper.Names;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.Quantities.Analyzer;

public sealed class UnitMethodExtensionGenerator(bool isPublic, TypeName typeName, DataCollection data)
    : UnitExtensionGenerator(isPublic, data)
{
    protected override string FileName => "_UnitMethodExtension";

    protected override ClassDeclarationSyntax ModifyClass(ClassDeclarationSyntax classDeclaration)
    {
        return classDeclaration.WithMembers([
            ..CreateMembers().Select(i =>
                MethodDeclaration(IdentifierName(i.quantity), Identifier(i.memberName))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitMethodExtensionGenerator))])
                    .WithXmlCommentInheritDoc($"{i.quantity}.From{i.unit}")
                    .WithParameterList(ParameterList(
                    [
                        Parameter(Identifier("value"))
                            .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)))
                            .WithType(IdentifierName(typeName.FullName))
                    ]))
                    .WithExpressionBody(ArrowExpressionClause(InvocationExpression(
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("Quantities." + i.quantity),
                                IdentifierName($"From{i.unit}")))
                        .WithArgumentList(ArgumentList(
                        [
                            Argument(IdentifierName("value"))
                        ]))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))
        ]);
    }
}