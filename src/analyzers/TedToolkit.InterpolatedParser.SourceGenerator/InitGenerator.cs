using TedToolkit.RoslynHelper.Extensions;

namespace TedToolkit.InterpolatedParser.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public class InitGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(InitializationOutput);
    }

    private static void InitializationOutput(IncrementalGeneratorPostInitializationContext context)
    {
        var assembly = typeof(InitGenerator).Assembly;
        foreach (var name in assembly.GetManifestResourceNames())
        {
            if (!name.EndsWith(".cs")) continue;

            using var stream = assembly.GetManifestResourceStream(name);
            if (stream == null) continue;

            using var reader = new StreamReader(stream);

            var root = CSharpSyntaxTree.ParseText(reader.ReadToEnd(),
                    new CSharpParseOptions(preprocessorSymbols: ["NET7_0_OR_GREATER", "NETCOREAPP", "NETFRAMEWORK"]))
                .GetRoot();
            var updateRoot = new GeneratedRewriter(typeof(InitGenerator)).Visit(root);
            context.AddSource($"{name}.g.cs", updateRoot.NodeToString());
        }
    }
}