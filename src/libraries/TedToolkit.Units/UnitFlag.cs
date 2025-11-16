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
    InternalUnits = 1 << 0,
    
    /// <summary>
    /// Generate the extension methods.
    /// </summary>
    GenerateExtensionMethods = 1 << 1,
    
    /// <summary>
    /// Generate the extension properties.
    /// </summary>
    GenerateExtensionProperties = 1 << 2,
}