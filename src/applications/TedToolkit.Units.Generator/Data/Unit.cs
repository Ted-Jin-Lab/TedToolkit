using VDS.RDF;

namespace TedToolkit.Units.Generator.Data;

public readonly record struct Unit(
    string Name,
    string Description,
    IReadOnlyList<Link> Links,
    string Symbol,
    Dictionary<string, string> Labels,
    string Multiplier,
    string Offset,
    IReadOnlyList<FactorUnit> FactorUnits)
{
    public static Unit Parse(Graph g, IUriNode node)
    {
        var labels = GetLabels(g, node)
            .Where(i => !string.IsNullOrEmpty(i.Language))
            .ToDictionary(i => i.Language, i => i.Value);

        var label = labels
            .OrderBy(n => n.Key.Length)
            .FirstOrDefault().Value?.LabelToName() ?? node.GetUrlName();

        var multiplier = g.GetTriplesWithSubjectPredicate(node, g.CreateUriNode("qudt:conversionMultiplier"))
            .Select(t => t.Object)
            .OfType<ILiteralNode>()
            .FirstOrDefault()?.Value ?? "1";

        var offset = g.GetTriplesWithSubjectPredicate(node, g.CreateUriNode("qudt:conversionOffset"))
            .Select(t => t.Object)
            .OfType<ILiteralNode>()
            .FirstOrDefault()?.Value ?? "0";

        var factors = g.GetTriplesWithSubjectPredicate(node, g.CreateUriNode("qudt:hasFactorUnit"))
            .Select(t => t.Object)
            .OfType<IBlankNode>()
            .Select(n => FactorUnit.Parse(g, n))
            .ToArray();

        return new Unit(label, node.GetDescription(g), node.GetLinks(g), GetSymbol(g, node), labels, multiplier, offset,
            factors);
    }

    private static IEnumerable<ILiteralNode> GetLabels(Graph g, IUriNode node)
    {
        return g.GetTriplesWithSubjectPredicate(node, g.CreateUriNode("rdfs:label"))
            .Select(t => t.Object)
            .OfType<ILiteralNode>();
    }

    private static string GetSymbol(Graph g, IUriNode node)
    {
        return GetString("qudt:symbol")
               ?? GetString("qudt:ucumCode")
               ?? GetString("qudt:udunitsCode")
               ?? string.Empty;

        string? GetString(string predicate)
        {
            return g.GetTriplesWithSubjectPredicate(node,
                    g.CreateUriNode(predicate))
                .Select(t => t.Object)
                .OfType<ILiteralNode>()
                .FirstOrDefault()?.Value;
        }
    }
}