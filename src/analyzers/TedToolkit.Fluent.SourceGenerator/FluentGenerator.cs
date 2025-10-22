using TedToolkit.RoslynHelper.Extensions;
using TedToolkit.RoslynHelper.Names;

namespace TedToolkit.Fluent.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public class FluentGenerator : IIncrementalGenerator
{
    private const string Fluent = "global::TedToolkit.Fluent.Fluent",
        ModifyDelegate = "global::TedToolkit.Fluent.ModifyDelegate",
        FluentType = "global::TedToolkit.Fluent.FluentType";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methodInvocations = context.SyntaxProvider
            .CreateSyntaxProvider(Predicate, TransformCallings)
            .SelectMany((i, _) => i).Collect();

        var attributeProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is AttributeSyntax,
                TransformAttributes)
            .SelectMany((i, _) => i)
            .Collect();

        var attributeTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName("TedToolkit.Fluent.FluentApiAttribute",
                static (node, _) => node is BaseTypeDeclarationSyntax,
                static ITypeSymbol (ctx, _) =>
                    ctx.SemanticModel.GetDeclaredSymbol((BaseTypeDeclarationSyntax)ctx.TargetNode)!)
            .Collect();

        var merged = methodInvocations
            .Combine(attributeProvider).Select((pair, _) => pair.Left.AddRange(pair.Right))
            .Combine(attributeTypes).Select((pair, _) => pair.Left.AddRange(pair.Right));

        var allTypes = merged.Combine(context.CompilationProvider);

        context.RegisterSourceOutput(allTypes, Generate);
    }


    private static IEnumerable<ITypeSymbol> TransformAttributes(GeneratorSyntaxContext ctx, CancellationToken token)
    {
        var semanticModel = ctx.SemanticModel;
        var attributeSyntax = (AttributeSyntax)ctx.Node;
        if (semanticModel.GetSymbolInfo(attributeSyntax, token).Symbol is not IMethodSymbol methodSymbol) yield break;
        if (methodSymbol.ContainingType.GetName().FullName is not "global::TedToolkit.Fluent.FluentApiAttribute")
            yield break;

        var constructorArguments = attributeSyntax.ArgumentList?.Arguments;
        if (constructorArguments is not { Count: > 0 }) yield break;
        foreach (var typeSymbol in constructorArguments.Value
                     .Select(arg => arg.Expression)
                     .OfType<TypeOfExpressionSyntax>()
                     .Select(typeSyntax => semanticModel.GetSymbolInfo(typeSyntax.Type).Symbol as ITypeSymbol)
                     .OfType<ITypeSymbol>())
            yield return typeSymbol;
    }

    private static void Generate(SourceProductionContext context,
        (ImmutableArray<ITypeSymbol> types, Compilation compilation) arg)
    {
        var staticClasses = arg.compilation.GetAllExtensionMethods();

        foreach (var type in GetTypes(arg.types).ToImmutableHashSet(SymbolEqualityComparer.Default)
                     .OfType<ITypeSymbol>())
            Generate(context, type, arg.compilation.Assembly,
                staticClasses.TryGetValue(type.OriginalDefinition, out var methods) ? methods : []);

        return;

        static IEnumerable<ITypeSymbol> GetTypes(ImmutableArray<ITypeSymbol> types)
        {
            foreach (var type in types)
            {
                var typeSymbol = type.OriginalDefinition;
                if (typeSymbol.IsRefLikeType) continue;
                if (typeSymbol.ContainingAssembly.Name is "TedToolkit.Fluent") continue;
                yield return typeSymbol;
            }
        }
    }

    private static bool Predicate(SyntaxNode node, CancellationToken token)
    {
        if (node is not InvocationExpressionSyntax invocation) return false;
        return invocation.ArgumentList.Arguments.Count >= 0;
    }

    private static IEnumerable<ITypeSymbol> TransformCallings(GeneratorSyntaxContext context,
        CancellationToken token)
    {
        var model = context.SemanticModel;
        if (context.Node is not InvocationExpressionSyntax invocation) yield break;
        if (model.GetSymbolInfo(invocation).Symbol is not IMethodSymbol symbol) yield break;
        if (symbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) is not
            "global::TedToolkit.Fluent.FluentExtensions") yield break;

        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            if (model.GetTypeInfo(memberAccess.Expression).Type is { } type)
                yield return type;

        foreach (var arg in invocation.ArgumentList.Arguments)
            if (model.GetTypeInfo(arg.Expression).Type is { } type)
                yield return type;
    }

    private static void Generate(SourceProductionContext context, ITypeSymbol type, IAssemblySymbol? assembly,
        IEnumerable<IMethodSymbol> additionalMethods)
    {
        Dictionary<MethodSignature, Dictionary<string, int>> addedMethods = [];
        var name = type.GetName();
        var root = NamespaceDeclaration("TedToolkit.Fluent",
            $"For adding the fluent extensions of {name.FullName}").AddMembers(
            GetClass(name.SafeName).AddMembers([
                ..type.GetMembers().Where(m => !m.IsStatic).Concat(additionalMethods)
                    .SelectMany(member => GetMemberDeclarations(name, member, assembly, addedMethods))
            ]));

        context.AddSource($"{name.SafeName}.g.cs", root.NodeToString());
    }

    private static IEnumerable<MemberDeclarationSyntax> GetMemberDeclarations(TypeName typeName, ISymbol member,
        IAssemblySymbol? assembly, Dictionary<MethodSignature, Dictionary<string, int>> addedMethods)
    {
        switch (member)
        {
            case IPropertySymbol property when !member.IsStatic:
            {
                var propType = property.Type.GetName().FullName;
                var propName = property.Name;
                if (CanAccess(property.SetMethod, assembly))
                {
                    yield return SetPropertyDirect(typeName, propType, propName);
                    if (CanAccess(property.GetMethod, assembly))
                        yield return SetPropertyInvoke(typeName, propType, propName);
                }

                break;
            }
            case IFieldSymbol field when CanAccess(field, assembly) && !member.IsStatic:
            {
                var fieldType = field.Type.GetName().FullName;
                var fieldName = field.Name;
                yield return SetPropertyDirect(typeName, fieldType, fieldName);
                yield return SetPropertyInvoke(typeName, fieldType, fieldName);
                break;
            }
            case IMethodSymbol method when CanAccess(method, assembly)
                                           && method.MethodKind == MethodKind.Ordinary
                                           && (!member.IsStatic || method.IsExtensionMethod):
            {
                var key = new MethodSignature(method);
                var hasIt = true;
                if (!addedMethods.TryGetValue(key, out var dic))
                {
                    addedMethods[key] = dic = new Dictionary<string, int>();
                    hasIt = false;
                }

                yield return Invoke(method.GetName(), hasIt, dic);
                break;
            }
        }
    }

    private static TypeSyntax? GetReturnType(MethodName method)
    {
        var tuples = method.Parameters.Where(p => p.IsOut)
            .Select(p => TupleElement(IdentifierName(p.Type.FullName)).WithIdentifier(Identifier(p.Name))).ToArray();
        var isVoid = method.ReturnType.Symbol.SpecialType == SpecialType.System_Void;

        if (tuples.Length is 0)
        {
            if (isVoid) return null;
            return IdentifierName(method.ReturnType.FullName);
        }

        if (isVoid) return TupleType([..tuples]);
        return TupleType([
            TupleElement(IdentifierName(method.ReturnType.FullName)).WithIdentifier(Identifier("result")),
            ..tuples
        ]);
    }

    private static MethodDeclarationSyntax Invoke(MethodName method, bool hasIt, Dictionary<string, int> dic)
    {
        var returnType = GetReturnType(method);
        var isExtension = method.Symbol.IsExtensionMethod;
        var hostType = isExtension
            ? method.Parameters[0].Type
            : method.ContainingType;
        var methodParameters = isExtension ? method.Parameters.Skip(1).ToArray() : method.Parameters;
        var inParameters = methodParameters.Where(p => p.IsIn).ToArray();
        var isRefLike = method.Parameters.Any(p => p.Type.Symbol.IsRefLikeType);

        var types = new List<TypeSyntax>(2) { IdentifierName(hostType.FullName) };
        if (returnType is not null) types.Add(returnType);
        var thisData = isRefLike ? "_fluent.Result" : "data";
        var invocation = InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(isExtension ? method.ContainingType.FullName : thisData),
                IdentifierName(method.Name)))
            .WithArgumentList(
                ArgumentList(
                [
                    ..method.Parameters.Select((n, i) =>
                    {
                        var name = i == 0 && isExtension ? thisData : n.Name;
                        return n.Symbol.RefKind switch
                        {
                            RefKind.Ref => Argument(IdentifierName(name))
                                .WithRefOrOutKeyword(Token(SyntaxKind.RefKeyword)),
                            RefKind.Out => Argument(DeclarationExpression(IdentifierName("var"),
                                    SingleVariableDesignation(Identifier(name))))
                                .WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword)),
                            _ => Argument(IdentifierName(name))
                        };
                    })
                ]));

        List<StatementSyntax> statements = [];
        if (returnType is null)
        {
            statements.Add(ExpressionStatement(invocation));
        }
        else
        {
            var names = methodParameters.Where(p => p.IsOut).Select(i => i.Name).ToList();
            if (method.ReturnType.Symbol.SpecialType == SpecialType.System_Void)
            {
                statements.Add(ExpressionStatement(invocation));
            }
            else
            {
                statements.Add(LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
                    .WithVariables([
                        VariableDeclarator(Identifier("result")).WithInitializer(EqualsValueClause(invocation))
                    ])));
                names.Insert(0, "result");
            }

            var tuple = TupleExpression([..names.Select(n => Argument(IdentifierName(n)))]);

            statements.Add(ReturnStatement(isRefLike
                ? ImplicitObjectCreationExpression().WithArgumentList(
                    ArgumentList(
                    [
                        Argument(IdentifierName("_fluent")),
                        Argument(tuple)
                    ]))
                : tuple));
        }

        var block = Block(statements);

        var summary = method.ContainingType.SummaryName + "." + method.SummaryName;
        var parameters = inParameters.Select(n =>
            Parameter(Identifier(n.Name)).WithType(IdentifierName(n.Type.FullName)));
        if (!isRefLike)
            parameters = parameters.Append(Parameter(Identifier("_type"))
                .WithType(NullableType(IdentifierName(FluentType)))
                .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression))));

        var postFix = string.Empty;
        if (hasIt)
        {
            var className = method.ContainingType.Name;
            if (className.EndsWith("Extensions"))
                className = className.Substring(0, className.Length - "Extensions".Length);

            postFix = "_" + className;
            if (dic.TryGetValue(className, out var count))
                postFix += (dic[className] = ++count).ToString();
            else
                dic[className] = 0;
        }

        return MethodDeclaration(GenericName(Identifier("DoResult"))
                    .WithTypeArgumentList(TypeArgumentList([..types])),
                Identifier("Do" + method.Name + postFix))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
            .AddAttributes()
            .AddTypeParameters(method)
            .WithParameterList(
                ParameterList(
                [
                    Parameter(Identifier("_fluent")).WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)))
                        .WithType(GenericName(Identifier(Fluent))
                            .WithTypeArgumentList(TypeArgumentList([IdentifierName(hostType.FullName)]))),
                    ..parameters
                ]))
            .WithBody(isRefLike
                ? block
                : Block(ReturnStatement(InvocationExpression(MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression, IdentifierName("_fluent"),
                            IdentifierName("InvokeMethod")))
                        .WithArgumentList(ArgumentList([
                            Argument(IdentifierName("Invoke")), Argument(IdentifierName("_type"))
                        ]))),
                    LocalFunctionStatement(returnType ?? PredefinedType(Token(SyntaxKind.VoidKeyword)),
                            Identifier("Invoke"))
                        .WithParameterList(ParameterList([
                            Parameter(Identifier("data")).WithModifiers(
                                    TokenList(Token(SyntaxKind.RefKeyword)))
                                .WithType(IdentifierName(hostType.FullName))
                        ]))
                        .WithBody(block)))
            .WithXmlComment(
                $"""
                 /// <summary>
                 ///     🔧 Invoke the <b>Method</b> <see cref="{summary}" /> in <see cref="{method.ContainingType.SummaryName}" />
                 ///     <para>
                 ///         <inheritdoc cref="{summary}" />
                 ///     </para>
                 /// </summary>
                 /// <param name="_fluent">Self</param>
                 /// {string.Join("\n/// ", inParameters.Select(p => $"<param name=\"{p.Name}\"><inheritdoc cref=\"{summary}\"/></param>"))}
                 /// {(isRefLike ? "<remarks>⚠️<b>WARNING:</b> This method will be invoked immediately!</remarks>" : "<param name=\"_type\">Fluent type</param>")}
                 /// <returns>Self and Result</returns>
                 """);
    }

    private static MethodDeclarationSyntax SetPropertyDirect(TypeName typeName, string propertyType,
        string propertyName)
    {
        var parameter = Parameter(Identifier("value")).WithType(IdentifierName(propertyType));
        var statement = ExpressionStatement(
            AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("data"),
                    IdentifierName(propertyName)),
                IdentifierName("value")));
        const string paramSummary = "<param name=\"value\">The value to input</param>";

        return SetProperty(typeName, propertyName, parameter, paramSummary, statement);
    }

    private static MethodDeclarationSyntax SetPropertyInvoke(TypeName typeName, string propertyType,
        string propertyName)
    {
        var parameter = Parameter(Identifier("modifyValue"))
            .WithType(GenericName(Identifier(ModifyDelegate)).WithTypeArgumentList(
                TypeArgumentList([IdentifierName(propertyType)])));

        var statement = ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("data"),
                IdentifierName(propertyName)),
            InvocationExpression(IdentifierName("modifyValue"))
                .WithArgumentList(ArgumentList(
                [
                    Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("data"), IdentifierName(propertyName)))
                ]))));

        const string paramSummary = "<param name=\"modifyValue\">The method to modify it</param>";

        return SetProperty(typeName, propertyName, parameter, paramSummary, statement);
    }

    private static MethodDeclarationSyntax SetProperty(TypeName typeName,
        string propertyName, ParameterSyntax parameter, string parameterSummary, params StatementSyntax[] statements)
    {
        return MethodDeclaration(GenericName(Identifier(Fluent))
                    .WithTypeArgumentList(TypeArgumentList([IdentifierName(typeName.FullName)])),
                Identifier("With" + propertyName))
            .WithModifiers(
                TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
            .AddTypeParameters(typeName)
            .WithParameterList(ParameterList(
            [
                Parameter(Identifier("_fluent")).WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)))
                    .WithType(GenericName(Identifier(Fluent)).WithTypeArgumentList(
                        TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName(typeName.FullName))))),
                parameter,
                Parameter(Identifier("_type"))
                    .WithType(NullableType(IdentifierName(FluentType)))
                    .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression)))
            ]))
            .WithBody(Block(ReturnStatement(InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("_fluent"),
                            IdentifierName("AddProperty")))
                    .WithArgumentList(ArgumentList([
                        Argument(IdentifierName("Modify")), Argument(IdentifierName("_type"))
                    ]))),
                LocalFunctionStatement(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("Modify"))
                    .WithParameterList(ParameterList(
                    [
                        Parameter(Identifier("data")).WithModifiers(TokenList(Token(SyntaxKind.RefKeyword)))
                            .WithType(IdentifierName(typeName.FullName))
                    ]))
                    .WithBody(Block(statements))))
            .AddAttributes()
            .WithXmlComment($"""
                             /// <summary>
                             ///     🏠 Set the <b>Property</b> <see cref="{typeName.SummaryName}.{propertyName}" /> in <see cref="{typeName.SummaryName}" />
                             ///     <para>
                             ///         <inheritdoc cref="{typeName.SummaryName}.{propertyName}" />
                             ///     </para>
                             /// </summary>
                             /// <param name="_fluent">Self</param>
                             /// {parameterSummary}
                             /// <param name="_type">Fluent type</param>
                             /// <returns>Self</returns>
                             """);
    }

    private static bool CanAccess(ISymbol? symbol, IAssemblySymbol? assembly)
    {
        if (symbol is null) return false;
        var access = symbol.DeclaredAccessibility;
        if (access == Accessibility.Public) return true;
        if (assembly is not null && symbol.ContainingAssembly.Equals(assembly, SymbolEqualityComparer.Default))
            if (access is Accessibility.Internal or Accessibility.ProtectedOrInternal)
                return true;

        return false;
    }

    private static ClassDeclarationSyntax GetClass(string className)
    {
        return ClassDeclaration(className + "_Extensions")
            .WithModifiers(TokenList(
                Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.PartialKeyword)))
            .WithAttributeLists(
            [
                GeneratedCodeAttribute(typeof(FluentGenerator)).AddAttributes(NonUserCodeAttribute())
            ]);
    }
}