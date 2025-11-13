using TedToolkit.Scopes;

namespace TedToolkit.Units;

/// <summary>
/// Tolerance for the units
/// </summary>
public partial class Tolerance : ScopeBase<Tolerance>
{
    public Length Length { get; init; }
}