using TedToolkit.RoslynHelper.Extensions;
using Microsoft.CodeAnalysis;

namespace TedToolkit.ValidResults.SourceGenerator;

public class SimpleType(string fullName, string name, string nameSpace, bool isSymbol)
{
    public string FullName => fullName;
    public string Name => name;
    public string NameSpace => nameSpace;
    public bool IsSymbol => isSymbol;

    public SimpleType(ITypeSymbol type) : this(type.GetName().FullName, type.Name, type.ContainingNamespace.ToDisplayString(), true)
    {

    }

    public SimpleType(string className, string nameSpace) : this("global::" + nameSpace + "." + className, className, nameSpace, false)
    {

    }

    public SimpleType WithGenericTypes(params IEnumerable<string> typeNames)
    {
        return WithFullNamePostfix("<" + string.Join(", ", typeNames) + ">");
    }

    public SimpleType WithFullNamePostfix(string postFix)
    {
        return new SimpleType(FullName + postFix, Name, NameSpace, IsSymbol);
    }
}