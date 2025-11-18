using System.Runtime.InteropServices;
using TedToolkit.Quantities.Data;
using VDS.RDF;

namespace TedToolkit.Quantities.Generator;

internal sealed class QudtAnalyzer(Graph g, INode? quantitySystem)
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
        if (quantitySystem is null)
        {
            return g.GetTriplesWithPredicateObject(
                    g.CreateUriNode("rdf:type"),
                    g.CreateUriNode("qudt:QuantityKind"))
                .Select(t => t.Subject)
                .OfType<IUriNode>();
        }

        return g.GetTriplesWithSubjectPredicate(
                quantitySystem,
                g.CreateUriNode("qudt:hasQuantityKind"))
            .Concat(g.GetTriplesWithSubjectPredicate(
                quantitySystem,
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
            .Where(q => q.HasValue)
            .Select(q => q!.Value)
            .ToArray();
        return new DataCollection(quantities.ToDictionary(i => i.Name), _units, _dimensions);
    }

    #region Unit

    public Unit UnitParse(IUriNode node)
    {
        var labels = node.GetLabels(g)
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

        var multiplier = node.GetProperty<ILiteralNode>(g, "qudt:conversionMultiplier")
            .FirstOrDefault()?.Value ?? "1";

        var offset = node.GetProperty<ILiteralNode>(g, "qudt:conversionOffset")
            .FirstOrDefault()?.Value ?? "0";

        var factors = node.GetProperty<IBlankNode>(g, "qudt:hasFactorUnit")
            .Select(FactorUnitParse)
            .ToArray();

        var count = g.GetTriplesWithSubjectPredicate(node, g.CreateUriNode("qudt:applicableSystem"))
            .Count();

        return new Unit(node.GetUrlName(), label, node.GetDescription(g), node.GetLinks(g), GetSymbol(node), labels,
            multiplier, offset,
            factors, count);
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
    private readonly Dictionary<string, Dimension> _dimensions = [];

    #region Quantity

    public Quantity? QuantityParse(IUriNode node, bool isBasic)
    {
        var dimensionNode = node.GetProperty<IUriNode>(g, "qudt:hasDimensionVector").First();
        var quantityKind = node.GetProperty<IUriNode>(g, "qudt:hasReferenceQuantityKind").FirstOrDefault();

        var isDefaultQuantity = quantityKind?.ToString() == node.ToString();
        if (DimensionParse(dimensionNode) is not { } dimension) return null;

        var denominator = DimensionParse(node.GetProperty<IUriNode>(g, "qudt:qkdvDenominator").FirstOrDefault());
        var numerator = DimensionParse(node.GetProperty<IUriNode>(g, "qudt:qkdvNumerator").FirstOrDefault());
        var matchName = node.GetProperty<IUriNode>(g, "qudt:exactMatch").Select(r => r.GetUrlName()).ToArray();

        return new Quantity(node.GetUrlName().Replace('-','_'), node.GetDescription(g), node.GetLinks(g), isBasic,
            dimension, isDefaultQuantity,
            GetUnits(node))
        {
            Numerator = numerator ?? string.Empty,
            Denominator = denominator ?? string.Empty,
            ExactMatch = matchName,
        };
    }

    private IReadOnlyList<string> GetUnits(IUriNode node)
    {
        List<string> result = [];
        foreach (var uriNode in node.GetProperty<IUriNode>(g, "qudt:applicableUnit"))
        {
            var name = uriNode.GetUrlName();
            result.Add(name);
            ref var unit = ref CollectionsMarshal.GetValueRefOrAddDefault(_units, name, out var exists);
            if (exists) continue;
            unit = UnitParse(uriNode);
        }

        return result;
    }

    #endregion

    #region FactorUnit

    public FactorUnit FactorUnitParse(IBlankNode node)
    {
        var exponent = int.Parse(node.GetProperty<ILiteralNode>(g, "qudt:exponent").First().Value);
        var unit = node.GetProperty<IUriNode>(g, "qudt:hasUnit").First();
        var dimension = unit.GetProperty<IUriNode>(g, "qudt:hasDimensionVector").First();

        return new FactorUnit(exponent, DimensionParse(dimension)!);
    }

    #endregion

    #region Dimensions

    public string? DimensionParse(IUriNode? node)
    {
        if (node is null) return null;
        try
        {
            var key = node.GetUrlName();
            if (_dimensions.ContainsKey(key)) return key;

            var dimension = new Dimension(
                GetExponent("qudt:dimensionExponentForAmountOfSubstance"),
                GetExponent("qudt:dimensionExponentForElectricCurrent"),
                GetExponent("qudt:dimensionExponentForLength"),
                GetExponent("qudt:dimensionExponentForLuminousIntensity"),
                GetExponent("qudt:dimensionExponentForMass"),
                GetExponent("qudt:dimensionExponentForThermodynamicTemperature"),
                GetExponent("qudt:dimensionExponentForTime"),
                GetExponent("qudt:dimensionlessExponent"));

            _dimensions.Add(key, dimension);
            return key;
        }
        catch
        {
            return null;
        }

        int GetExponent(string name)
        {
            var value = g.GetTriplesWithSubjectPredicate(node,
                    g.CreateUriNode(name))
                .Select(n => n.Object)
                .OfType<LiteralNode>()
                .First()
                .Value;

            return int.Parse(value);
        }
    }

    #endregion
}