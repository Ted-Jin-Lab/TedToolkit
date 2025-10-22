using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TedToolkit.Console;

public static class Helper
{
    public static string GetFulTypeName(this Type type)
    {
        var compilation = CSharpCompilation.Create("Temp")
            .AddReferences(MetadataReference.CreateFromFile(type.Assembly.Location))
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText($"class C {{ {type.FullName} field; }}"));

        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);

        var fieldDeclaration = tree.GetRoot()
            .DescendantNodes()
            .OfType<FieldDeclarationSyntax>()
            .First();

        var variableType = fieldDeclaration.Declaration.Type;
        var typeSymbol = model.GetSymbolInfo(variableType).Symbol as ITypeSymbol;

        return typeSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "Unknown";
    }
}