using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TedToolkit.Quantities.Data;
using TedToolkit.RoslynHelper.Extensions;
using TedToolkit.RoslynHelper.Names;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.Quantities.Analyzer;

internal class QuantityStructGenerator(
    DataCollection data,
    Quantity quantity,
    TypeName typeName,
    UnitSystem unitSystem,
    bool isPublic,
    INamedTypeSymbol? quantitySymbol)
{
    public void GenerateCode(SourceProductionContext context)
    {
        List<MemberDeclarationSyntax> operators = [];

        if (!quantity.IsNoDimensions)
        {
            operators.Add(
                OperatorDeclaration(IdentifierName(quantity.Name), Token(SyntaxKind.AsteriskToken))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.StaticKeyword)))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                    .WithParameterList(ParameterList(
                    [
                        Parameter(Identifier("left")).WithType(IdentifierName(typeName.FullName)),
                        Parameter(Identifier("right")).WithType(IdentifierName(quantity.Name)),
                    ]))
                    .WithExpressionBody(ArrowExpressionClause(
                        BinaryExpression(SyntaxKind.MultiplyExpression,
                            IdentifierName("right"),
                            IdentifierName("left"))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
        }

        if (quantitySymbol is not null)
        {
            foreach (var attributeData in quantitySymbol.GetAttributes()
                         .Where(a => a.AttributeClass is { IsGenericType: true } attributeClass
                                     && attributeClass.ConstructUnboundGenericType().GetName().FullName is
                                         "global::TedToolkit.Quantities.QuantityOperatorAttribute<,>"))
            {
                if (attributeData.ConstructorArguments.FirstOrDefault().Value is not byte value) continue;

                if (attributeData.AttributeClass is not { TypeArguments.Length: > 1 } attributeClass) continue;

                var leftType = attributeClass.TypeArguments[0].GetName();
                var rightType = attributeClass.TypeArguments[1].GetName();
                var resultType = attributeClass.TypeArguments[2].GetName();

                operators.Add(
                    OperatorDeclaration(IdentifierName(resultType.FullName), Token(value switch
                        {
                            0 => SyntaxKind.PlusToken,
                            1 => SyntaxKind.MinusToken,
                            2 => SyntaxKind.AsteriskToken,
                            3 => SyntaxKind.SlashToken,
                            _ => SyntaxKind.None,
                        }))
                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword),
                            Token(SyntaxKind.StaticKeyword)))
                        .WithAttributeLists([
                            GeneratedCodeAttribute(typeof(QuantityStructGenerator))
                        ])
                        .WithParameterList(ParameterList(
                        [
                            Parameter(Identifier("left")).WithType(IdentifierName(leftType.Name)),
                            Parameter(Identifier("right")).WithType(IdentifierName(rightType.FullName)),
                        ]))
                        .WithExpressionBody(ArrowExpressionClause(CastExpression(IdentifierName(resultType.FullName),
                            ParenthesizedExpression(BinaryExpression(value switch
                                {
                                    0 => SyntaxKind.AddExpression,
                                    1 => SyntaxKind.SubtractExpression,
                                    2 => SyntaxKind.MultiplyExpression,
                                    3 => SyntaxKind.DivideExpression,
                                    _ => SyntaxKind.None,
                                },
                                leftType.FullName.Contains("TedToolkit.Quantities")
                                    ? MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("left"), IdentifierName("Value"))
                                    : IdentifierName("left"),
                                rightType.FullName.Contains("TedToolkit.Quantities")
                                    ? MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("right"), IdentifierName("Value"))
                                    : IdentifierName("right"))))))
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
            }
        }

        List<MemberDeclarationSyntax> mathsExtensions =
        [
            CreateMathMethod(nameof(Math.Abs)),

            MethodDeclaration(IdentifierName(quantity.Name), Identifier("Min"))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                .WithXmlCommentInheritDoc(
                    $"global::System.Math.Min({typeName.FullName}, {typeName.FullName})")
                .WithParameterList(ParameterList([
                    Parameter(Identifier("val2")).WithType(IdentifierName(quantity.Name))
                ]))
                .WithExpressionBody(ArrowExpressionClause(CastExpression(
                    IdentifierName(quantity.Name),
                    InvocationExpression(MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("global::System.Math"),
                            IdentifierName("Min")))
                        .WithArgumentList(ArgumentList(
                        [
                            Argument(IdentifierName("Value")),
                            Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("val2"), IdentifierName("Value")))
                        ])))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),

            MethodDeclaration(IdentifierName(quantity.Name), Identifier("Max"))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                .WithXmlCommentInheritDoc(
                    $"global::System.Math.Max({typeName.FullName}, {typeName.FullName})")
                .WithParameterList(ParameterList([
                    Parameter(Identifier("val2")).WithType(IdentifierName(quantity.Name))
                ]))
                .WithExpressionBody(ArrowExpressionClause(CastExpression(
                    IdentifierName(quantity.Name),
                    InvocationExpression(MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("global::System.Math"),
                            IdentifierName("Max")))
                        .WithArgumentList(ArgumentList(
                        [
                            Argument(IdentifierName("Value")),
                            Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("val2"), IdentifierName("Value")))
                        ])))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),

            PropertyDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)), Identifier("Sign"))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                .WithXmlCommentInheritDoc($"global::System.Math.Sign({typeName.FullName})")
                .WithExpressionBody(ArrowExpressionClause(InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName($"Tolerance.CurrentDefault.{quantity.Name}"),
                            IdentifierName("CompareTo")))
                    .WithArgumentList(ArgumentList(
                    [
                        Argument(CastExpression(IdentifierName(quantity.Name),
                            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0))))
                    ]))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
        ];

        if (typeName.Symbol.IsFloatingPoint())
        {
            mathsExtensions.AddRange([
                CreateMathMethod(nameof(Math.Floor)),
                CreateMathMethod(nameof(Math.Ceiling)),
                CreateMathMethod(nameof(Math.Round)),
                CreateEnumerableMethod(nameof(Enumerable.Sum), $"""
                                                                /// <summary>
                                                                /// Computes the sum of a sequence of <see cref="{quantity.Name}"/> values
                                                                /// </summary>
                                                                /// <param name="values"></param>
                                                                /// <returns>The sum of the projected values</returns>
                                                                """),
                CreateEnumerableMethod(nameof(Enumerable.Average), $"""
                                                                    /// <summary>
                                                                    /// Computes the average of a sequence of <see cref="{quantity.Name}"/> values that are obtained by invoking a transform function on each element of the input sequence
                                                                    /// </summary>
                                                                    /// <param name="values"></param>
                                                                    /// <returns>The average of the sequence of values</returns>
                                                                    """)
            ]);
        }

        var nameSpace = NamespaceDeclaration("TedToolkit.Quantities")
            .WithMembers([
                StructDeclaration(quantity.Name)
                    .WithModifiers(TokenList(Token(isPublic ? SyntaxKind.PublicKeyword : SyntaxKind.InternalKeyword),
                        Token(SyntaxKind.ReadOnlyKeyword), Token(SyntaxKind.PartialKeyword)))
                    .WithBaseList(BaseList(
                    [
                        SimpleBaseType(GenericName(Identifier("global::TedToolkit.Quantities.IQuantity"))
                            .WithTypeArgumentList(TypeArgumentList([
                                IdentifierName(quantity.Name),
                                IdentifierName(typeName.FullName)
                            ]))),
                    ]))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                    .WithXmlComment($"/// <inheritdoc cref=\"{quantity.UnitName}\"/>")
                    .WithMembers(
                    [
                        PropertyDeclaration(IdentifierName(quantity.Name), Identifier("Zero"))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                            .WithExpressionBody(ArrowExpressionClause(CastExpression(
                                IdentifierName(quantity.Name),
                                LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),

                        PropertyDeclaration(IdentifierName(quantity.Name), Identifier("One"))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                            .WithExpressionBody(ArrowExpressionClause(CastExpression(
                                IdentifierName(quantity.Name),
                                LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1)))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),

                        FieldDeclaration(
                                VariableDeclaration(IdentifierName(typeName.FullName))
                                    .WithVariables(
                                    [
                                        VariableDeclarator(Identifier("Value"))
                                    ]))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword),
                                Token(SyntaxKind.ReadOnlyKeyword)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))]),

                        ConstructorDeclaration(Identifier(quantity.Name))
                            .WithModifiers(TokenList(Token(quantity.IsNoDimensions
                                ? SyntaxKind.PublicKeyword
                                : SyntaxKind.PrivateKeyword)))
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("value"))
                                    .WithType(IdentifierName(typeName.FullName)),
                            ]))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                            .WithXmlComment()
                            .WithBody(Block(
                                ExpressionStatement(
                                    AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        IdentifierName("Value"),
                                        IdentifierName("value"))))),

                        #region Unit Conversions

                        ConstructorDeclaration(Identifier(quantity.Name))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("value"))
                                    .WithType(IdentifierName(typeName.FullName)),
                                Parameter(Identifier("unit"))
                                    .WithType(IdentifierName(quantity.UnitName))
                            ]))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                            .WithXmlComment()
                            .WithBody(Block(
                                ExpressionStatement(
                                    AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        IdentifierName("Value"), CastExpression(
                                            IdentifierName(typeName.FullName), ParenthesizedExpression(
                                                CreateSwitchStatement(info =>
                                                    info.GetUnitToSystem(unitSystem,
                                                        data.Dimensions[quantity.Dimension],
                                                        typeName.Symbol))))))
                            )),

                        MethodDeclaration(IdentifierName(typeName.FullName),
                                Identifier("As"))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("unit"))
                                    .WithType(IdentifierName(quantity.UnitName))
                            ]))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                            .WithXmlComment()
                            .WithBody(Block(
                                ReturnStatement(CastExpression(
                                    IdentifierName(typeName.FullName), ParenthesizedExpression(
                                        CreateSwitchStatement(info =>
                                            info.GetSystemToUnit(unitSystem, data.Dimensions[quantity.Dimension],
                                                typeName.Symbol)))))
                            )),

                        #endregion

                        #region ToString

                        MethodDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)), Identifier("ToString"))
                            .WithModifiers(
                                TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                            .WithXmlCommentInheritDoc((string?)null)
                            .WithExpressionBody(ArrowExpressionClause(
                                InvocationExpression(IdentifierName("ToString"))
                                    .WithArgumentList(ArgumentList(
                                    [
                                        Argument(LiteralExpression(SyntaxKind.NullLiteralExpression)),
                                        Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("global::System.Globalization.CultureInfo"),
                                            IdentifierName("CurrentCulture")))
                                    ]))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),

                        MethodDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)),
                                Identifier("ToString"))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                            .WithXmlComment()
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("format"))
                                    .WithType(NullableType(PredefinedType(Token(SyntaxKind.StringKeyword))))
                                    .WithDefault(
                                        EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression))),
                                Parameter(Identifier("formatProvider"))
                                    .WithType(NullableType(IdentifierName("global::System.IFormatProvider")))
                                    .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression)))
                            ]))
                            .WithBody(Block(
                                ExpressionStatement(AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    IdentifierName("format"),
                                    InvocationExpression(MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("global::TedToolkit.Quantities.Internal"),
                                            IdentifierName("GetFormat")))
                                        .WithArgumentList(ArgumentList(
                                        [
                                            Argument(IdentifierName("format")),
                                            Argument(DeclarationExpression(IdentifierName("var"),
                                                    SingleVariableDesignation(Identifier("index"))))
                                                .WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword))
                                        ])))),
                                ReturnStatement(BinaryExpression(SyntaxKind.AddExpression, BinaryExpression(
                                        SyntaxKind.AddExpression, InvocationExpression(MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName("Value"),
                                                IdentifierName("ToString")))
                                            .WithArgumentList(ArgumentList(
                                            [
                                                Argument(IdentifierName("format")),
                                                Argument(IdentifierName("formatProvider"))
                                            ])),
                                        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(" "))),
                                    GetSystemUnitName()
                                )))),

                        MethodDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)),
                                Identifier("ToString"))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                            .WithXmlCommentInheritDoc((string?)null)
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("unit"))
                                    .WithType(IdentifierName(quantity.UnitName)),
                                Parameter(Identifier("format"))
                                    .WithType(NullableType(PredefinedType(Token(SyntaxKind.StringKeyword))))
                                    .WithDefault(
                                        EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression))),
                                Parameter(Identifier("formatProvider"))
                                    .WithType(NullableType(IdentifierName("global::System.IFormatProvider")))
                                    .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression)))
                            ]))
                            .WithBody(Block(
                                ExpressionStatement(AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    IdentifierName("format"),
                                    InvocationExpression(MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("global::TedToolkit.Quantities.Internal"),
                                            IdentifierName("GetFormat")))
                                        .WithArgumentList(ArgumentList(
                                        [
                                            Argument(IdentifierName("format")),
                                            Argument(DeclarationExpression(IdentifierName("var"),
                                                    SingleVariableDesignation(Identifier("index"))))
                                                .WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword))
                                        ])))),
                                ReturnStatement(BinaryExpression(SyntaxKind.AddExpression, BinaryExpression(
                                        SyntaxKind.AddExpression, InvocationExpression(MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                InvocationExpression(
                                                    IdentifierName("As")).WithArgumentList(ArgumentList([
                                                    Argument(IdentifierName("unit")),
                                                ])),
                                                IdentifierName("ToString")))
                                            .WithArgumentList(ArgumentList(
                                            [
                                                Argument(IdentifierName("format")),
                                                Argument(IdentifierName("formatProvider"))
                                            ])),
                                        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(" "))),
                                    InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("unit"), IdentifierName("ToString")))
                                        .WithArgumentList(ArgumentList(
                                        [
                                            Argument(IdentifierName("index")),
                                            Argument(IdentifierName("formatProvider"))
                                        ])))))),

                        #endregion

                        ..quantity.Units.SelectMany(u =>
                        {
                            var info = data.Units[u];
                            var unitName = info.GetUnitName(data.Units.Values);
                            var parameterName = "@" + char.ToLowerInvariant(unitName[0]) + unitName[1..];
                            return (IEnumerable<MemberDeclarationSyntax>)
                            [
                                MethodDeclaration(
                                        IdentifierName(quantity.Name),
                                        Identifier("From" + unitName))
                                    .WithModifiers(
                                        TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                                    .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                                    .WithXmlComment(
                                        $"/// <summary> From <see cref=\"{quantity.UnitName}.{unitName}\"/> </summary>")
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
                                                        IdentifierName("global::TedToolkit.Quantities." +
                                                                       quantity.UnitName),
                                                        IdentifierName(unitName)))
                                            ]))))),

                                PropertyDeclaration(IdentifierName(typeName.FullName),
                                        Identifier(unitName == quantity.Name ? unitName + "_" : unitName))
                                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                                    .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                                    .WithXmlCommentInheritDoc($"{quantity.UnitName}.{unitName}")
                                    .WithExpressionBody(ArrowExpressionClause(
                                        InvocationExpression(IdentifierName("As"))
                                            .WithArgumentList(ArgumentList(
                                            [
                                                Argument(MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    IdentifierName("global::TedToolkit.Quantities." +
                                                                   quantity.UnitName),
                                                    IdentifierName(unitName)))
                                            ]))))
                                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                            ];
                        }),

                        #region Conversions

                        ConversionOperatorDeclaration(
                                Token(quantity.IsNoDimensions
                                    ? SyntaxKind.ImplicitKeyword
                                    : SyntaxKind.ExplicitKeyword),
                                IdentifierName(typeName.FullName)).WithModifiers(TokenList(
                                Token(SyntaxKind.PublicKeyword),
                                Token(SyntaxKind.StaticKeyword)))
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("quantity"))
                                    .WithType(IdentifierName(quantity.Name))
                            ]))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                            .WithXmlComment()
                            .WithExpressionBody(ArrowExpressionClause(MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression, IdentifierName("quantity"),
                                IdentifierName("Value"))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),

                        ConversionOperatorDeclaration(Token(SyntaxKind.ExplicitKeyword),
                                IdentifierName(quantity.Name))
                            .WithModifiers(
                                TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("value"))
                                    .WithType(IdentifierName(typeName.FullName))
                            ]))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                            .WithXmlComment()
                            .WithExpressionBody(ArrowExpressionClause(
                                ImplicitObjectCreationExpression()
                                    .WithArgumentList(ArgumentList(
                                    [
                                        Argument(IdentifierName("value"))
                                    ]))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),

                        ..quantity.ExactMatch.Where(data.Quantities.ContainsKey).Select(m =>
                            ConversionOperatorDeclaration(Token(SyntaxKind.ImplicitKeyword),
                                    IdentifierName(quantity.Name))
                                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword),
                                    Token(SyntaxKind.StaticKeyword)))
                                .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                                .WithParameterList(ParameterList([
                                    Parameter(Identifier("data")).WithType(IdentifierName(m))
                                ])).WithExpressionBody(
                                    ArrowExpressionClause(CastExpression(IdentifierName(quantity.Name),
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, IdentifierName("data"),
                                            IdentifierName("Value")))))
                                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))),

                        #endregion

                        #region Comparisons

                        MethodDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), Identifier("Equals"))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                            .WithXmlComment()
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("other"))
                                    .WithType(IdentifierName(quantity.Name))
                            ]))
                            .WithExpressionBody(ArrowExpressionClause(InvocationExpression(
                                    IdentifierName("Tolerance.CurrentDefault.Equals"))
                                .WithArgumentList(ArgumentList(
                                [
                                    Argument(ThisExpression()),
                                    Argument(IdentifierName("other"))
                                ]))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                        MethodDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), Identifier("Equals"))
                            .WithModifiers(
                                TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                            .WithXmlComment()
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("obj"))
                                    .WithType(NullableType(PredefinedType(Token(SyntaxKind.ObjectKeyword))))
                            ]))
                            .WithExpressionBody(ArrowExpressionClause(
                                BinaryExpression(SyntaxKind.LogicalAndExpression, IsPatternExpression(
                                        IdentifierName("obj"),
                                        DeclarationPattern(IdentifierName(quantity.Name),
                                            SingleVariableDesignation(Identifier("other")))),
                                    InvocationExpression(IdentifierName("Equals"))
                                        .WithArgumentList(ArgumentList(
                                        [
                                            Argument(IdentifierName("other"))
                                        ])))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                        MethodDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)), Identifier("GetHashCode"))
                            .WithModifiers(
                                TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                            .WithXmlComment()
                            .WithExpressionBody(
                                ArrowExpressionClause(
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("Value"),
                                            IdentifierName("GetHashCode")))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                        MethodDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)), Identifier("CompareTo"))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("other"))
                                    .WithType(IdentifierName(quantity.Name))
                            ]))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                            .WithXmlComment()
                            .WithExpressionBody(ArrowExpressionClause(InvocationExpression(
                                    IdentifierName("Tolerance.CurrentDefault.Compare"))
                                .WithArgumentList(ArgumentList(
                                [
                                    Argument(ThisExpression()),
                                    Argument(IdentifierName("other"))
                                ]))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),

                        #endregion

                        #region Operators

                        OperatorDeclaration(IdentifierName(quantity.Name), Token(SyntaxKind.MinusToken))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("value")).WithType(IdentifierName(quantity.Name))
                            ]))
                            .WithExpressionBody(ArrowExpressionClause(CastExpression(IdentifierName(quantity.Name),
                                        ParenthesizedExpression(PrefixUnaryExpression(SyntaxKind.UnaryMinusExpression,
                                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                    IdentifierName("value"), IdentifierName("Value")))))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),

                        OperatorDeclaration(IdentifierName(quantity.Name), Token(SyntaxKind.PlusToken))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("left")).WithType(IdentifierName(quantity.Name)),
                                Parameter(Identifier("right")).WithType(IdentifierName(quantity.Name))
                            ]))
                            .WithExpressionBody(ArrowExpressionClause(CastExpression(IdentifierName(quantity.Name),
                                ParenthesizedExpression(BinaryExpression(SyntaxKind.AddExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("left"),
                                        IdentifierName("Value")),
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("right"),
                                        IdentifierName("Value")))))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),

                        OperatorDeclaration(IdentifierName(quantity.Name), Token(SyntaxKind.PercentToken))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("left")).WithType(IdentifierName(quantity.Name)),
                                Parameter(Identifier("right")).WithType(IdentifierName(quantity.Name))
                            ]))
                            .WithExpressionBody(ArrowExpressionClause(CastExpression(IdentifierName(quantity.Name),
                                ParenthesizedExpression(BinaryExpression(SyntaxKind.ModuloExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("left"),
                                        IdentifierName("Value")),
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("right"),
                                        IdentifierName("Value")))))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),

                        OperatorDeclaration(IdentifierName(quantity.Name), Token(SyntaxKind.SlashToken))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword),
                                Token(SyntaxKind.StaticKeyword)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("left")).WithType(IdentifierName(quantity.Name)),
                                Parameter(Identifier("right")).WithType(IdentifierName(typeName.FullName)),
                            ]))
                            .WithExpressionBody(ArrowExpressionClause(CastExpression(IdentifierName(quantity.Name),
                                ParenthesizedExpression(BinaryExpression(SyntaxKind.DivideExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("left"),
                                        IdentifierName("Value")),
                                    IdentifierName("right"))))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),

                        OperatorDeclaration(IdentifierName(quantity.Name), Token(SyntaxKind.AsteriskToken))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword),
                                Token(SyntaxKind.StaticKeyword)))
                            .WithAttributeLists([
                                GeneratedCodeAttribute(typeof(QuantityStructGenerator))
                            ])
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("left")).WithType(IdentifierName(quantity.Name)),
                                Parameter(Identifier("right")).WithType(IdentifierName(typeName.FullName)),
                            ]))
                            .WithExpressionBody(ArrowExpressionClause(CastExpression(IdentifierName(quantity.Name),
                                ParenthesizedExpression(BinaryExpression(SyntaxKind.MultiplyExpression,
                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("left"), IdentifierName("Value")),
                                    IdentifierName("right"))))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),

                        ..operators,

                        #endregion

                        ..mathsExtensions
                    ])
            ]);
        context.AddSource(quantity.Name + ".g.cs", nameSpace.NodeToString());
        return;

        MethodDeclarationSyntax CreateMathMethod(string methodName)
        {
            return MethodDeclaration(IdentifierName(quantity.Name), Identifier(methodName))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                .WithXmlCommentInheritDoc($"global::System.Math.{methodName}({typeName.FullName})")
                .WithExpressionBody(ArrowExpressionClause(CastExpression(IdentifierName(quantity.Name),
                    InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("global::System.Math"), IdentifierName(methodName)))
                        .WithArgumentList(ArgumentList([Argument(IdentifierName("Value"))])))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        MemberDeclarationSyntax CreateEnumerableMethod(string methodName, string xml)
        {
            return MethodDeclaration(IdentifierName(quantity.Name), Identifier(methodName))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantityStructGenerator))])
                .WithXmlComment(xml)
                .WithParameterList(ParameterList(
                [
                    Parameter(Identifier("values"))
                        .WithType(GenericName(Identifier("global::System.Collections.Generic.IEnumerable"))
                            .WithTypeArgumentList(TypeArgumentList([IdentifierName(quantity.Name)])))
                ]))
                .WithExpressionBody(ArrowExpressionClause(CastExpression(IdentifierName(quantity.Name),
                    InvocationExpression(MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("global::System.Linq.Enumerable"),
                            IdentifierName(methodName)))
                        .WithArgumentList(ArgumentList(
                        [
                            Argument(IdentifierName("values")),
                            Argument(SimpleLambdaExpression(Parameter(Identifier("i")))
                                .WithExpressionBody(MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("i"), IdentifierName("Value"))))
                        ])))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }
    }

    private ExpressionSyntax GetSystemUnitName()
    {
        var allUnits = quantity.Units
            .Select(u => data.Units[u])
            .ToArray();

        if (allUnits.Length == 0)
        {
            return LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                Literal(quantity.Name));
        }

        if (quantity.IsNoDimensions)
        {
            var memberName = allUnits
                .OrderBy(u => u.DistanceToDefault)
                .First().GetUnitName(data.Units.Values);
            return FromMember(quantity.UnitName, memberName);
        }

        var dimension = data.Dimensions[quantity.Dimension];
        var systemConversion = Helpers.ToSystemConversion(unitSystem, dimension);
        if (systemConversion is not null)
        {
            var conversion = systemConversion.Value;
            var multiplier = Helpers.ToDecimal(conversion.Multiplier);
            var offset = Helpers.ToDecimal(conversion.Offset);
            var choiceUnit = allUnits
                .Where(u =>
                {
                    if (Math.Abs(Helpers.ToDecimal(u.Conversion.Multiplier) - multiplier) > 1e-9m)
                        return false;
                    if (Math.Abs(Helpers.ToDecimal(u.Conversion.Offset) - offset) > 1e-9m)
                        return false;
                    return true;
                })
                .OrderBy(u => u.DistanceToDefault)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(choiceUnit.Name))
            {
                return FromMember(quantity.UnitName, choiceUnit.GetUnitName(data.Units.Values));
            }
        }

        ExpressionSyntax? result = null;
        AddOne(dimension.AmountOfSubstance, nameof(Dimension.AmountOfSubstance));
        AddOne(dimension.ElectricCurrent, nameof(Dimension.ElectricCurrent));
        AddOne(dimension.Length, nameof(Dimension.Length));
        AddOne(dimension.LuminousIntensity, nameof(Dimension.LuminousIntensity));
        AddOne(dimension.Mass, nameof(Dimension.Mass));
        AddOne(dimension.ThermodynamicTemperature, nameof(Dimension.ThermodynamicTemperature));
        AddOne(dimension.Time, nameof(Dimension.Time));
        return result ?? LiteralExpression(
            SyntaxKind.StringLiteralExpression,
            Literal(quantity.Name));

        void AddOne(int count, string key)
        {
            if (count is 0) return;
            var unit = unitSystem.GetUnit(key);

            var member = BinaryExpression(
                SyntaxKind.AddExpression,
                FromMember(key + "Unit", unit.GetUnitName(data.Units.Values)),
                LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    Literal(count.ToSuperscript())));
            if (result is null)
            {
                result = member;
            }
            else
            {
                result = BinaryExpression(SyntaxKind.AddExpression,
                    BinaryExpression(SyntaxKind.AddExpression,
                        result, LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal("·"))), member);
            }
        }

        InvocationExpressionSyntax FromMember(string quantityName, string memberName)
        {
            return InvocationExpression(MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("global::TedToolkit.Quantities." + quantityName),
                        IdentifierName(memberName)),
                    IdentifierName("ToString")))
                .WithArgumentList(ArgumentList(
                [
                    Argument(IdentifierName("index")),
                    Argument(IdentifierName("formatProvider"))
                ]));
        }
    }

    private SwitchExpressionSyntax CreateSwitchStatement(
        Func<Unit, ExpressionSyntax> getStatements)
    {
        return SwitchExpression(IdentifierName("unit"))
            .WithArms(
            [
                ..quantity.Units.Select(u =>
                {
                    var unit = data.Units[u];
                    return SwitchExpressionArm(
                        ConstantPattern(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("global::TedToolkit.Quantities." + quantity.UnitName),
                                IdentifierName(unit.GetUnitName(data.Units.Values)))),
                        getStatements(unit));
                }),
                SwitchExpressionArm(
                    DiscardPattern(),
                    ThrowExpression(ObjectCreationExpression(
                            IdentifierName("global::System.ArgumentOutOfRangeException"))
                        .WithArgumentList(
                            ArgumentList(
                            [
                                Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("unit"))),
                                Argument(IdentifierName("unit")),
                                Argument(LiteralExpression(SyntaxKind.NullLiteralExpression))
                            ]))))
            ]);
    }
}