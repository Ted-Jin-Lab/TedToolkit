using TedToolkit.RoslynHelper.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TedToolkit.ValidResults.SourceGenerator;

public class ParameterRelay(ITypeSymbol type, string name, RefKind kind, ExpressionSyntax? defaultValue = null)
{
    public ParameterRelay(IParameterSymbol symbol) : this(symbol.Type, symbol.Name, symbol.RefKind,
        symbol.GetName().DefaultValueExpression)
    {
    }

    public ITypeSymbol Type => type;
    public string Name => name;
    public RefKind Kind => kind;
    public ExpressionSyntax? DefaultValue => defaultValue;


    private SyntaxKind? Modifier => Kind switch
    {
        RefKind.Ref => SyntaxKind.RefKeyword,
        RefKind.Out => SyntaxKind.OutKeyword,
        RefKind.In => SyntaxKind.InKeyword,
        _ => null
    };

    public ExpressionStatementSyntax? AfterAssign()
    {
        if (Kind is not RefKind.Ref and not RefKind.Out) return null;
        var resultDataType = TypeHelper.GetResultDataType(Type);
        return ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
            IdentifierName(Name), ObjectCreationExpression(resultDataType)
                .WithArgumentList(ArgumentList(
                [
                    Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("__result"), IdentifierName("Result"))),
                    Argument(
                        IdentifierName("_" + Name))
                ]))));
    }

    public LocalDeclarationStatementSyntax CreateLocalDeclarationStatement(bool isTracker)
    {
        if (Type.IsRefLikeType)
            return LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
                .WithVariables([
                    VariableDeclarator(Identifier("_" + Name))
                        .WithInitializer(EqualsValueClause(IdentifierName(Name)))
                ]));

        return Kind switch
        {
            RefKind.Out => LocalDeclarationStatement(VariableDeclaration(IdentifierName(Type.GetName().FullName))
                .WithVariables([
                    VariableDeclarator(Identifier("_" + Name))
                        .WithInitializer(EqualsValueClause(PostfixUnaryExpression(
                            SyntaxKind.SuppressNullableWarningExpression,
                            LiteralExpression(SyntaxKind.DefaultLiteralExpression, Token(SyntaxKind.DefaultKeyword)))))
                ])),
            _ => LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
                .WithVariables([
                    VariableDeclarator(Identifier("_" + Name))
                        .WithInitializer(EqualsValueClause(MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression, IdentifierName(Name),
                            IdentifierName(isTracker ? "Value.ValueOrDefault" : "ValueOrDefault"))))
                ]))
        };
    }

    public ArgumentSyntax GenerateArgument()
    {
        var argument = Argument(IdentifierName("_" + Name));
        if (Modifier is { } modifier) argument = argument.WithRefOrOutKeyword(Token(modifier));

        return argument;
    }

    public ParameterSyntax GenerateParameter(bool isTracker, string trackerName, ITypeSymbol? containingType)
    {
        var parameter = Parameter(Identifier(Name));

        if (Type.IsRefLikeType)
        {
            parameter = parameter.WithType(IdentifierName(Type.GetName().FullName));
        }
        else if (isTracker)
        {
            if (Type.Equals(containingType, SymbolEqualityComparer.Default))
                parameter = parameter.WithType(IdentifierName(trackerName));
            else
                parameter = parameter.WithType(
                    GenericName(Identifier("global::TedToolkit.ValidResults.ResultTracker"))
                        .WithTypeArgumentList(TypeArgumentList(
                        [
                            GenericName(Identifier("global::TedToolkit.ValidResults.ValidResult"))
                                .WithTypeArgumentList(TypeArgumentList(
                                [
                                    IdentifierName(Type.GetName().FullName)
                                ]))
                        ])));
        }
        else
        {
            parameter = parameter.WithType(GenericName(Identifier("global::TedToolkit.ValidResults.ValidResult"))
                .WithTypeArgumentList(TypeArgumentList(
                [
                    IdentifierName(Type.GetName().FullName)
                ])));
        }


        if (Modifier is { } modifier) parameter = parameter.WithModifiers(TokenList(Token(modifier)));

        if (DefaultValue is not null)
        {
            if (Type.IsRefLikeType)
                parameter = parameter.WithDefault(
                    EqualsValueClause(
                        LiteralExpression(
                            SyntaxKind.DefaultLiteralExpression,
                            Token(SyntaxKind.DefaultKeyword))));
            else
                parameter = parameter.WithType(NullableType(parameter.Type!))
                    .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression)));
        }

        return parameter;
    }

    public ExpressionStatementSyntax? CreateDefaultValue()
    {
        if (DefaultValue is null || Type.IsRefLikeType) return null;

        return ExpressionStatement(AssignmentExpression(SyntaxKind.CoalesceAssignmentExpression,
            IdentifierName(Name),
            InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        GenericName(Identifier("global::TedToolkit.ValidResults.ValidResult"))
                            .WithTypeArgumentList(TypeArgumentList(
                            [
                                IdentifierName(Type.GetName().FullName)
                            ])),
                        IdentifierName("Data")), IdentifierName("Ok")))
                .WithArgumentList(ArgumentList(
                [
                    Argument(
                        CastExpression(
                            IdentifierName(type.GetName().FullName),
                            DefaultValue))
                ]))));
    }

    public LocalDeclarationStatementSyntax GenerateReason(out string reasonName, bool isTracker)
    {
        reasonName = "_" + name + "Reasons";

        var argumentList = isTracker
            ? ArgumentList(
            [
                Argument(IdentifierName(name))
            ])
            : ArgumentList(
            [
                Argument(IdentifierName(name)),
                Argument(IdentifierName(name + "Name")),
                Argument(IdentifierName("_filePath")),
                Argument(IdentifierName("_fileLineNumber"))
            ]);

        return LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
            .WithVariables([
                VariableDeclarator(Identifier(reasonName))
                    .WithInitializer(EqualsValueClause(InvocationExpression(MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("global::TedToolkit.ValidResults.ValidResultsExtensions"),
                            GenericName(Identifier("GetReasons"))
                                .WithTypeArgumentList(TypeArgumentList(
                                [
                                    GenericName(Identifier("global::TedToolkit.ValidResults.ValidResult"))
                                        .WithTypeArgumentList(
                                            TypeArgumentList([IdentifierName(type.GetName().FullName)]))
                                ]))))
                        .WithArgumentList(argumentList)))
            ]));
    }
}