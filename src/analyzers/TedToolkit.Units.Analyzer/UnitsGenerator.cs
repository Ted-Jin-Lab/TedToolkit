using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using TedToolkit.RoslynHelper.Extensions;
using TedToolkit.Units.Json;

namespace TedToolkit.Units.Analyzer;

[Generator(LanguageNames.CSharp)]
public class UnitsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var additionalJsonFiles = context.AdditionalTextsProvider
            .Where(file => Path.GetFileName(file.Path).StartsWith("Unit", StringComparison.OrdinalIgnoreCase)
                           && Path.GetExtension(file.Path).Equals(".json", StringComparison.OrdinalIgnoreCase));
        var compilationProvider = context.CompilationProvider;

        var combined = compilationProvider.Combine(additionalJsonFiles.Collect());

        context.RegisterSourceOutput(combined, Generate);
    }

    private void Generate(SourceProductionContext context,
        (Compilation Compilation, ImmutableArray<AdditionalText> Texts) arg)
    {
        var (compilations, texts) = arg;
        
        if (compilations.Assembly.GetAttributes()
                .FirstOrDefault(a =>
                    a.AttributeClass is { IsGenericType: true } attributeClass &&
                    attributeClass.ConstructUnboundGenericType().GetName().FullName is
                        "global::TedToolkit.Units.UnitsAttribute<>") is not { } attrData) return;

        var tDataType = attrData.AttributeClass?.TypeArguments.FirstOrDefault()?.GetName();
        if (tDataType is null) return;

        var arguments = attrData.ConstructorArguments;
        if (arguments.Length < 8) return;

        var length = (byte)attrData.ConstructorArguments[0].Value!;
        var mass = (byte)attrData.ConstructorArguments[1].Value!;
        var time = (byte)attrData.ConstructorArguments[2].Value!;
        var current = (byte)attrData.ConstructorArguments[3].Value!;
        var temperature = (byte)attrData.ConstructorArguments[4].Value!;
        var amount = (byte)attrData.ConstructorArguments[5].Value!;
        var luminousIntensity = (byte)attrData.ConstructorArguments[6].Value!;
        var flag = (byte)attrData.ConstructorArguments[7].Value!;

        try
        {
            var quantities = Quantity
                .GetQuantitiesAsync(texts.Select(file => file.GetText(context.CancellationToken)?.ToString())
                    .OfType<string>()).Result;
            var unit = new UnitSystem(amount, current, length, luminousIntensity, mass, temperature, time, quantities);

            foreach (var quantity in quantities)
            {
                new UnitStructGenerator(quantity, tDataType, unit,
                        (flag & 1 << 0) is 0)
                    .GenerateCode(context);

                var type = compilations.GetTypeByMetadataName($"TedToolkit.Units.{quantity.UnitName}");
                if (type != null) continue;
                var enumGenerator = new UnitEnumGenerator(quantity);
                context.AddSource(enumGenerator.FileName, enumGenerator.GenerateCode());
            }

            new ToleranceGenerator(unit, [..quantities.Where(q => q.IsNoDimensions)])
                .Generate(context);
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