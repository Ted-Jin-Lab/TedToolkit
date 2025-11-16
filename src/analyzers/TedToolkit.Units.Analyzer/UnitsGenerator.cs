using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TedToolkit.RoslynHelper.Extensions;
using TedToolkit.RoslynHelper.Names;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

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

    private static (TypeName tDataType, byte flag, Dictionary<string, string> units)? ReadUnit(Compilation compilations)
    {
        if (compilations.Assembly.GetAttributes()
                .FirstOrDefault(a =>
                    a.AttributeClass is { IsGenericType: true } attributeClass &&
                    attributeClass.ConstructUnboundGenericType().ToString().Contains("Units")) is not
            { } attrData) return null;

        var tDataType = attrData.AttributeClass?.TypeArguments.FirstOrDefault()?.GetName();
        if (tDataType is null) return null;

        if (attrData.ApplicationSyntaxReference?.GetSyntax() is not AttributeSyntax syntax) return null;
        byte flag = 0;
        Dictionary<string, string> quantityTypes = [];
        if (syntax.ArgumentList?.Arguments is { } arguments)
            foreach (var attributeArgumentSyntax in arguments)
            {
                var name = attributeArgumentSyntax.NameEquals?.Name.Identifier.ValueText;
                if (name is null) continue;
                var expr = attributeArgumentSyntax.Expression;
                if (name == "Flag")
                {
                    var semanticModel = compilations.GetSemanticModel(expr.SyntaxTree);
                    var constant = semanticModel.GetConstantValue(expr);
                    if (constant is { HasValue: true, Value : byte v })
                    {
                        flag = v;
                    }
                }
                else
                {
                    quantityTypes[name] = expr.ToString().Split('.').Last();
                }
            }

        return (tDataType, flag, quantityTypes);
    }

    private static void Generate(SourceProductionContext context,
        (Compilation Compilation, ImmutableArray<AdditionalText> Texts) arg)
    {
        var (compilations, texts) = arg;
        var unitAttribute = ReadUnit(compilations);

        try
        {
            var data = Helpers.GetData(texts.Select(t => t.GetText(context.CancellationToken)!.ToString()));

            {
                // Default Enum And To Strings.
                new UnitEnumGenerator([..data.Units.Values]).GenerateCode(context);
                var toStringExtensions = ClassDeclaration("UnitToStringExtensions")
                    .WithModifiers(
                        TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitsGenerator))]);
                foreach (var quantity in data.Quantities)
                {
                    var enumGenerator = new QuantityUnitEnumGenerator(data, quantity);
                    enumGenerator.GenerateCode(context);
                    toStringExtensions = toStringExtensions.AddMembers(enumGenerator.GenerateToString());
                }

                context.AddSource("_UnitToStringExtensions.g.cs", NamespaceDeclaration("TedToolkit.Units")
                    .WithMembers([toStringExtensions]).NodeToString());
                new UnitAttributeGenerator(data).Generate(context);
            }

            if (unitAttribute is null) return;
            
            { // For the case with unit attribute
                var (tDataType, flag, units) = unitAttribute.Value;
                var unit = new UnitSystem(units, data);

                foreach (var quantity in data.Quantities)
                {
                    new UnitStructGenerator(data, quantity, tDataType, unit,
                            (flag & 1 << 0) is 0)
                        .GenerateCode(context);
                }

                //
                // new ToleranceGenerator(unit, [..quantities.Where(q => q.IsNoDimensions)])
                //     .Generate(context);
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