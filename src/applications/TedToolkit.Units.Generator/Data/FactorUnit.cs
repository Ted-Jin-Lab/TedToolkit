using VDS.RDF;

namespace TedToolkit.Units.Generator.Data;

public readonly record struct FactorUnit(int Exponent, Dimension Dimension)
{
    public static FactorUnit Parse(Graph g, IBlankNode node)
    {
        var exponent = int.Parse(g.GetTriplesWithSubjectPredicate(node, g.CreateUriNode("qudt:exponent"))
            .Select(t => t.Object)
            .OfType<ILiteralNode>()
            .First().Value);

        var unit = g.GetTriplesWithSubjectPredicate(node, g.CreateUriNode("qudt:hasUnit"))
            .Select(t => t.Object)
            .OfType<IUriNode>()
            .First();
        
        var dimension = g.GetTriplesWithSubjectPredicate(unit, g.CreateUriNode("qudt:hasDimensionVector"))
            .Select(t => t.Object)
            .OfType<IUriNode>()
            .First();
        
        return new FactorUnit(exponent, Dimension.Parse(g, dimension));
    }
}