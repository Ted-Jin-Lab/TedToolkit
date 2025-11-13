using TedToolkit.Scopes;

namespace TedToolkit.Units;

/// <summary>
/// Tolerance for the units
/// </summary>
public class Tolerance : ScopeBase<Tolerance>
{
    public Length<double> Length { get; } = new (1e-6, LengthUnit.Meter);
}