using TedToolkit.RoslynHelper.Extensions;
using TedToolkit.RoslynHelper.Names;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.ValidResults.SourceGenerator;

public static class MethodParametersHelper
{
    public enum MethodType : byte
    {
        Static,
        Instance,
        Operator
    }

    private static readonly Dictionary<string, SyntaxKind> OperatorNameToSyntaxKind = new()
    {
        ["op_Addition"] = SyntaxKind.PlusToken,
        ["op_Subtraction"] = SyntaxKind.MinusToken,
        ["op_Multiply"] = SyntaxKind.AsteriskToken,
        ["op_Division"] = SyntaxKind.SlashToken,
        ["op_Modulus"] = SyntaxKind.PercentToken,
        ["op_ExclusiveOr"] = SyntaxKind.CaretToken,
        ["op_BitwiseAnd"] = SyntaxKind.AmpersandToken,
        ["op_BitwiseOr"] = SyntaxKind.BarToken,
        ["op_LogicalAnd"] = SyntaxKind.AmpersandAmpersandToken,
        ["op_LogicalOr"] = SyntaxKind.BarBarToken,
        ["op_Assign"] = SyntaxKind.EqualsToken,
        ["op_LeftShift"] = SyntaxKind.LessThanLessThanToken,
        ["op_RightShift"] = SyntaxKind.GreaterThanGreaterThanToken,
        ["op_Equality"] = SyntaxKind.EqualsEqualsToken,
        ["op_Inequality"] = SyntaxKind.ExclamationEqualsToken,
        ["op_GreaterThan"] = SyntaxKind.GreaterThanToken,
        ["op_LessThan"] = SyntaxKind.LessThanToken,
        ["op_GreaterThanOrEqual"] = SyntaxKind.GreaterThanEqualsToken,
        ["op_LessThanOrEqual"] = SyntaxKind.LessThanEqualsToken,
        ["op_Increment"] = SyntaxKind.PlusPlusToken,
        ["op_Decrement"] = SyntaxKind.MinusMinusToken,
        ["op_UnaryNegation"] = SyntaxKind.MinusToken,
        ["op_UnaryPlus"] = SyntaxKind.PlusToken,
        ["op_OnesComplement"] = SyntaxKind.TildeToken,
        ["op_True"] = SyntaxKind.TrueKeyword,
        ["op_False"] = SyntaxKind.FalseKeyword
    };

    public static readonly Dictionary<string, SyntaxKind> OperatorNameToExpressionSyntaxKind = new()
    {
        ["op_Addition"] = SyntaxKind.AddExpression,
        ["op_Subtraction"] = SyntaxKind.SubtractExpression,
        ["op_Multiply"] = SyntaxKind.MultiplyExpression,
        ["op_Division"] = SyntaxKind.DivideExpression,
        ["op_Modulus"] = SyntaxKind.ModuloExpression,
        ["op_BitwiseAnd"] = SyntaxKind.BitwiseAndExpression,
        ["op_BitwiseOr"] = SyntaxKind.BitwiseOrExpression,
        ["op_ExclusiveOr"] = SyntaxKind.ExclusiveOrExpression,
        ["op_LeftShift"] = SyntaxKind.LeftShiftExpression,
        ["op_RightShift"] = SyntaxKind.RightShiftExpression,
        ["op_Equality"] = SyntaxKind.EqualsExpression,
        ["op_Inequality"] = SyntaxKind.NotEqualsExpression,
        ["op_GreaterThan"] = SyntaxKind.GreaterThanExpression,
        ["op_LessThan"] = SyntaxKind.LessThanExpression,
        ["op_GreaterThanOrEqual"] = SyntaxKind.GreaterThanOrEqualExpression,
        ["op_LessThanOrEqual"] = SyntaxKind.LessThanOrEqualExpression,
        ["op_LogicalAnd"] = SyntaxKind.LogicalAndExpression,
        ["op_LogicalOr"] = SyntaxKind.LogicalOrExpression,
        ["op_Assign"] = SyntaxKind.SimpleAssignmentExpression,
        ["op_Increment"] = SyntaxKind.PreIncrementExpression, // Or PostIncrementExpression
        ["op_Decrement"] = SyntaxKind.PreDecrementExpression, // Or PostDecrementExpression
        ["op_UnaryPlus"] = SyntaxKind.UnaryPlusExpression,
        ["op_UnaryNegation"] = SyntaxKind.UnaryMinusExpression,
        ["op_OnesComplement"] = SyntaxKind.BitwiseNotExpression,
        ["op_LogicalNot"] = SyntaxKind.LogicalNotExpression
    };

