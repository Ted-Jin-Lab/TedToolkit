using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TedToolkit.Quantities.Data;
using TedToolkit.RoslynHelper.Extensions;
using TedToolkit.RoslynHelper.Names;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.Quantities.Analyzer;

internal class UnitStructGenerator(
    DataCollection data,
    Quantity quantity,
    TypeName typeName,
    UnitSystem unitSystem,
    bool isPublic)
{
    public void GenerateCode(SourceProductionContext context)
    {
        List<MemberDeclarationSyntax> members = [];

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
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
                    .WithXmlComment($"/// <inheritdoc cref=\"{quantity.UnitName}\"/>")
                    .WithMembers(
                    [
                        FieldDeclaration(
                                VariableDeclaration(IdentifierName(typeName.FullName))
                                    .WithVariables(
                                    [
                                        VariableDeclarator(Identifier("Value"))
                                    ]))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword),
                                Token(SyntaxKind.ReadOnlyKeyword)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))]),

                        ConstructorDeclaration(Identifier(quantity.Name))
                            .WithModifiers(TokenList(Token(quantity.IsNoDimensions
                                ? SyntaxKind.PublicKeyword
                                : SyntaxKind.PrivateKeyword)))
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("value"))
                                    .WithType(IdentifierName(typeName.FullName)),
                            ]))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
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
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
                            .WithXmlComment()
                            .WithBody(Block(
                                ExpressionStatement(
                                    AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        IdentifierName("Value"), CastExpression(
                                            IdentifierName(typeName.FullName), ParenthesizedExpression(
                                                CreateSwitchStatement(info =>
                                                    info.GetUnitToSystem(unitSystem, quantity.Dimension,
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
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
                            .WithXmlComment()
                            .WithBody(Block(
                                ReturnStatement(CastExpression(
                                    IdentifierName(typeName.FullName), ParenthesizedExpression(
                                        CreateSwitchStatement(info =>
                                            info.GetSystemToUnit(unitSystem, quantity.Dimension,
                                                typeName.Symbol)))))
                            )),

                        #endregion

                        #region ToString

                        MethodDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)), Identifier("ToString"))
                            .WithModifiers(
                                TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
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
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
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
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
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
                                    .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
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
                                                        IdentifierName(quantity.UnitName),
                                                        IdentifierName(unitName)))
                                            ]))))),

                                PropertyDeclaration(IdentifierName(typeName.FullName),
                                        Identifier(unitName == quantity.Name ? unitName + "_" : unitName))
                                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                                    .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
                                    .WithXmlCommentInheritDoc($"{quantity.UnitName}.{unitName}")
                                    .WithExpressionBody(ArrowExpressionClause(
                                        InvocationExpression(IdentifierName("As"))
                                            .WithArgumentList(ArgumentList(
                                            [
                                                Argument(MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    IdentifierName(quantity.UnitName),
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
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
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
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
                            .WithXmlComment()
                            .WithExpressionBody(ArrowExpressionClause(
                                ImplicitObjectCreationExpression()
                                    .WithArgumentList(ArgumentList(
                                    [
                                        Argument(IdentifierName("value"))
                                    ]))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),

                        #endregion

                        #region Comparisons

                        MethodDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), Identifier("Equals"))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
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
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
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
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
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
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
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

                        ..members
                    ])
            ]);
        context.AddSource(quantity.Name + ".g.cs", nameSpace.NodeToString());
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

        var systemConversion = Helpers.ToSystemConversion(unitSystem, quantity.Dimension);
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
        AddOne(quantity.Dimension.AmountOfSubstance, nameof(Dimension.AmountOfSubstance));
        AddOne(quantity.Dimension.ElectricCurrent, nameof(Dimension.ElectricCurrent));
        AddOne(quantity.Dimension.Length, nameof(Dimension.Length));
        AddOne(quantity.Dimension.LuminousIntensity, nameof(Dimension.LuminousIntensity));
        AddOne(quantity.Dimension.Mass, nameof(Dimension.Mass));
        AddOne(quantity.Dimension.ThermodynamicTemperature, nameof(Dimension.ThermodynamicTemperature));
        AddOne(quantity.Dimension.Time, nameof(Dimension.Time));
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
                        IdentifierName(quantityName),
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
                                IdentifierName(quantity.UnitName),
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