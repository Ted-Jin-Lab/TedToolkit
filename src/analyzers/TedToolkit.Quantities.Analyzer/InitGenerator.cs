using Microsoft.CodeAnalysis;

namespace TedToolkit.Quantities.Analyzer;
[Generator(LanguageNames.CSharp)]

public class InitGenerator: IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(InitializationOutput);
    }

    private static void InitializationOutput(IncrementalGeneratorPostInitializationContext context)
    {
    }
}