    public static BaseMethodDeclarationSyntax GenerateMethodByParameters(IMethodSymbol method,
        ParameterRelay[] parameters,
        Dictionary<ISymbol?, SimpleType> dictionary, MethodType type, string trackerName,
        IReadOnlyCollection<ITypeParamName> paramNames)
    {
        var resultType = TypeHelper.FindValidResultType(dictionary, method.ReturnType, out _, out _, true);
        var resultDataType = TypeHelper.GetResultDataType(method.ReturnType);

        ExpressionSyntax invocation = type switch
        {
            MethodType.Operator when parameters.Length > 1 => BinaryExpression(
                OperatorNameToExpressionSyntaxKind[method.Name], IdentifierName("_" + parameters[0].Name),
                IdentifierName("_" + parameters[1].Name)),
            MethodType.Operator => PrefixUnaryExpression(OperatorNameToExpressionSyntaxKind[method.Name],
                IdentifierName("_" + parameters[0].Name)),
            MethodType.Static => InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(method.ContainingType.GetName().FullName), IdentifierName(method.Name).WithTypeParameterNames([
                        ..method.TypeParameters.Select(p => p.GetName())
                    ])))
                .WithArgumentList(ArgumentList([..parameters.Select(p => p.GenerateArgument())])),
            _ => InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("_" + parameters[0].Name), IdentifierName(method.Name).WithTypeParameterNames([
                        ..method.TypeParameters.Select(p => p.GetName())
                    ])))
                .WithArgumentList(ArgumentList([..parameters.Skip(1).Select(p => p.GenerateArgument())]))
        };

        StatementSyntax[] statements;

        if (method.ReturnType.SpecialType is SpecialType.System_Void)
            statements =
            [
                ExpressionStatement(invocation),
                ExpressionStatement(AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression, IdentifierName("__result"),
                    InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        resultDataType,
                        IdentifierName("Ok")))))
            ];
        else
            statements =
            [
                ExpressionStatement(AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression, IdentifierName("__result"),
                    InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            resultDataType,
                            IdentifierName("Ok")))
                        .WithArgumentList(ArgumentList(
                        [
                            Argument(invocation)
                        ]))))
            ];

        var generateThis = type is MethodType.Instance;
        var isTracker = type is not MethodType.Instance and not MethodType.Static;


        BaseMethodDeclarationSyntax methodDeclaration = type is MethodType.Operator
            ? OperatorDeclaration(resultType, Token(OperatorNameToSyntaxKind[method.Name]))
            : MethodDeclaration(resultType, Identifier(method.Name))
                .WithTypeParameterNames([
                    ..paramNames,
                    ..method.TypeParameters.Select(p => p.GetName())
                ]);

        return methodDeclaration
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
            .WithAttributeLists([
                GeneratedCodeAttribute(typeof(MethodParametersHelper)).AddAttributes(NonUserCodeAttribute())
            ])
            .WithXmlComment(method)
            .WithParameterList(ParameterList([
                ..GenerateParametersWithCaller(parameters, generateThis, isTracker, trackerName, method.ContainingType)
            ]))
            .WithBody(Block((StatementSyntax[])
            [
                ..parameters.Select(p => p.CreateDefaultValue()).OfType<ExpressionStatementSyntax>(),
                ..GenerateReasons(parameters, out var reasonNames, isTracker),
                ..parameters.Select(p => p.CreateLocalDeclarationStatement(isTracker)),
                LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
                    .WithVariables(
                    [
                        VariableDeclarator(Identifier("__result"))
                            .WithInitializer(EqualsValueClause(InvocationExpression(
                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        resultDataType, IdentifierName("Fail")))
                                .WithArgumentList(ArgumentList(
                                [
                                    Argument(CollectionExpression(
                                    [
                                        ..reasonNames.Select(name => SpreadElement(IdentifierName(name)))
                                    ]))
                                ]))))
                    ])),
                IfStatement(IsPatternExpression(IdentifierName("__result"),
                        ConstantPattern(LiteralExpression(SyntaxKind.NullLiteralExpression))),
                    Block(
                        IfStatement(IsPatternExpression(MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("global::TedToolkit.ValidResults.ValidResultsConfig"),
                                        IdentifierName("ExceptionHandler")),
                                    UnaryPattern(RecursivePattern().WithPropertyPatternClause(PropertyPatternClause())
                                        .WithDesignation(SingleVariableDesignation(Identifier("__handler"))))),
                                Block(statements))
                            .WithElse(ElseClause(Block(
                                TryStatement(
                                    [
                                        CatchClause().WithDeclaration(
                                                CatchDeclaration(IdentifierName("global::System.Exception"))
                                                    .WithIdentifier(Identifier("ex")))
                                            .WithBlock(Block(
                                                ExpressionStatement(AssignmentExpression(
                                                    SyntaxKind.SimpleAssignmentExpression,
                                                    IdentifierName("__result"), PostfixUnaryExpression(
                                                        SyntaxKind.SuppressNullableWarningExpression,
                                                        InvocationExpression(
                                                                MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    resultDataType, IdentifierName("Fail")))
                                                            .WithArgumentList(ArgumentList(
                                                            [
                                                                Argument(CollectionExpression(
                                                                [
                                                                    ExpressionElement(
                                                                        InvocationExpression(
                                                                                IdentifierName("__handler"))
                                                                            .WithArgumentList(ArgumentList(
                                                                            [
                                                                                Argument(IdentifierName("ex"))
                                                                            ])))
                                                                ]))
                                                            ])))))))
                                    ])
                                    .WithBlock(Block(statements))
                            )))
                    )),
                ..parameters.Select(p => p.AfterAssign()).OfType<ExpressionStatementSyntax>(),
                ReturnStatement(IdentifierName("__result"))
            ]));
    }

    #region Body

    private static IEnumerable<LocalDeclarationStatementSyntax> GenerateReasons(
        IReadOnlyCollection<ParameterRelay> parameters, out List<string> reasonNames, bool isTracker)
    {
        List<LocalDeclarationStatementSyntax> result = new(parameters.Count);
        reasonNames = new List<string>(parameters.Count);
        foreach (var item in parameters
                     .Where(i => i.Kind is not RefKind.Out)
                     .Where(i => !i.Type.IsRefLikeType))
        {
            result.Add(item.GenerateReason(out var reasonName, isTracker));
            reasonNames.Add(reasonName);
        }

        return result;
    }

    #endregion

    #region Parameters

    private static IEnumerable<ParameterSyntax> GenerateParametersWithCaller(
        IReadOnlyCollection<ParameterRelay> parameterNames, bool generateThis, bool isTracker, string trackerName,
        ITypeSymbol? containingType)
    {
        foreach (var item in parameterNames)
        {
            var parameter = item.GenerateParameter(isTracker, trackerName, containingType);
            if (generateThis)
            {
                parameter = parameter.WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)));
                generateThis = false;
            }

            yield return parameter;
        }

        if (isTracker) yield break;

        foreach (var parameter in GenerateCallersSyntax(parameterNames
                     .Where(i => i.Kind is not RefKind.Out)
                     .Where(i => !i.Type.IsRefLikeType)
                     .Select(i => i.Name)))
            yield return parameter;
    }

    public static IEnumerable<ParameterSyntax> GenerateCallersSyntax(IEnumerable<string> parameterNames)
    {
        foreach (var parameterName in parameterNames) yield return GenerateCallerSyntax(parameterName);

        foreach (var caller in GenerateFileCallers()) yield return caller;
    }

    private static IEnumerable<ParameterSyntax> GenerateFileCallers()
    {
        yield return Parameter(Identifier("_filePath"))
            .WithAttributeLists(
            [
                AttributeList(
                [
                    Attribute(IdentifierName("global::System.Runtime.CompilerServices.CallerFilePath"))
                ])
            ])
            .WithType(PredefinedType(Token(SyntaxKind.StringKeyword)))
            .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(""))));

        yield return Parameter(Identifier("_fileLineNumber"))
            .WithAttributeLists(
            [
                AttributeList(
                [
                    Attribute(IdentifierName("global::System.Runtime.CompilerServices.CallerLineNumber"))
                ])
            ])
            .WithType(PredefinedType(Token(SyntaxKind.IntKeyword)))
            .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0))));
    }

    private static ParameterSyntax GenerateCallerSyntax(string parameterName)
    {
        return Parameter(Identifier(parameterName + "Name"))
            .WithAttributeLists(
            [
                AttributeList(
                [
                    Attribute(IdentifierName("global::System.Runtime.CompilerServices.CallerArgumentExpression"))
                        .WithArgumentList(AttributeArgumentList(
                        [
                            AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                Literal(parameterName)))
                        ]))
                ])
            ])
            .WithType(PredefinedType(Token(SyntaxKind.StringKeyword)))
            .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(""))));
    }

    #endregion
}