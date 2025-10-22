using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using TedToolkit.RoslynHelper.Extensions;
using TedToolkit.RoslynHelper.Names;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TedToolkit.Grasshopper.SourceGenerator;

public class CateInfo
{
    public string ShortName { get; set; } = string.Empty;
    public char? SymbolName { get; set; }
}

public readonly struct LocInfo(string key, string value)
{
    public string Key => key;
    public string Value => value;
}

[Generator(LanguageNames.CSharp)]
[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers")]
public class DocumentObjectGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var attributeTypes =
            context.SyntaxProvider.ForAttributeWithMetadataName("TedToolkit.Grasshopper.DocObjAttribute",
                static (node, _) => node is BaseTypeDeclarationSyntax,
                static (context, _) => new TypeGenerator(context.TargetSymbol));

        var attributeMethods =
            context.SyntaxProvider.ForAttributeWithMetadataName("TedToolkit.Grasshopper.DocObjAttribute",
                static (node, _) => node is BaseMethodDeclarationSyntax method &&
                                    method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                static (context, _) => new MethodGenerator(context.TargetSymbol));

        var methodInvocations = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is InvocationExpressionSyntax,
                GetLocNames)
            .Where(static info => info is not null)
            .Select(static (info, _) => info!.Value);

        var items = attributeTypes.Collect()
            .Combine(attributeMethods.Collect())
            .Combine(context.CompilationProvider)
            .Combine(methodInvocations.Collect());
        context.RegisterSourceOutput(items, Generate);
    }


    private static LocInfo? GetLocNames(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var model = context.SemanticModel;

        if (model.GetSymbolInfo(invocation).Symbol is not IMethodSymbol symbol) return null;

        if (symbol.Name is not "Loc"
            || symbol.ContainingType.GetName().FullName is not "global::TedToolkit.Grasshopper.TedToolkitResources")
            return null;

        List<string> arguments = [];
        if (symbol is { IsExtensionMethod: true, ReducedFrom: not null } &&
            invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            if (ExtractConstantValue(memberAccess.Expression, model) is { } stringArg)
                arguments.Add(stringArg);

        foreach (var arg in invocation.ArgumentList.Arguments)
            if (ExtractConstantValue(arg.Expression, model) is { } stringArg)
                arguments.Add(stringArg);

        if (!arguments.Any()) return null;
        var value = arguments[0];
        var key = arguments.Count == 2 ? arguments[1] : GetName();

        return new LocInfo(key, value);

        string GetName()
        {
            var methodSyntax = invocation.Ancestors().OfType<MemberDeclarationSyntax>().FirstOrDefault();
            if (methodSyntax is null) return value;
            if (model.GetDeclaredSymbol(methodSyntax) is not { } methodSymbol) return value;
            var typeSymbol = methodSymbol.ContainingType;
            return typeSymbol.ContainingNamespace + "." + typeSymbol.MetadataName + "." + methodSymbol.Name + "." +
                   value;
        }
    }

    private static string? ExtractConstantValue(ExpressionSyntax expression, SemanticModel model)
    {
        switch (expression)
        {
            case LiteralExpressionSyntax { Token.ValueText: { } stringValue }:
                return stringValue;
            case IdentifierNameSyntax identifier:
            {
                var symbol = model.GetSymbolInfo(identifier).Symbol;
                switch (symbol)
                {
                    case IFieldSymbol { HasConstantValue: true } fieldSymbol:
                        return fieldSymbol.ConstantValue as string;
                    case ILocalSymbol { HasConstantValue: true } localSymbol:
                        return localSymbol.ConstantValue as string;
                }

                break;
            }
        }

        return null;
    }

    private static void Generate(SourceProductionContext context,
        (((ImmutableArray<TypeGenerator> Types, ImmutableArray<MethodGenerator> Methods) Items,
            Compilation Compilation) Left, ImmutableArray<LocInfo> Locs ) arg)
    {
        var compilation = arg.Left.Compilation;
        var assembly = compilation.Assembly;
        var types = arg.Left.Items.Types.Concat(GetTypeGenerators(assembly.GetAttributes()));
        var methods = arg.Left.Items.Methods;

        var baseComponent = GetBaseComponent(assembly.GetAttributes())
                            ?? compilation.GetTypeByMetadataName("Grasshopper.Kernel.GH_Component");

        var baseTaskComponent = compilation.GetTypeByMetadataName("Grasshopper.Kernel.GH_TaskCapableComponent`1");
        var baseCategory = GetBaseCategory(assembly.GetAttributes()) ?? assembly.Name;
        var baseSubcategory = GetBaseSubcategory(assembly.GetAttributes()) ?? assembly.Name;
        var baseAttribute = GetBaseAttribute(assembly.GetAttributes());

        var typeAndClasses = new Dictionary<string, string>();

        BasicGenerator.Translations.Clear();
        BasicGenerator.Icons.Clear();
        BasicGenerator.Categories.Clear();
        BasicGenerator.BaseCategory = baseCategory;
        BasicGenerator.BaseSubcategory = baseSubcategory;
        TypeGenerator.BaseGoo = compilation.GetTypeByMetadataName("Grasshopper.Kernel.Types.GH_Goo`1")!;
        BasicGenerator.CategoryInfos = GetCategoryInfos(assembly.GetAttributes());

        foreach (var type in types)
        {
            type.GenerateSource(context);

            var key = type.Name.FullName;
            var className = "global::" + type.NameSpace + "." + type.RealClassName;
            if (!typeAndClasses.ContainsKey(key)) typeAndClasses.Add(key, className);
            key = className + ".Goo";
            if (!typeAndClasses.ContainsKey(key)) typeAndClasses.Add(key, className);
        }

        foreach (var item in GetAllParams(compilation.GlobalNamespace)
                     .OrderByDescending(i => i.Score))
        foreach (var k in item.Keys)
        {
            var key = k.GetName().FullName;
            if (typeAndClasses.ContainsKey(key)) continue;
            typeAndClasses.Add(key, item.Type.GetName().FullName);
        }

        MethodGenerator.NeedIdNames = methods
            .GroupBy(method => method.ClassName)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();

        MethodParamItem.TypeDictionary = typeAndClasses;

        BasicGenerator.BaseAttribute = baseAttribute;
        MethodGenerator.GlobalBaseComponent = baseComponent!;
        MethodGenerator.CreateSymbol =
            str => baseTaskComponent!.Construct(compilation.CreateErrorTypeSymbol(null, str, 0));

        foreach (var method in methods)
        {
            method.GenerateSource(context);
            if (UpgraderGenerator.Create(method) is { } upgrader) upgrader.GenerateSource(context);
        }

        if (GetCsprojDirectory(assembly) is { } dir)
        {
            foreach (var info in arg.Locs) BasicGenerator.Translations[info.Key] = info.Value;

            GenerateTranslations(dir.FullName);
            GenerateIcons(dir.FullName);
        }

        CategoryGenerator.GenerateCategories(context, assembly, BasicGenerator.Categories);
    }

    private static void GenerateTranslations(string directory)
    {
        directory = Path.Combine(directory, "l10n");
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        try
        {
            ResxManager.Generate(Path.Combine(directory, "TedToolkit.Resources.resx"), BasicGenerator.Translations);
        }
        catch
        {
            // ignored
        }
    }

    private static void GenerateIcons(string directory)
    {
        directory = Path.Combine(directory, "Icons");
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        foreach (var file in Directory.EnumerateFiles(directory, "*.png"))
            try
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.Length != 120) continue;
                fileInfo.Delete();
            }
            catch
            {
                // ignored
            }

        var assembly = typeof(DocumentObjectGenerator).Assembly;
        foreach (var icon in BasicGenerator.Icons)
        {
            var iconType = icon[0];
            var fileName = Path.Combine(directory, string.Concat(icon.Substring(1), ".png"));
            if (File.Exists(fileName)) continue;
            try
            {
                using var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                switch (iconType)
                {
                    case 'P':
                        assembly.GetManifestResourceStream("TedToolkit.Grasshopper.SourceGenerator.Icons.Red.png")
                            ?.CopyTo(fileStream);
                        break;
                    case 'C':
                        assembly.GetManifestResourceStream("TedToolkit.Grasshopper.SourceGenerator.Icons.Blue.png")
                            ?.CopyTo(fileStream);
                        break;
                    case 'c':
                        assembly.GetManifestResourceStream("TedToolkit.Grasshopper.SourceGenerator.Icons.Green.png")
                            ?.CopyTo(fileStream);
                        break;
                    default:
                        assembly.GetManifestResourceStream("TedToolkit.Grasshopper.SourceGenerator.Icons.White.png")
                            ?.CopyTo(fileStream);
                        break;
                }
            }
            catch
            {
                // ignored
            }
        }
    }

    private static DirectoryInfo? GetCsprojDirectory(IAssemblySymbol assembly)
    {
        var fileCs = assembly.Locations
            .Where(l => l.Kind is LocationKind.SourceFile)
            .Select(i => i.GetLineSpan().Path)
            .FirstOrDefault(File.Exists);

        if (string.IsNullOrEmpty(fileCs)) return null;

        var directory = new FileInfo(fileCs).Directory;

        while (directory is not null
               && !directory.EnumerateFiles("*.csproj").Any())
            directory = directory.Parent;

        return directory;
    }

    public static ITypeSymbol? GetBaseComponent(IEnumerable<AttributeData> attributes)
    {
        return (from type in attributes.Select(attribute => attribute.AttributeClass).OfType<INamedTypeSymbol>()
            where type.IsGenericType
            where type.ConstructUnboundGenericType().GetName().FullName is
                "global::TedToolkit.Grasshopper.BaseComponentAttribute<>"
            select type.TypeArguments[0]).FirstOrDefault();
    }

    private static IEnumerable<TypeGenerator> GetTypeGenerators(IEnumerable<AttributeData> attributes)
    {
        return attributes
            .Where(attribute => attribute.AttributeClass is { IsGenericType: true } type
                                && type.ConstructUnboundGenericType().GetName().FullName is
                                    "global::TedToolkit.Grasshopper.DocObjAttribute<>")
            .Select(attr =>
            {
                var result = new TypeGenerator(attr.AttributeClass!.TypeArguments[0])
                {
                    Exposure = "-1"
                };
                ApplyString(attr, 0, s => result.KeyName = s);
                ApplyString(attr, 1, s => result.ObjName = s);
                ApplyString(attr, 2, s => result.ObjNickname = s);
                ApplyString(attr, 3, s => result.ObjDescription = s);
                ApplyString(attr, 4, s => result.ObjTypeName = s);
                ApplyString(attr, 5, s => result.ObjTypeDescription = s);
                return result;
            });
    }

    private static void ApplyString(AttributeData attr, int index, Action<string> append)
    {
        var str = attr.ConstructorArguments.ElementAtOrDefault(index).Value?.ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(str)) return;
        append(str);
    }

    public static string? GetBaseAttribute(IEnumerable<AttributeData> attributes)
    {
        return GetTypeAttribute(attributes, "global::TedToolkit.Grasshopper.ObjAttrAttribute<>")?.FullName;
    }

    public static TypeName? GetTypeAttribute(IEnumerable<AttributeData> attributes, string name)
    {
        foreach (var attr in attributes)
        {
            if (attr.AttributeClass is not { } attributeClass) continue;
            if (!attributeClass.IsGenericType) continue;
            if (attributeClass.TypeArguments.Length < 1) continue;
            if (attributeClass.ConstructUnboundGenericType().GetName().FullName != name) continue;
            return attributeClass.TypeArguments[0].GetName();
        }

        return null;
    }

    public static string? GetBaseCategory(IEnumerable<AttributeData> attributes)
    {
        return GetBaseCate(attributes, "global::TedToolkit.Grasshopper.CategoryAttribute");
    }

    public static string? GetBaseSubcategory(IEnumerable<AttributeData> attributes)
    {
        return GetBaseCate(attributes, "global::TedToolkit.Grasshopper.SubcategoryAttribute");
    }

    private static string? GetBaseCate(IEnumerable<AttributeData> attributes, string attributeFullname)
    {
        foreach (var attr in attributes)
        {
            if (attr.AttributeClass is not { } attributeClass) continue;
            if (attributeClass.GetName().FullName != attributeFullname) continue;
            if (attr.ConstructorArguments.Length is 0) continue;
            return attr.ConstructorArguments[0].Value?.ToString();
        }

        return null;
    }

    private static Dictionary<string, CateInfo> GetCategoryInfos(IEnumerable<AttributeData> attributes)
    {
        var categories = new Dictionary<string, CateInfo>();

        foreach (var attr in attributes)
        {
            if (attr.AttributeClass is not { } attributeClass) continue;
            if (attributeClass.GetName().FullName is not "global::TedToolkit.Grasshopper.CategoryInfoAttribute")
                continue;
            if (attr.ConstructorArguments.Length is 0) continue;

            var key = attr.ConstructorArguments[0].Value?.ToString() ?? string.Empty;
            if (!categories.TryGetValue(key, out var info)) categories[key] = info = new CateInfo();
            if (attr.ConstructorArguments[1].Value?.ToString() is { Length: > 0 } shortName) info.ShortName = shortName;
            if (attr.ConstructorArguments[2].Value?.ToString() is { Length: > 0 } symbolName
                && symbolName[0] is not char.MinValue) info.SymbolName = symbolName[0];
        }

        return categories;
    }

    private static IEnumerable<GhParamItem> GetAllParams(INamespaceSymbol namespaceSymbol)
    {
        foreach (var type in GetTypesFromNamespace(namespaceSymbol))
        {
            if (type.IsAbstract) continue;
            if (type.IsGenericType) continue;
            if (type.DeclaredAccessibility is not Accessibility.Public) continue;
            if (!type.AllInterfaces.Any(i => i.GetName().FullName is "global::Grasshopper.Kernel.IGH_Param")) continue;
            if (type.Name.EndsWith("OBSOLETE", StringComparison.OrdinalIgnoreCase)) continue;
            yield return new GhParamItem(type);
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetTypesFromNamespace(INamespaceSymbol namespaceSymbol)
    {
        return namespaceSymbol.GetTypeMembers()
            .SelectMany(type => (IEnumerable<INamedTypeSymbol>) [type, ..GetNestedTypes(type)])
            .Concat(namespaceSymbol.GetNamespaceMembers().SelectMany(GetTypesFromNamespace));
    }

    private static IEnumerable<INamedTypeSymbol> GetNestedTypes(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.GetTypeMembers()
            .SelectMany(nestTypes => (IEnumerable<INamedTypeSymbol>) [nestTypes, ..GetNestedTypes(nestTypes)]);
    }

    public static void GetObjNames(IEnumerable<AttributeData> attributes, ref string name, ref string nickname,
        ref string description)
    {
        if (attributes
                .FirstOrDefault(a => a.AttributeClass?.GetName().FullName
                    is "global::TedToolkit.Grasshopper.ObjNamesAttribute") is not
            { ConstructorArguments.Length: 3 } attr) return;

        var relay = attr.ConstructorArguments[0].Value?.ToString();
        if (!string.IsNullOrEmpty(relay)) name = relay!;
        relay = attr.ConstructorArguments[1].Value?.ToString();
        if (!string.IsNullOrEmpty(relay)) nickname = relay!;
        relay = attr.ConstructorArguments[2].Value?.ToString();
        if (!string.IsNullOrEmpty(relay)) description = relay!;
    }
}