using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TedToolkit.RoslynHelper.Extensions;
using TedToolkit.RoslynHelper.Names;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.Quantities.Analyzer;

[Generator(LanguageNames.CSharp)]
public class QuantitiesGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var additionalJsonFiles = context.AdditionalTextsProvider
            .Where(file => Path.GetFileName(file.Path).StartsWith("Quantity", StringComparison.OrdinalIgnoreCase)
                           && Path.GetExtension(file.Path).Equals(".json", StringComparison.OrdinalIgnoreCase));
        var compilationProvider = context.CompilationProvider;

        var combined = compilationProvider.Combine(additionalJsonFiles.Collect());

        context.RegisterSourceOutput(combined, Generate);
    }

    private static (TypeName tDataType, byte flag, Dictionary<string, string> units, string quantitySystem, string[]
        quantities)? ReadUnit(
            Compilation compilations)
    {
        if (compilations.Assembly.GetAttributes()
                .FirstOrDefault(a =>
                    a.AttributeClass is { IsGenericType: true } attributeClass &&
                    attributeClass.ConstructUnboundGenericType().ToString().Contains("Quantities")) is not
            { } attrData) return null;

        var tDataType = attrData.AttributeClass?.TypeArguments.FirstOrDefault()?.GetName();
        if (tDataType is null) return null;

        if (attrData.ApplicationSyntaxReference?.GetSyntax() is not AttributeSyntax syntax) return null;
        byte flag = 0;
        var quantitySystem = "";
        List<string> quantities = [];
        Dictionary<string, string> quantityTypes = [];
        if (syntax.ArgumentList?.Arguments is { } arguments)
        {
            var isFirst = true;
            foreach (var attributeArgumentSyntax in arguments)
            {
                var name = attributeArgumentSyntax.NameEquals?.Name.Identifier.ValueText;

                var expr = attributeArgumentSyntax.Expression;
                if (name == "Flag")
                {
                    GetData<byte>(v => flag = v);
                }
                else if (name is null)
                {
                    GetData<string>(v =>
                    {
                        if (isFirst)
                        {
                            quantitySystem = v;
                            isFirst = false;
                        }
                        else
                        {
                            quantities.Add(v);
                        }
                    });
                }
                else
                {
                    quantityTypes[name] = expr.ToString().Split('.').Last();
                }

                continue;

                void GetData<TData>(Action<TData> action)
                {
                    var semanticModel = compilations.GetSemanticModel(expr.SyntaxTree);
                    var constant = semanticModel.GetConstantValue(expr);

                    if (constant is { HasValue: true, Value : TData v })
                    {
                        action(v);
                    }
                }
            }
        }

        return (tDataType, flag, quantityTypes, quantitySystem, quantities.ToArray());
    }

    private static void Generate(SourceProductionContext context,
        (Compilation Compilation, ImmutableArray<AdditionalText> Texts) arg)
    {
        try
        {
            var (compilations, texts) = arg;

            if (compilations.GetTypeByMetadataName("TedToolkit.Quantities.QuantitiesAttribute`1")?.BaseType?.GetName()
                    ?.FullName
                is "global::System.Attribute") return;

            var unitAttribute = ReadUnit(compilations);
            var tDataType = unitAttribute?.tDataType!;
            var flag = unitAttribute?.flag!;
            var units = unitAttribute?.units!;
            var quantitySystem = unitAttribute?.quantitySystem;
            var quantities = unitAttribute?.quantities;

            var data = Helpers.GetData(quantitySystem,
                texts.Select(t => t.GetText(context.CancellationToken)!.ToString()), quantities ?? []);
            {
                // Default Enum And To Strings.
                new UnitEnumGenerator([..data.Units.Values]).GenerateCode(context);
                var toStringExtensions = ClassDeclaration("UnitToStringExtensions")
                    .WithModifiers(
                        TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(QuantitiesGenerator))]);
                foreach (var quantity in data.Quantities)
                {
                    var enumGenerator = new QuantityUnitEnumGenerator(data, quantity.Value);
                    enumGenerator.GenerateCode(context);
                    toStringExtensions = toStringExtensions.AddMembers(enumGenerator.GenerateToString());
                }

                context.AddSource("_UnitToStringExtensions.g.cs", NamespaceDeclaration("TedToolkit.Quantities")
                    .WithMembers([toStringExtensions]).NodeToString());
                new QuantitiesAttributeGenerator(data).Generate(context);
            }

            if (unitAttribute is null) return;

            {
                var isPublic = (flag & 1 << 0) is 0;
                var generateMethods = (flag & 1 << 1) is not 0;
                var generateProperties = (flag & 1 << 2) is not 0;

                var unit = new UnitSystem(units, data);

                foreach (var quantity in data.Quantities)
                {
                    var quantitySymbol =
                        compilations.Assembly.GetTypeByMetadataName("TedToolkit.Quantities." + quantity.Value.Name);
                    new QuantityStructGenerator(data, quantity.Value, tDataType, unit,
                            isPublic, quantitySymbol)
                        .GenerateCode(context);
                }

                new ToleranceGenerator(unit, data.Quantities.Values.ToArray(), isPublic, tDataType)
                    .Generate(context);

                if (generateProperties)
                {
                    new UnitPropertyExtensionGenerator(isPublic, tDataType, data).Generate(context);
                }
                else if (generateMethods)
                {
                    new UnitMethodExtensionGenerator(isPublic, tDataType, data).Generate(context);
                }
            }
        }
        catch (Exception e)
        {
            var msg = e.GetType().Name + ": " + e.Message + "\n" + e.StackTrace;
            context.AddSource("_ERROR", msg);
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("ERR", msg, msg, "Error", DiagnosticSeverity.Error, true), Location.None));
        }
    }
}