using VDS.RDF;

namespace TedToolkit.Units.Generator.Data;

public readonly record struct Quantity(
    string Name,
    string Description,
    IReadOnlyList<Link> Links,
    bool IsBasic,
    Dimension Dimension,
    bool IsDimensionDefault,
    IReadOnlyList<Unit> Units)
{
    public static Quantity Parse(Graph g, IUriNode node, bool isBasic)
    {
        return new Quantity(node.GetUrlName(), node.GetDescription(g), node.GetLinks(g), isBasic,
            GetDimensionVector(g, node, out var isDefaultQuantity), isDefaultQuantity,
            GetUnits(g, node));
    }

    private static IReadOnlyList<Unit> GetUnits(Graph g, IUriNode node)
    {
        return
        [
            ..g.GetTriplesWithSubjectPredicate(node, g.CreateUriNode("qudt:applicableUnit"))
                .Select(t => t.Object)
                .OfType<IUriNode>().Select(i => Unit.Parse(g, i))
        ];
    }


    private static Dimension GetDimensionVector(Graph g, IUriNode quantity, out bool isDefaultQuantity)
    {
        var dimension = g.GetTriplesWithSubjectPredicate(quantity,
                g.CreateUriNode("qudt:hasDimensionVector"))
            .Select(n => n.Object)
            .OfType<IUriNode>()
            .First();

        var quantityKind = g.GetTriplesWithSubjectPredicate(dimension,
                g.CreateUriNode("qudt:hasReferenceQuantityKind"))
            .Select(n => n.Object)
            .FirstOrDefault();

        isDefaultQuantity = quantityKind?.ToString() == quantity.ToString();

        return Data.Dimension.Parse(g, dimension);
    }
}