using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TedToolkit.Quantities.Data;
using TedToolkit.RoslynHelper.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TedToolkit.RoslynHelper.Extensions.SyntaxExtensions;

namespace TedToolkit.Quantities.Analyzer;

public abstract class UnitExtensionGenerator(bool isPublic, DataCollection data)
{
    protected IEnumerable<(string quantity, string unit, string memberName)> CreateMembers()
    {
        return CreateUnits().GroupBy(i => i.unit).SelectMany(g =>
        {
            return g.Count() < 2
                ? g.Select(pair => (pair.quantity, pair.unit, pair.unit))
                : g.Select(pair => (pair.quantity, pair.unit, pair.unit + "_" + pair.quantity));
        });
    }

    private IEnumerable<(string quantity, string unit)> CreateUnits()
    {
        return data.Quantities.SelectMany(q =>
            q.Units.Select(u => (q.Name, data.Units[u].GetUnitName(data.Units.Values))));
    }
    
    protected abstract string FileName { get; }
    
    protected abstract ClassDeclarationSyntax ModifyClass(ClassDeclarationSyntax classDeclaration);
    
    public void Generate(SourceProductionContext context)
    {
        var nameSpace = NamespaceDeclaration("TedToolkit.Quantities")
            .WithMembers([
                ModifyClass(ClassDeclaration("UnitsExtension")
                    .WithModifiers(TokenList(Token(isPublic ? SyntaxKind.PublicKeyword : SyntaxKind.InternalKeyword),
                        Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.PartialKeyword)))
                    .WithAttributeLists([GeneratedCodeAttribute(typeof(UnitMethodExtensionGenerator))]))
            ]);
        
        context.AddSource(FileName + ".g.cs", nameSpace.NodeToString());
        
    }
}