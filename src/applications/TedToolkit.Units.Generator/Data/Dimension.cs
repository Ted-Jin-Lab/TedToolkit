using VDS.RDF;

namespace TedToolkit.Units.Generator.Data;

public readonly record struct Dimension(
    int AmountOfSubstance,
    int ElectricCurrent,
    int Length,
    int LuminousIntensity,
    int Mass,
    int ThermodynamicTemperature,
    int Time,
    int Dimensionless)
{
    public static Dimension operator *(int left, Dimension right) => new(
        left * right.AmountOfSubstance,
        left * right.ElectricCurrent,
        left * right.Length,
        left * right.LuminousIntensity,
        left * right.Mass,
        left * right.ThermodynamicTemperature,
        left * right.Time,
        left * right.Dimensionless);

    public static Dimension operator *(Dimension left, int right) => right * left;

    public static Dimension Parse(Graph g, IUriNode node)
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
}