using System.Collections.Immutable;
using TedToolkit.RoslynHelper.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.CppInteropGen.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public class CppSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var configText = context.AdditionalTextsProvider
            .Where(f => Path.GetFileName(f.Path).Equals("CppInteropGen.ini", StringComparison.OrdinalIgnoreCase))
            .Collect()
            .Select((a, token) =>
            {
                var config = new Config();
                var lines = a.FirstOrDefault()?.GetText(token)?.Lines;
                if (lines is null) return config;

                foreach (var line in lines)
                {
                    var pair = line.ToString().Split('=');
                    if (pair.Length is not 2) continue;

                    if (pair[0].Trim() is "Accessibility" && pair[1].Trim() is "internal")
                    {
                        config.IsInternal = true;
                    }
                }
                return config;
            });

        var cppFiles = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith(".cpp")
                           || file.Path.EndsWith(".h")
                           || file.Path.EndsWith(".hxx")
                           || file.Path.EndsWith(".cxx")
            )
            .Select((file, cancellationToken) =>
            {
                var text = file.GetText(cancellationToken);
                return (file.Path, text);
            });

        var compilation = context.CompilationProvider;

        context.RegisterSourceOutput(cppFiles.Collect().Combine(compilation.Combine(configText)), Generate);
    }

    private static void Generate(SourceProductionContext content, (ImmutableArray<(string Path, SourceText? text)> Left, (Compilation compilation, Config option) Right) items)
    {
        var compilation = items.Right.compilation;
        var assemblyName = compilation.AssemblyName;

        foreach (var (path, text) in items.Left)
        {
            if (text is null) continue;
            var fileName = Path.GetFileNameWithoutExtension(path);
            var className = fileName.Substring(0, fileName.Length - 2);

            var nameSpace = assemblyName?.EndsWith(".Wrapper") ?? false ? assemblyName : assemblyName + ".Wrapper";
            var node = NamespaceDeclaration(nameSpace)
                .WithMembers([new CppClassGenerator(text, className, items.Right.option).Generate()]);

            content.AddSource($"{className}.g.cs", node.NodeToString());
        }
    }
}