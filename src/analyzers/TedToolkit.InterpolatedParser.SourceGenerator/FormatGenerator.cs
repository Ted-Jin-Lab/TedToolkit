using TedToolkit.RoslynHelper.Extensions;

namespace TedToolkit.InterpolatedParser.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public partial class FormatGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methodInvocations = context.SyntaxProvider
            .CreateSyntaxProvider(Predicate, TransForm).Collect();

        context.RegisterSourceOutput(methodInvocations, Generate);
    }

    private static bool Predicate(SyntaxNode node, CancellationToken token)
    {
        if (node is not InvocationExpressionSyntax invocation) return false;
        return invocation.ArgumentList.Arguments.Count > 0;
    }

    private static IEnumerable<TypeInfo> TransForm(GeneratorSyntaxContext context, CancellationToken token)
    {
        var model = context.SemanticModel;
        if (context.Node is not InvocationExpressionSyntax invocation) yield break;
        if (model.GetSymbolInfo(invocation).Symbol is not IMethodSymbol symbol) yield break;
        if (symbol.ContainingType.GetName().FullName is not
            "global::TedToolkit.InterpolatedParser.InterpolatedParserExtensions") yield break;

        foreach (var arg in invocation.ArgumentList.Arguments)
        {
            if (arg.Expression is not InterpolatedStringExpressionSyntax interpolatedString) continue;
            foreach (var content in interpolatedString.Contents)
            {
                if (content is not InterpolationSyntax interpolation) continue;
                yield return model.GetTypeInfo(interpolation.Expression);
            }
        }
    }

    private static void Generate(SourceProductionContext context, ImmutableArray<IEnumerable<TypeInfo>> typeInfos)
    {
        ParseItem[] items =
        [
            ..
            from typeSymbol in typeInfos.SelectMany(infos => infos).ToImmutableHashSet()
                .Select(typeInfo => typeInfo.Type).OfType<ITypeSymbol>()
            let typeValue = GetCollectionElementType(typeSymbol)
            select new ParseItem(typeSymbol, typeValue)
        ];
        Generate(context, items);
        return;

        static ITypeSymbol? GetCollectionElementType(ITypeSymbol typeSymbol)
        {
            var iCollectionInterface = typeSymbol.AllInterfaces
                .FirstOrDefault(i =>
                    i.OriginalDefinition.GetName().FullName == "global::System.Collections.Generic.ICollection<T>");
            return iCollectionInterface?.TypeArguments.FirstOrDefault();
        }
    }

    private static bool HasInterface(ITypeSymbol typeSymbol, string interfaceName)
    {
        return typeSymbol.AllInterfaces.Any(i => i.OriginalDefinition.GetName().FullName == interfaceName);
    }

    private static void Generate(SourceProductionContext context, ParseItem[] items)
    {
        var validTypes = items
            .SelectMany(i => (ITypeSymbol?[]) [i.Type, i.SubType])
            .ToImmutableHashSet(SymbolEqualityComparer.Default)
            .OfType<ITypeSymbol>()
            .Select(t =>
            {
                var result = Generate(context, t, out var className);
                return (result, t, className);
            })
            .Where(t => t is { result: true, t: not null })
            .ToImmutableDictionary<(bool, ITypeSymbol, ObjectCreationExpressionSyntax), ITypeSymbol,
                ObjectCreationExpressionSyntax>(
                i => i.Item2,
                i => i.Item3,
                SymbolEqualityComparer.Default);

        foreach (var item in items)
        {
            var metadataName = item.Type.GetName();
            if (metadataName.TypeParameters.Length > 0) continue;
            var methodName = "Add_" + metadataName.SafeName;
            var root = NamespaceDeclaration("TedToolkit.InterpolatedParser",
                    $"For adding the formatted type of {metadataName.FullName}")
                .AddMembers(BasicStruct(metadataName.FullName, methodName, item, validTypes));
            context.AddSource($"Format.{metadataName.SafeName}.g.cs", root.NodeToString());
        }
    }

    private static bool Generate(SourceProductionContext context, ITypeSymbol type,
        out ObjectCreationExpressionSyntax creation)
    {
        var name = type.GetName();
        if (name.HasTypeParameters)
        {
            creation = null!;
            return false;
        }

        var classDeclaration = GetParserType(type, name, out creation);
        if (classDeclaration == null) return false;

        var root = NamespaceDeclaration("TedToolkit.InterpolatedParser",
                $"For adding the formatted type of {name.FullName}")
            .AddMembers(ClassDeclaration("InterpolatedParseStringHandler")
                .WithModifiers(
                    TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.PartialKeyword)))
                .AddMembers(classDeclaration.WithAttributeLists([
                    GeneratedCodeAttribute(typeof(FormatGenerator)).AddAttributes(NonUserCodeAttribute())
                ]))
            );

        context.AddSource($"Parser.{name.SafeName}.g.cs", root.NodeToString());
        return true;
    }

    private static AttributeListSyntax MethodAttribute()
    {
        return GeneratedCodeAttribute(typeof(FormatGenerator)).AddAttributes(NonUserCodeAttribute());
    }

    private readonly struct ParseItem(ITypeSymbol type, ITypeSymbol? subType)
    {
        public ITypeSymbol Type => type;
        public ITypeSymbol? SubType => subType;
    }
}