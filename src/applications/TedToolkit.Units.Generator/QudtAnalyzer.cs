using System.Runtime.InteropServices;
using System.Text;
using AngleSharp.Dom;
using TedToolkit.Units.Data;
using VDS.RDF;
using INode = VDS.RDF.INode;

namespace TedToolkit.Units.Generator;

internal sealed class QudtAnalyzer(Graph g, string quantitySystem = "ISQ")
{
    private IEnumerable<IUriNode> GetBaseQuantityNodes()
    {
        return g.GetTriplesWithSubjectPredicate(
                g.CreateUriNode("soqk:ISQ"),
                g.CreateUriNode("qudt:hasBaseQuantityKind"))
            .Select(t => t.Object)
            .OfType<IUriNode>();
    }

    private IEnumerable<IUriNode> GetQuantityNodes()
    {
        return g.GetTriplesWithSubjectPredicate(
                g.CreateUriNode("soqk:" + quantitySystem),
                g.CreateUriNode("qudt:hasQuantityKind"))
            .Concat(g.GetTriplesWithSubjectPredicate(
                g.CreateUriNode("soqk:" + quantitySystem),
                g.CreateUriNode("qudt:systemDerivedQuantityKind")))
            .Select(t => t.Object)
            .OfType<IUriNode>();
    }

    public DataCollection Analyze()
    {
        var basicNodes = GetBaseQuantityNodes().ToArray();
        var otherNodes = GetQuantityNodes().Except(basicNodes).ToArray();
        var quantities = basicNodes.Select(n => QuantityParse(n, true))
            .Concat(otherNodes.Select(n => QuantityParse(n, false)))
            .ToArray();
        return new DataCollection(quantities, _units);
    }

    #region Unit

    public Unit UnitParse(IUriNode node)
    {
        var labels = GetLabels(node)
            .Where(i => !string.IsNullOrEmpty(i.Language))
            .ToDictionary(i => i.Language, i => i.Value);

        if (!labels.TryGetValue("", out var label))
        {
            if (!labels.TryGetValue("en", out label))
            {
                label = null!;
            }
        }
        label = label?.LabelToName() ?? node.GetUrlName();

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
            .Select(FactorUnitParse)
            .ToArray();

        var count = g.GetTriplesWithSubjectPredicate(node, g.CreateUriNode("qudt:applicableSystem"))
            .Count();

        return new Unit(node.GetUrlName(), label, node.GetDescription(g), node.GetLinks(g), GetSymbol(node), labels, multiplier, offset,
            factors, count);
    }

    private IEnumerable<ILiteralNode> GetLabels(IUriNode node)
    {
        return g.GetTriplesWithSubjectPredicate(node, g.CreateUriNode("rdfs:label"))
            .Select(t => t.Object)
            .OfType<ILiteralNode>();
    }

    private string GetSymbol(IUriNode node)
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

    #endregion

    private readonly Dictionary<string, Unit> _units = [];

    #region Quantity

    public Quantity QuantityParse(IUriNode node, bool isBasic)
    {
        return new Quantity(node.GetUrlName(), node.GetDescription(g), node.GetLinks(g), isBasic,
            GetDimensionVector(node, out var isDefaultQuantity), isDefaultQuantity,
            GetUnits(node));
    }

    private IReadOnlyList<string> GetUnits(IUriNode node)
    {
        List<string> result = [];
        foreach (var uriNode in g.GetTriplesWithSubjectPredicate(node, g.CreateUriNode("qudt:applicableUnit"))
                     .Select(t => t.Object)
                     .OfType<IUriNode>())
        {
            var name = uriNode.GetUrlName();
            result.Add(name);
            ref var unit = ref CollectionsMarshal.GetValueRefOrAddDefault(_units, name, out var exists);
            if (exists) continue;
            unit = UnitParse(uriNode);
        }

        return result;
    }


    private Dimension GetDimensionVector(IUriNode quantity, out bool isDefaultQuantity)
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

        return DimensionParse(dimension);
    }

    #endregion

    #region FactorUnit

    public FactorUnit FactorUnitParse(IBlankNode node)
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

        return new FactorUnit(exponent, DimensionParse(dimension));
    }

    #endregion

    #region Dimensions

    public Dimension DimensionParse(IUriNode node)
    {
        return new Dimension(
            GetExponent("qudt:dimensionExponentForAmountOfSubstance"),
            GetExponent("qudt:dimensionExponentForElectricCurrent"),
            GetExponent("qudt:dimensionExponentForLength"),
            GetExponent("qudt:dimensionExponentForLuminousIntensity"),
            GetExponent("qudt:dimensionExponentForMass"),
            GetExponent("qudt:dimensionExponentForThermodynamicTemperature"),
            GetExponent("qudt:dimensionExponentForTime"),
            GetExponent("qudt:dimensionlessExponent"));

        int GetExponent(string name)
        {
            return int.Parse(g.GetTriplesWithSubjectPredicate(node,
                    g.CreateUriNode(name))
                .Select(n => n.Object)
                .OfType<LiteralNode>()
                .First()
                .Value);
        }
    }

    #endregion
}