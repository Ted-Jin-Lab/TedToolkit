using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TedToolkit.RoslynHelper.Extensions;
using TedToolkit.RoslynHelper.Names;
using TedToolkit.Units.Json;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.Units.Analyzer;

internal class UnitStructGenerator(
    Quantity quantity,
    TypeName typeName,
    UnitSystem unitSystem,
    bool isPublic,
    bool simplify)
{
    public void GenerateCode(SourceProductionContext context)
    {
        List<MemberDeclarationSyntax> members = [];

        if (quantity.IsNoDimensions)
        {
            members.Add(ConversionOperatorDeclaration(Token(SyntaxKind.ImplicitKeyword),
                    IdentifierName(typeName.FullName)).WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.StaticKeyword)))
                .WithParameterList(ParameterList(
                [
                    Parameter(Identifier("quantity"))
                        .WithType(IdentifierName(quantity.Name))
                ]))
                .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
                .WithXmlComment()
                .WithExpressionBody(ArrowExpressionClause(MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression, IdentifierName("quantity"), IdentifierName("_value"))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
        }

        var nameSpace = NamespaceDeclaration("TedToolkit.Units")
            .WithMembers([
                StructDeclaration(quantity.Name)
                    .WithModifiers(TokenList(Token(isPublic ? SyntaxKind.PublicKeyword : SyntaxKind.InternalKeyword),
                        Token(SyntaxKind.ReadOnlyKeyword), Token(SyntaxKind.PartialKeyword)))
                    .WithBaseList(BaseList(
                    [
                        SimpleBaseType(IdentifierName("global::System.IFormattable"))
                    ]))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
                    .WithXmlComment($"/// <inheritdoc cref=\"{quantity.UnitName}\"/>")
                    .WithMembers(
                    [
                        FieldDeclaration(
                                VariableDeclaration(IdentifierName(typeName.FullName))
                                    .WithVariables(
                                    [
                                        VariableDeclarator(Identifier("_value"))
                                    ]))
                            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword),
                                Token(SyntaxKind.ReadOnlyKeyword)))
                            .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))]),

                        ConstructorDeclaration(Identifier(quantity.Name))
                            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
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
                                        IdentifierName("_value"),
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
                                        IdentifierName("_value"),CastExpression(
                                            IdentifierName(typeName.FullName),ParenthesizedExpression(CreateSwitchStatement(info => 
                                            info.GetUnitToSystem(unitSystem, quantity.BaseDimensions)
                                                .ToExpression("value", simplify, typeName.Symbol))))))
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
                                    ReturnStatement(
                                        CastExpression(
                                            IdentifierName(typeName.FullName),ParenthesizedExpression(CreateSwitchStatement(info => 
                                        info.GetSystemToUnit(unitSystem, quantity.BaseDimensions)
                                            .ToExpression("_value", simplify, typeName.Symbol)))))
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
                                            IdentifierName("global::TedToolkit.Units.Internal"),
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
                                                IdentifierName("_value"),
                                                IdentifierName("ToString")))
                                            .WithArgumentList(ArgumentList(
                                            [
                                                Argument(IdentifierName("format")),
                                                Argument(IdentifierName("formatProvider"))
                                            ])),
                                        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(" "))),
                                    GetSystemUnitName(quantity, unitSystem))))),

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
                                            IdentifierName("global::TedToolkit.Units.Internal"),
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

                        ..quantity.UnitsInfos.Select(info =>
                        {
                            var parameterName = "@" + char.ToLowerInvariant(info.Name[0]) + info.Name[1..];
                            return MethodDeclaration(
                                    IdentifierName(quantity.Name),
                                    Identifier("From" + info.Names))
                                .WithModifiers(
                                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                                .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitStructGenerator))])
                                .WithXmlComment(
                                    $"/// <summary> From <see cref=\"{quantity.UnitName}.{info.Name}\"/> </summary>")
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
                                                    IdentifierName(info.Name)))
                                        ])))));
                        }),

                        ..members
                    ])
            ]);
        context.AddSource(quantity.Name + ".g.cs", nameSpace.NodeToString());
    }

    private static ExpressionSyntax GetSystemUnitName(Quantity quantity, UnitSystem unitSystem)
    {
        if (quantity.IsNoDimensions)
        {
            return FromMember(quantity.UnitsInfos.FirstOrDefault(i =>
                StringEquals(i.Name, quantity.BaseUnit)).Name);
        }

        var factor = 0;
        var amountOfSubstanceName = GetMetaUnitName(unitSystem.AmountOfSubstance.Name, out var relay);
        factor += quantity.BaseDimensions.N * relay;
        var electricCurrentName = GetMetaUnitName(unitSystem.ElectricCurrent.Name, out relay);
        factor += quantity.BaseDimensions.I * relay;
        var lengthName = GetMetaUnitName(unitSystem.Length.Name, out relay);
        factor += quantity.BaseDimensions.L * relay;
        var luminousIntensityName = GetMetaUnitName(unitSystem.LuminousIntensity.Name, out relay);
        factor += quantity.BaseDimensions.J * relay;
        var massName = GetMetaUnitName(unitSystem.Mass.Name, out relay);
        factor += quantity.BaseDimensions.M * relay;
        var temperatureName = GetMetaUnitName(unitSystem.Temperature.Name, out relay);
        factor += quantity.BaseDimensions.Θ * relay;
        var timeName = GetMetaUnitName(unitSystem.Time.Name, out relay);
        factor += quantity.BaseDimensions.T * relay;

        var matchedUnits = quantity.UnitsInfos.Where(u =>
        {
            var baseUnits = u.Unit.BaseUnits;

            var thisFactor = u.Prefix is Prefix.None ? 0 : u.PrefixInfo.Factor;
            if (!ShouldContinue(quantity.BaseDimensions.N, baseUnits.N, amountOfSubstanceName, ref thisFactor))
                return false;
            if (!ShouldContinue(quantity.BaseDimensions.I, baseUnits.I, electricCurrentName, ref thisFactor))
                return false;
            if (!ShouldContinue(quantity.BaseDimensions.L, baseUnits.L, lengthName, ref thisFactor))
                return false;
            if (!ShouldContinue(quantity.BaseDimensions.J, baseUnits.J, luminousIntensityName, ref thisFactor))
                return false;
            if (!ShouldContinue(quantity.BaseDimensions.M, baseUnits.M, massName, ref thisFactor))
                return false;
            if (!ShouldContinue(quantity.BaseDimensions.Θ, baseUnits.Θ, temperatureName, ref thisFactor))
                return false;
            if (!ShouldContinue(quantity.BaseDimensions.T, baseUnits.T, timeName, ref thisFactor))
                return false;

            return factor == thisFactor;

            static bool ShouldContinue(int count, string baseUnitName, string systemUnitBasicName, ref int factor)
            {
                if (count is 0) return true;
                if (string.IsNullOrEmpty(baseUnitName)) return false;
                var basicName = GetMetaUnitName(baseUnitName, out var relay);
                if (!StringEquals(systemUnitBasicName, basicName)) return false;
                factor += count * relay;
                return true;
            }
        }).ToArray();

        if (matchedUnits.Any())
        {
            return FromMember(matchedUnits
                .OrderBy(i => !StringEquals(i.Name, quantity.BaseUnit))
                .ThenByDescending(i => Helpers.GetCommonCharactersCount(i.Name, quantity.BaseUnit))
                .First().Name);
        }

        return InvocationExpression(MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName("global::TedToolkit.Units.Internal"),
                IdentifierName("GetUnitString")))
            .WithArgumentList(ArgumentList(
            [
                Argument(IdentifierName("index")),
                Argument(IdentifierName("formatProvider")),

                Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("LengthUnit"), IdentifierName(unitSystem.Length.Name))),
                Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression,
                    Literal(quantity.BaseDimensions.L))),

                Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("MassUnit"), IdentifierName(unitSystem.Mass.Name))),
                Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression,
                    Literal(quantity.BaseDimensions.M))),

                Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("DurationUnit"), IdentifierName(unitSystem.Time.Name))),
                Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression,
                    Literal(quantity.BaseDimensions.T))),

                Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("ElectricCurrentUnit"), IdentifierName(unitSystem.ElectricCurrent.Name))),
                Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression,
                    Literal(quantity.BaseDimensions.I))),

                Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("TemperatureUnit"), IdentifierName(unitSystem.Temperature.Name))),
                Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression,
                    Literal(quantity.BaseDimensions.Θ))),

                Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("AmountOfSubstanceUnit"), IdentifierName(unitSystem.AmountOfSubstance.Name))),
                Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression,
                    Literal(quantity.BaseDimensions.N))),

                Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("LuminousIntensityUnit"), IdentifierName(unitSystem.LuminousIntensity.Name))),
                Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression,
                    Literal(quantity.BaseDimensions.J))),
            ]));

        static bool StringEquals(string a, string b) =>
            string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);

        InvocationExpressionSyntax FromMember(string memberName)
        {
            return InvocationExpression(MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(quantity.UnitName),
                        IdentifierName(memberName)),
                    IdentifierName("ToString")))
                .WithArgumentList(ArgumentList(
                [
                    Argument(IdentifierName("index")),
                    Argument(IdentifierName("formatProvider"))
                ]));
        }
    }

    private static string GetMetaUnitName(string unit, out int factor)
    {
        factor = 0;
        foreach (var prefixInfo in PrefixInfo.Entries.Values.Where(i => i.Type is PrefixType.SI))
        {
            var prefixName = prefixInfo.Prefix.ToString();
            if (!unit.StartsWith(prefixName, StringComparison.InvariantCultureIgnoreCase)) continue;
            factor += prefixInfo.Factor;
            unit = unit[prefixName.Length..];
        }

        return unit;
    }

    private SwitchExpressionSyntax CreateSwitchStatement(
        Func<UnitInfo, ExpressionSyntax> getStatements)
    {
        return SwitchExpression(IdentifierName("unit"))
            .WithArms(
            [
                ..quantity.UnitsInfos.Select(info =>
                    SwitchExpressionArm(
                        ConstantPattern(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(quantity.UnitName),
                                IdentifierName(info.Name))),
                        getStatements(info))),
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

        // return SwitchStatement(IdentifierName("unit"))
        //     .WithSections(
        //     [
        //         ..quantity.UnitsInfos.Select(info =>
        //             SwitchSection().WithLabels(
        //                 [
        //                     CaseSwitchLabel(
        //                         MemberAccessExpression(
        //                             SyntaxKind.SimpleMemberAccessExpression,
        //                             IdentifierName(quantity.UnitName),
        //                             IdentifierName(info.Name)))
        //                 ])
        //                 .WithStatements(
        //                 [
        //                     ..getStatements(info),
        //                 ])),
        //
        //         SwitchSection().WithLabels([DefaultSwitchLabel()])
        //             .WithStatements(
        //             [
        //                 ThrowStatement(ObjectCreationExpression(
        //                         IdentifierName("global::System.ArgumentOutOfRangeException"))
        //                     .WithArgumentList(
        //                         ArgumentList(
        //                         [
        //                             Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("unit"))),
        //                             Argument(IdentifierName("unit")),
        //                             Argument(LiteralExpression(SyntaxKind.NullLiteralExpression))
        //                         ])))
        //             ])
        //     ]);
    }
}