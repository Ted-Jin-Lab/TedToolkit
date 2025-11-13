namespace TedToolkit.Units;

/// <summary>
/// The Generated Type access
/// </summary>
[Flags]
public enum UnitFlag : byte
{
    /// <summary>
    /// Nothing
    /// </summary>
    None = 0,
    
    /// <summary>
    /// 
    /// </summary>
    InternalUnit = 1 << 0,

    /// <summary>
    /// 
    /// </summary>
    SimplifyExpression = 1 << 1,
}