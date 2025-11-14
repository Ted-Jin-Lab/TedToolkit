using TedToolkit.Scopes;

namespace TedToolkit.Units;

public readonly struct Length : IEquatable<Length>, IComparable<Length>
{
    public static explicit operator Length(double value) => new Length();

    private readonly double Value;
    public static double Tolerance => Units.Tolerance.CurrentDefault.Length.Value;


    public bool Equals(Length other) => (double)Math.Abs(Value - other.Value) <= Tolerance;

    public override bool Equals(object? obj) => obj is Length other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public int CompareTo(Length other) => Equals(other) ? 0 : Value.CompareTo(other.Value);
}

/// <summary>
/// Tolerance for the units
/// </summary>
public partial class Tolerance : ScopeBase<Tolerance>
{
    public Length Length { get; init; } = (Length)1e-6;


    public static Tolerance CurrentDefault => Current ?? new Tolerance();
}