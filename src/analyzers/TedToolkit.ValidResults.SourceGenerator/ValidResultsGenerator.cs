using System.Collections.Immutable;
using TedToolkit.RoslynHelper;
using TedToolkit.RoslynHelper.Extensions;
using TedToolkit.RoslynHelper.Names;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.ValidResults.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class ValidResultsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var compilation = context.CompilationProvider;

        var attributeProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is AttributeSyntax,
                TransformAttributes);

        var classes = context.SyntaxProvider.CreateSyntaxProvider(
            static (s, _) => s is BaseTypeDeclarationSyntax,
            static (c, token) =>
                ModelExtensions.GetDeclaredSymbol(c.SemanticModel, (BaseTypeDeclarationSyntax)c.Node, token));

        context.RegisterSourceOutput(classes.Collect().Combine(attributeProvider.Collect()).Combine(compilation),
            Generate);
    }

    private static (INamedTypeSymbol Type, string Name)? TransformAttributes(GeneratorSyntaxContext ctx,
        CancellationToken token)
    {
        var semanticModel = ctx.SemanticModel;
        var attributeSyntax = (AttributeSyntax)ctx.Node;
        if (semanticModel.GetSymbolInfo(attributeSyntax, token).Symbol is not IMethodSymbol methodSymbol) return null;
        if (methodSymbol.ContainingType.GetName().FullName is not
            "global::TedToolkit.ValidResults.GenerateValidResultAttribute")
            return null;

        var constructorArguments = attributeSyntax.ArgumentList?.Arguments;
        if (constructorArguments is not { Count: > 0 }) return null;
        if (constructorArguments.Value[0].Expression is not TypeOfExpressionSyntax typeOfExpressionSyntax) return null;
        if (semanticModel.GetTypeInfo(typeOfExpressionSyntax.Type).Type is not INamedTypeSymbol typeSymbol) return null;
        if (typeSymbol.IsGenericType &&
            typeSymbol.TypeArguments.Any(t => t.TypeKind == TypeKind.Error))
        {
            typeSymbol = typeSymbol.OriginalDefinition;
        }

        var resultName = typeSymbol.Name + "Result";
        if (constructorArguments.Value.Count > 1)
        {
            var expr = constructorArguments.Value[1].Expression;
            var constantValue = semanticModel.GetConstantValue(expr);
            if (constantValue is { HasValue: true, Value: string str })
                resultName = str;
        }

        return (typeSymbol, resultName);
    }

    private static void Generate(SourceProductionContext context,
        ((ImmutableArray<ISymbol?> Sources, ImmutableArray<(INamedTypeSymbol Type, string Name)?> Types) Left,
            Compilation Compilation) arguments)
    {
        var compilation = arguments.Compilation;
        var extensionMethods = compilation.GetAllExtensionMethods();
        var dictionary = GetClassesSymbols(arguments.Left.Sources);
        var assemblyName = compilation.Assembly.Name;
        foreach (var pair in arguments.Left.Types)
        {
            if (pair is null) continue;
            var type = pair.Value.Type;
            var name = pair.Value.Name;
            dictionary[type] = new SimpleType(name, assemblyName);
        }

        foreach (var pair in dictionary)
        {
            if (pair.Key is not INamedTypeSymbol data) continue;
            if (!extensionMethods.TryGetValue(data, out var methods)) methods = [];
            try
            {
                GenerateItem(context, dictionary, pair.Value, data, methods);
            }
            catch
            {
                // ignored
            }
        }
    }

    private static void GenerateItem(SourceProductionContext context, Dictionary<ISymbol?, SimpleType> dictionary,
        SimpleType target, INamedTypeSymbol data, IEnumerable<IMethodSymbol> extensionMethods)
    {
        var members = data
            .GetMembers()
            .Where(p => p.DeclaredAccessibility is Accessibility.Public)
            .ToArray();

        var baseType =
            TypeHelper.FindValidResultType(dictionary, data, out var addDisposed, out var baseDataSymbol, false);
        var trackerName = target.Name + "Tracker";

        IEnumerable<BaseTypeSyntax> baseTypes = [SimpleBaseType(baseType)];
        if (addDisposed) baseTypes = baseTypes.Append(SimpleBaseType(IdentifierName("global::System.IDisposable")));

        IEnumerable<MemberDeclarationSyntax> disposableMembers = addDisposed
            ?
            [
                MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("Dispose"))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithAttributeLists([
                        GeneratedCodeAttribute(typeof(ValidResultsGenerator)).AddAttributes(NonUserCodeAttribute())
                    ])
                    .WithBody(Block(
                        IfStatement(IsPatternExpression(IdentifierName("ValueOrDefault"),
                                DeclarationPattern(IdentifierName("global::System.IDisposable"),
                                    SingleVariableDesignation(Identifier("dispose")))),
                            ExpressionStatement(InvocationExpression(MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("dispose"), IdentifierName("Dispose")))))))
            ]
            : [];

        var tokenList = target.IsSymbol
            ? TokenList(Token(SyntaxKind.PartialKeyword))
            : TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword));

        var parameterNames = data.IsGenericType ? data.TypeParameters.Select(p => p.GetName()).ToArray() : [];

        var node = NamespaceDeclaration(target.NameSpace)
            .WithMembers(
            [
                ClassDeclaration(target.Name)
                    .WithTypeParameterNames(parameterNames)
                    .WithModifiers(tokenList)
                    .WithBaseList(BaseList([..baseTypes]))
                    .WithXmlComment(
                        $"/// <summary>The Valid Result for <see cref=\"{data.GetName().SummaryName}\"/>.</summary>")
                    .WithMembers([
                        ..CreatorMembers(target.Name, data),
                        ..InterfacesMembers(target.Name, data, dictionary),
                        ..GenerateMembers(members, dictionary, trackerName, baseDataSymbol, parameterNames),
                        ..disposableMembers
                    ]),
                ClassDeclaration(target.Name + "Extensions")
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(ValidResultsGenerator))])
                    .WithMembers([
                        GenerateCreateTracker(IdentifierName(target.FullName).WithTypeParameterNames(parameterNames), true),
                        GenerateCreateTracker(IdentifierName(data.GetName().FullName), false),
                        MethodDeclaration(IdentifierName(target.Name).WithTypeParameterNames(parameterNames), Identifier("ToValidResult"))
                            .WithTypeParameterNames(parameterNames)
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                            .WithAttributeLists([
                                GeneratedCodeAttribute(typeof(ValidResultsGenerator))
                                    .AddAttributes(NonUserCodeAttribute(), PureAttribute())
                            ])
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("value"))
                                    .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)))
                                    .WithType(IdentifierName(data.GetName().FullName))
                            ]))
                            .WithExpressionBody(ArrowExpressionClause(InvocationExpression(MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName(target.Name).WithTypeParameterNames(parameterNames), IdentifierName("Ok")))
                                .WithArgumentList(ArgumentList(
                                [
                                    Argument(IdentifierName("value"))
                                ]))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                        ..GenerateStaticMembers(members, dictionary, trackerName, baseDataSymbol, extensionMethods, parameterNames)
                    ]),
                ClassDeclaration(trackerName)
                    .WithTypeParameterNames(parameterNames)
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(ValidResultsGenerator))])
                    .WithBaseList(BaseList([
                        SimpleBaseType(
                            GenericName(Identifier("global::TedToolkit.ValidResults.ResultTracker"))
                                .WithTypeArgumentList(TypeArgumentList([baseType])))
                    ]))
                    .WithMembers([
                        ConstructorDeclaration(Identifier(trackerName))
                            .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword)))
                            .WithAttributeLists([
                                GeneratedCodeAttribute(typeof(ValidResultsGenerator))
                                    .AddAttributes(NonUserCodeAttribute())
                            ])
                            .WithParameterList(ParameterList(
                            [
                                Parameter(Identifier("value")).WithType(baseType),
                                Parameter(Identifier("callerInfo"))
                                    .WithType(PredefinedType(Token(SyntaxKind.StringKeyword)))
                            ]))
                            .WithInitializer(ConstructorInitializer(SyntaxKind.BaseConstructorInitializer,
                                ArgumentList(
                                [
                                    Argument(IdentifierName("value")),
                                    Argument(IdentifierName("callerInfo"))
                                ])))
                            .WithBody(Block()),
                        ..GenerateStaticOperatorMembers(members, dictionary, trackerName)
                    ])
            ]);
        context.AddSource(data.GetName().SafeName + ".g.cs", node.NodeToString());
        return;

        MethodDeclarationSyntax GenerateCreateTracker(TypeSyntax type, bool isTarget)
        {
            var argument = isTarget
                ? Argument(IdentifierName("value"))
                : Argument(InvocationExpression(IdentifierName("ToValidResult").WithTypeParameterNames(parameterNames))
                    .WithArgumentList(ArgumentList(
                    [
                        Argument(IdentifierName("value"))
                    ])));

            return MethodDeclaration(IdentifierName(trackerName).WithTypeParameterNames(parameterNames),
                    Identifier("Track"))
                .WithTypeParameterNames(parameterNames)
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                .WithAttributeLists([
                    GeneratedCodeAttribute(typeof(ValidResultsGenerator))
                        .AddAttributes(NonUserCodeAttribute(), PureAttribute())
                ])
                .WithParameterList(ParameterList([
                    Parameter(Identifier("value")).WithType(type),
                    ..MethodParametersHelper.GenerateCallersSyntax(["value"])
                ]))
                .WithBody(Block(ReturnStatement(ObjectCreationExpression(
                        IdentifierName(trackerName).WithTypeParameterNames(parameterNames))
                    .WithArgumentList(ArgumentList(
                    [
                        argument,
                        Argument(InvocationExpression(MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("global::TedToolkit.ValidResults.ValidResultsExtensions"),
                                IdentifierName("GetCallerInfo")))
                            .WithArgumentList(ArgumentList(
                            [
                                Argument(IdentifierName("valueName")),
                                Argument(IdentifierName("_filePath")),
                                Argument(IdentifierName("_fileLineNumber"))
                            ])))
                    ])))
                ));
        }
    }

    private static IEnumerable<MemberDeclarationSyntax> GenerateStaticOperatorMembers(
        IReadOnlyCollection<ISymbol> members,
        Dictionary<ISymbol?, SimpleType> dictionary, string trackerName)
    {
        foreach (var method in members
                     .OfType<IMethodSymbol>()
                     .Where(p => !p.ReturnType.IsRefLikeType)
                     .Where(p => p.IsStatic)
                     .Where(p => p.MethodKind is MethodKind.UserDefinedOperator))
            yield return StaticOrdinaryMethod(method, dictionary, MethodParametersHelper.MethodType.Operator,
                trackerName, []);
    }

    private static IEnumerable<MemberDeclarationSyntax> GenerateStaticMembers(IReadOnlyCollection<ISymbol> members,
        Dictionary<ISymbol?, SimpleType> dictionary, string trackerName, ITypeSymbol? baseTypeSymbol,
        IEnumerable<IMethodSymbol> extensionMethods, IReadOnlyCollection<ITypeParamName> paramNames)
    {
        var staticMethods = baseTypeSymbol?
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => !m.IsStatic)
            .Select(m => new MethodSignature(m))
            .ToArray() ?? [];

        foreach (var method in members
                     .OfType<IMethodSymbol>()
                     .Where(p => !p.ReturnType.IsRefLikeType)
                     .Where(p => !p.IsStatic)
                     .Where(p => !staticMethods.Contains(new MethodSignature(p))))
            if (method.MethodKind is MethodKind.Ordinary)
                yield return OrdinaryMethod(method, dictionary, trackerName, paramNames);

        foreach (var method in extensionMethods
                     .Where(IsEffectivelyPublic)
                     .Where(p => !p.ReturnType.IsRefLikeType)
                     .Where(p => !staticMethods.Contains(new MethodSignature(p))))
        {
            if (method.MethodKind is MethodKind.Ordinary)
                yield return StaticOrdinaryMethod(method, dictionary, MethodParametersHelper.MethodType.Static,
                    trackerName, paramNames);
        }

        yield break;

        bool IsEffectivelyPublic(IMethodSymbol method)
        {
            if (method.DeclaredAccessibility != Accessibility.Public)
                return false;

            var containingType = method.ContainingType;
            while (containingType != null)
            {
                if (containingType.DeclaredAccessibility != Accessibility.Public)
                    return false;
                containingType = containingType.ContainingType;
            }

            return true;
        }
    }

    private static IEnumerable<MemberDeclarationSyntax> GenerateMembers(IReadOnlyCollection<ISymbol> members,
        Dictionary<ISymbol?, SimpleType> dictionary, string trackerName, ITypeSymbol? baseTypeSymbol, IReadOnlyCollection<ITypeParamName> paramNames)
    {
        var propertyOrFieldNames = baseTypeSymbol?.GetMembers().OfType<IPropertySymbol>().Select(i => i.Name)
            .Concat(baseTypeSymbol.GetMembers().OfType<IFieldSymbol>().Select(i => i.Name)).ToArray() ?? [];

        var staticMethods = baseTypeSymbol?
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.IsStatic)
            .Select(m => new MethodSignature(m))
            .ToArray() ?? [];

        foreach (var property in members
                     .OfType<IPropertySymbol>()
                     .Where(p => !p.IsStatic)
                     .Where(p => !propertyOrFieldNames.Contains(p.Name))
                     .Where(p => !p.Type.IsRefLikeType))
            yield return GenerateProperty(property.Type, property.Name, property.ContainingType,
                dictionary, property.GetMethod is not null, property.SetMethod is not null, property.Parameters);

        foreach (var field in members
                     .OfType<IFieldSymbol>()
                     .Where(p => !p.IsStatic)
                     .Where(p => !propertyOrFieldNames.Contains(p.Name))
                     .Where(p => !p.Type.IsRefLikeType))
            yield return GenerateProperty(field.Type, field.Name, field.ContainingType,
                dictionary, true, true);

        foreach (var method in members
                     .OfType<IMethodSymbol>()
                     .Where(p => !p.ReturnType.IsRefLikeType)
                     .Where(p => p.IsStatic)
                     .Where(p => !staticMethods.Contains(new MethodSignature(p))))
            if (method.MethodKind is MethodKind.Ordinary)
                yield return StaticOrdinaryMethod(method, dictionary, MethodParametersHelper.MethodType.Static,
                    trackerName, paramNames);
    }

    private static MemberDeclarationSyntax OrdinaryMethod(IMethodSymbol method,
        Dictionary<ISymbol?, SimpleType> dictionary, string trackerName, IReadOnlyCollection<ITypeParamName> paramNames)
    {
        return MethodParametersHelper.GenerateMethodByParameters(method,
        [
            new ParameterRelay(method.ContainingType, "self", RefKind.None),
            ..method.Parameters.Select(p => new ParameterRelay(p))
        ], dictionary, MethodParametersHelper.MethodType.Instance, trackerName, paramNames);
    }

    private static MemberDeclarationSyntax StaticOrdinaryMethod(IMethodSymbol method,
        Dictionary<ISymbol?, SimpleType> dictionary, MethodParametersHelper.MethodType type, string trackerName, IReadOnlyCollection<ITypeParamName> paramNames)
    {
        return MethodParametersHelper.GenerateMethodByParameters(method,
            [..method.Parameters.Select(p => new ParameterRelay(p))],
            dictionary, type, trackerName, paramNames);
    }

    private static BasePropertyDeclarationSyntax GenerateProperty(ITypeSymbol propertyType, string propertyName,
        ITypeSymbol declarationType,
        Dictionary<ISymbol?, SimpleType> dictionary, bool hasGetter, bool hasSetter,
        params IReadOnlyCollection<IParameterSymbol> parameters)
    {
        List<AccessorDeclarationSyntax> accessors = new(2);
        var isIndexer = propertyName is "this[]";

        if (hasGetter)
            accessors.Add(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithExpressionBody(ArrowExpressionClause(InvocationExpression(
                        IdentifierName("GetProperty"))
                    .WithArgumentList(ArgumentList(
                    [
                        Argument(ParenthesizedLambdaExpression()
                            .WithExpressionBody(isIndexer
                                ? ElementAccessExpression(IdentifierName("ValueOrDefault"))
                                    .WithArgumentList(BracketedArgumentList(
                                    [
                                        ..parameters.Select(p => Argument(IdentifierName(p.Name)))
                                    ]))
                                : MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression, IdentifierName("ValueOrDefault"),
                                    IdentifierName(propertyName))))
                    ]))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

        if (hasSetter)
            accessors.Add(AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                .WithExpressionBody(ArrowExpressionClause(InvocationExpression(
                        IdentifierName("SetProperty"))
                    .WithArgumentList(ArgumentList(
                    [
                        Argument(IdentifierName("value")),
                        Argument(SimpleLambdaExpression(Parameter(Identifier("v")))
                            .WithExpressionBody(AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                isIndexer
                                    ? ElementAccessExpression(IdentifierName("ValueOrDefault"))
                                        .WithArgumentList(BracketedArgumentList(
                                        [
                                            ..parameters.Select(p => Argument(IdentifierName(p.Name)))
                                        ]))
                                    : MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("ValueOrDefault"),
                                        IdentifierName(propertyName)), IdentifierName("v"))))
                    ]))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

        var memberName = propertyName is "Value" or "Result" ? propertyName + "_" : propertyName;
        var returnType = TypeHelper.FindValidResultType(dictionary, propertyType, out _, out _, true);

        BasePropertyDeclarationSyntax property;
        if (isIndexer)
        {
            property = IndexerDeclaration(returnType)
                .WithParameterList(BracketedParameterList([..parameters.Select(p => p.GetName().ParameterSyntax)]));
        }
        else
        {
            property = PropertyDeclaration(returnType, Identifier(memberName));
        }

        return property
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
            .WithAttributeLists([
                GeneratedCodeAttribute(typeof(ValidResultsGenerator)).AddAttributes(NonUserCodeAttribute())
            ])
            .WithXmlComment(
                $"/// <inheritdoc cref=\"{declarationType.GetName().SummaryName}.{(isIndexer ? $"this[{string.Join(", ", parameters.Select(p => p.GetName().Type.SummaryName))}]" : propertyName)}\"/>")
            .WithAccessorList(AccessorList([..accessors]));
    }

    private static IEnumerable<ConversionOperatorDeclarationSyntax> InterfacesMembers(string className,
        INamedTypeSymbol dataType, Dictionary<ISymbol?, SimpleType> dictionary)
    {
        foreach (var interfaceSymbol in dataType.AllInterfaces)
        {
            if (!dictionary.TryGetValue(interfaceSymbol, out var resultSymbol)) continue;
            yield return ConversionOperatorDeclaration(Token(SyntaxKind.ImplicitKeyword),
                    IdentifierName(resultSymbol.FullName))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                .WithAttributeLists([
                    GeneratedCodeAttribute(typeof(ValidResultsGenerator))
                        .AddAttributes(NonUserCodeAttribute(), PureAttribute())
                ])
                .WithParameterList(ParameterList(
                [
                    Parameter(Identifier("value")).WithType(IdentifierName(className))
                ]))
                .WithBody(Block(
                    ReturnStatement(ImplicitObjectCreationExpression()
                        .WithArgumentList(ArgumentList(
                        [
                            Argument(ImplicitObjectCreationExpression()
                                .WithArgumentList(ArgumentList(
                                [
                                    Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("value"), IdentifierName("Result"))),
                                    Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("value"), IdentifierName("ValueOrDefault")))
                                ])))
                        ])))));
        }
    }

    private static IEnumerable<MemberDeclarationSyntax> CreatorMembers(string className, INamedTypeSymbol dataType)
    {
        yield return ConstructorDeclaration(Identifier(className))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
            .WithAttributeLists([
                GeneratedCodeAttribute(typeof(ValidResultsGenerator)).AddAttributes(NonUserCodeAttribute())
            ])
            .WithParameterList(ParameterList(
            [
                Parameter(Identifier("data")).WithType(IdentifierName("Data"))
            ]))
            .WithInitializer(ConstructorInitializer(SyntaxKind.BaseConstructorInitializer,
                ArgumentList([
                    Argument(IdentifierName("data"))
                ])))
            .WithBody(Block());

        var returnType = GetReturnType();

        yield return MethodDeclaration(returnType, Identifier("Ok"))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
            .WithAttributeLists([
                GeneratedCodeAttribute(typeof(ValidResultsGenerator))
                    .AddAttributes(NonUserCodeAttribute(), PureAttribute())
            ])
            .WithParameterList(ParameterList(
            [
                Parameter(Identifier("value")).WithType(IdentifierName(dataType.GetName().FullName))
            ]))
            .WithExpressionBody(ArrowExpressionClause(InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("Data"), IdentifierName("Ok")))
                .WithArgumentList(ArgumentList(
                [
                    Argument(IdentifierName("value"))
                ]))))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        yield return ConversionOperatorDeclaration(Token(SyntaxKind.ImplicitKeyword),
                returnType)
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
            .WithAttributeLists([
                GeneratedCodeAttribute(typeof(ValidResultsGenerator))
                    .AddAttributes(NonUserCodeAttribute(), PureAttribute())
            ])
            .WithParameterList(ParameterList(
            [
                Parameter(Identifier("data"))
                    .WithType(IdentifierName("Data"))
            ]))
            .WithExpressionBody(ArrowExpressionClause(ImplicitObjectCreationExpression()
                .WithArgumentList(ArgumentList(
                [
                    Argument(IdentifierName("data"))
                ]))))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        if (dataType.TypeKind is not TypeKind.Interface)
            yield return ConversionOperatorDeclaration(Token(SyntaxKind.ImplicitKeyword),
                    returnType)
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                .WithAttributeLists([
                    GeneratedCodeAttribute(typeof(ValidResultsGenerator))
                        .AddAttributes(NonUserCodeAttribute(), PureAttribute())
                ])
                .WithParameterList(ParameterList(
                [
                    Parameter(Identifier("value")).WithType(IdentifierName(dataType.GetName().FullName))
                ]))
                .WithExpressionBody(ArrowExpressionClause(InvocationExpression(IdentifierName("Ok"))
                    .WithArgumentList(ArgumentList(
                    [
                        Argument(IdentifierName("value"))
                    ]))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        yield break;

        TypeSyntax GetReturnType()
        {
            if (!dataType.IsGenericType) return IdentifierName(className);
            return GenericName(className).WithTypeArgumentList(TypeArgumentList(
            [
                .. dataType.TypeParameters.Select(t => IdentifierName(t.GetName().SyntaxName))
            ]));
        }
    }

    private static Dictionary<ISymbol?, SimpleType> GetClassesSymbols(IEnumerable<ISymbol?> symbols)
    {
        return symbols
            .OfType<INamedTypeSymbol>()
            .Select(s => (s, s
                .GetAttributes()
                .Select(attribute => attribute.AttributeClass)
                .OfType<INamedTypeSymbol>()
                .Where(symbol => symbol.IsGenericType)
                .Where(symbol => symbol.ConstructedFrom.GetName().FullName
                    is "global::TedToolkit.ValidResults.GenerateValidResultAttribute<T>")
                .Select(symbol => symbol.TypeArguments.FirstOrDefault())
                .OfType<INamedTypeSymbol>()
                .FirstOrDefault()))
            .Where(i => i.Item2 is not null)
            .ToDictionary(i => i.Item2, i => new SimpleType(i.s),
                SymbolEqualityComparer.Default);
    }
}