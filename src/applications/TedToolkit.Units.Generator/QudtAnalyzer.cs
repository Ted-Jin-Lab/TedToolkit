using System.Runtime.InteropServices;
using System.Text;
using AngleSharp.Dom;
using TedToolkit.Units.Generator.Data;
using VDS.RDF;
using INode = VDS.RDF.INode;

namespace TedToolkit.Units.Generator;

internal sealed class QudtAnalyzer(Graph g)
{
    private const string QuantitySystem = "ISQ";

    private IEnumerable<IUriNode> GetBaseQuantityNodes()
    {
        return g.GetTriplesWithSubjectPredicate(
                g.CreateUriNode("soqk:" + QuantitySystem),
                g.CreateUriNode("qudt:hasBaseQuantityKind"))
            .Select(t => t.Object)
            .OfType<IUriNode>();
    }

    private IEnumerable<IUriNode> GetQuantityNodes()
    {
        return g.GetTriplesWithSubjectPredicate(
                g.CreateUriNode("soqk:" + QuantitySystem),
                g.CreateUriNode("qudt:hasQuantityKind"))
            .Concat(g.GetTriplesWithSubjectPredicate(
                g.CreateUriNode("soqk:" + QuantitySystem),
                g.CreateUriNode("qudt:systemDerivedQuantityKind")))
            .Select(t => t.Object)
            .OfType<IUriNode>();
    }

    public Quantity[] Analyze()
    {
        var basicNodes = GetBaseQuantityNodes().ToArray();
        var otherNodes = GetQuantityNodes().Except(basicNodes).ToArray();
        return basicNodes.Select(n => Quantity.Parse(g, n, true))
            .Concat(otherNodes.Select(n => Quantity.Parse(g, n, false)))
            .ToArray();
    }
}