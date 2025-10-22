using TedToolkit.RoslynHelper.Extensions;
using TedToolkit.RoslynHelper.Names;

namespace TedToolkit.InterpolatedParser.SourceGenerator;

partial class FormatGenerator
{
    private const string
        Parse = "Parse",
        CharSpan = "global::System.ReadOnlySpan<char>",
        NumberStyles = "global::System.Globalization.NumberStyles",
        FormatProvider = "global::System.IFormatProvider",
        TryParse = "TryParse",
        String = "string";

    private static ClassDeclarationSyntax? GetParserType(ITypeSymbol type, TypeName name,
        out ObjectCreationExpressionSyntax creation)
    {
        var className = "Parser_" + name.SafeName;

        var typeName = name.FullName;
        var basicClass = ClassDeclaration(className)
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.SealedKeyword)))
            .WithParameterList(ParameterList());
        creation = ObjectCreationExpression(IdentifierName(className))
            .WithArgumentList(ArgumentList());

        //TODO: number parsing.
        if (HasStaticMethod(type, Parse, CharSpan, FormatProvider)
            && HasStaticMethod(type, Parse, CharSpan, NumberStyles,
                FormatProvider)
            && HasStaticMethod(type, TryParse, CharSpan, FormatProvider,
                typeName)
            && HasStaticMethod(type, TryParse, CharSpan,
                NumberStyles, FormatProvider, typeName))
            return NumberChange(
                BaseTypeChange(basicClass, "global::TedToolkit.InterpolatedParser.Parsers.SpanParser", typeName),
                typeName, CharSpan);

        if (HasStaticMethod(type, Parse, String, FormatProvider)
            && HasStaticMethod(type, Parse, String, NumberStyles,
                FormatProvider)
            && HasStaticMethod(type, TryParse, String, FormatProvider,
                typeName)
            && HasStaticMethod(type, TryParse, String,
                NumberStyles, FormatProvider, typeName))
            return NumberChange(
                BaseTypeChange(basicClass, "global::TedToolkit.InterpolatedParser.Parsers.SpanParser", typeName),
                typeName, String);

        if (HasInterface(type, "global::System.ISpanParsable<TSelf>"))
            return BaseTypeChange(basicClass, "global::TedToolkit.InterpolatedParser.Parsers.SpanParseableParser",
                typeName);

        if (HasStaticMethod(type, Parse, CharSpan, FormatProvider)
            && HasStaticMethod(type, TryParse, CharSpan, FormatProvider,
                typeName))
            return GeneralChange(
                BaseTypeChange(basicClass, "global::TedToolkit.InterpolatedParser.Parsers.SpanParser", typeName),
                typeName, CharSpan);

        if (HasInterface(type, "global::System.IParsable<TSelf>"))
            return BaseTypeChange(basicClass, "global::TedToolkit.InterpolatedParser.Parsers.StringParseableParser",
                typeName);

        if (HasStaticMethod(type, Parse, String, FormatProvider)
            && HasStaticMethod(type, TryParse, String, FormatProvider,
                typeName))
            return GeneralChange(
                BaseTypeChange(basicClass, "global::TedToolkit.InterpolatedParser.Parsers.SpanParser", typeName),
                typeName, String);
        return null;
    }

    private static bool HasStaticMethod(ITypeSymbol typeSymbol, string methodName, params string[] argumentsName)
    {
        return typeSymbol
            .GetMembers(methodName)
            .OfType<IMethodSymbol>()
            .Any(method =>
            {
                if (!method.IsStatic) return false;
                if (method.Parameters.Length != argumentsName.Length) return false;
                return !argumentsName
                    .Where((t, i) => method.Parameters[i].Type.GetName().FullName != t).Any();
            });
    }

    private static ClassDeclarationSyntax BaseTypeChange(ClassDeclarationSyntax declaration, string baseTypeName,
        string typeName)
    {
        return declaration.WithBaseList(BaseList(
        [
            SimpleBaseType(GenericName(Identifier(baseTypeName))
                .WithTypeArgumentList(TypeArgumentList([IdentifierName(typeName)])))
        ]));
    }

    private static MethodDeclarationSyntax ParseMethod(string typeName, string inputName)
    {
        return MethodDeclaration(IdentifierName(typeName), Identifier("Parse"))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
            .WithAttributeLists([MethodAttribute()])
            .WithParameterList(
                ParameterList([
                    Parameter(Identifier("s")).WithType(IdentifierName(inputName)),
                    Parameter(Identifier("provider"))
                        .WithType(NullableType(IdentifierName("global::System.IFormatProvider")))
                ]));
    }

    private static MethodDeclarationSyntax TryParseMethod(string typeName, string inputName)
    {
        return MethodDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), Identifier("TryParse"))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
            .WithAttributeLists([MethodAttribute()])
            .WithParameterList(ParameterList(
            [
                Parameter(Identifier("s")).WithType(IdentifierName(inputName)),
                Parameter(Identifier("provider"))
                    .WithType(NullableType(IdentifierName("global::System.IFormatProvider"))),
                Parameter(Identifier("result"))
                    .WithModifiers(TokenList(Token(SyntaxKind.OutKeyword)))
                    .WithType(IdentifierName(typeName))
            ]));
    }

    private static ClassDeclarationSyntax NumberChange(ClassDeclarationSyntax declaration,
        string typeName, string inputName)
    {
        var member1 = ParseMethod(typeName, inputName)
            .WithBody(Block(ReturnStatement(ConditionalExpression(IsPatternExpression(
                    IdentifierName("Format"),
                    ConstantPattern(LiteralExpression(SyntaxKind.NullLiteralExpression))),
                InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(typeName), IdentifierName("Parse")))
                    .WithArgumentList(ArgumentList(
                    [
                        Argument(IdentifierName("s")),
                        Argument(IdentifierName("provider"))
                    ])),
                InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(typeName), IdentifierName("Parse")))
                    .WithArgumentList(ArgumentList(
                    [
                        Argument(IdentifierName("s")),
                        Argument(IdentifierName("NumberStyle")),
                        Argument(IdentifierName("provider"))
                    ]))))));

        var method2 = TryParseMethod(typeName, inputName)
            .WithBody(Block(ReturnStatement(ConditionalExpression(IsPatternExpression(IdentifierName("Format"),
                    ConstantPattern(LiteralExpression(SyntaxKind.NullLiteralExpression))),
                InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(typeName), IdentifierName("TryParse")))
                    .WithArgumentList(ArgumentList(
                    [
                        Argument(IdentifierName("s")),
                        Argument(IdentifierName("provider")),
                        Argument(IdentifierName("result")).WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword))
                    ])),
                InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(typeName), IdentifierName("TryParse")))
                    .WithArgumentList(
                        ArgumentList(
                        [
                            Argument(IdentifierName("s")),
                            Argument(IdentifierName("NumberStyle")),
                            Argument(IdentifierName("provider")),
                            Argument(IdentifierName("result")).WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword))
                        ]))))));
        return declaration.WithMembers([member1, method2]);
    }

    private static ClassDeclarationSyntax GeneralChange(ClassDeclarationSyntax declaration,
        string typeName, string inputName)
    {
        var member1 = ParseMethod(typeName, inputName)
            .WithBody(Block(ReturnStatement(
                InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(typeName), IdentifierName("Parse")))
                    .WithArgumentList(ArgumentList(
                    [
                        Argument(IdentifierName("s")),
                        Argument(IdentifierName("provider"))
                    ])))));

        var method2 = TryParseMethod(typeName, inputName)
            .WithBody(Block(ReturnStatement(
                InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(typeName), IdentifierName("TryParse")))
                    .WithArgumentList(ArgumentList(
                    [
                        Argument(IdentifierName("s")),
                        Argument(IdentifierName("provider")),
                        Argument(IdentifierName("result")).WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword))
                    ])))));
        return declaration.WithMembers([member1, method2]);
    }
}