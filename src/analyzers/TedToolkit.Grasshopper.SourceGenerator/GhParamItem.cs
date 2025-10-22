using TedToolkit.RoslynHelper.Extensions;
using Microsoft.CodeAnalysis;

namespace TedToolkit.Grasshopper.SourceGenerator;

public class GhParamItem
{
    private readonly ITypeSymbol? _gooType;
    public readonly INamedTypeSymbol Type;
    private int? _score;

    public GhParamItem(INamedTypeSymbol type)
    {
        Type = type;
        foreach (var t in type.GetBaseTypesAndThis())
        {
            if (t is not INamedTypeSymbol ts) continue;
            if (!ts.IsGenericType) continue;
            if (ts.ConstructUnboundGenericType().GetName().FullName
                is not "global::Grasshopper.Kernel.GH_Param<>") continue;
            _gooType = ts.TypeArguments[0];
            break;
        }
    }

    public int Score => _score ??= ScoreParam();

    public IEnumerable<ITypeSymbol> Keys
    {
        get
        {
            if (_gooType is null) yield break;
            yield return _gooType;
            var prop = _gooType.GetMembers().OfType<IPropertySymbol>()
                .FirstOrDefault(p => p.Name is "Value");
            if (prop is not null) yield return prop.Type;
            foreach (var type in _gooType.GetBaseTypes())
            {
                if (type is not INamedTypeSymbol ts) continue;
                if (!ts.IsGenericType) continue;
                var name = ts.ConstructUnboundGenericType().GetName().FullName;
                switch (name)
                {
                    case "global::Grasshopper.Kernel.Types.GH_GeometricGoo<>":
                    case "global::Grasshopper.Kernel.Types.GH_Goo<>":
                        yield return ts;
                        yield return ts.TypeArguments[0];
                        break;
                }
            }
        }
    }

    private int ScoreParam()
    {
        var score = 0;
        if (IsGenericType("global::Grasshopper.Kernel.GH_PersistentGeometryParam<>"))
            score += 20;
        else if (IsGenericType("global::Grasshopper.Kernel.GH_PersistentParam<>")) score += 10;

        if (_gooType is not null
            && _gooType.Name.Split('_').Last().Equals(Type.Name.Split('_').Last(), StringComparison.OrdinalIgnoreCase))
            score += 5;

        return score;

        bool IsGenericType(string name)
        {
            return Type.GetBaseTypesAndThis().Any(t =>
            {
                if (t is not INamedTypeSymbol ts) return false;
                if (!ts.IsGenericType) return false;
                return ts.ConstructUnboundGenericType().GetName().FullName == name;
            });
        }
    }
}