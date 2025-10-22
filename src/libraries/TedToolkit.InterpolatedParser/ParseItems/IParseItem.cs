using TedToolkit.InterpolatedParser.Options;

namespace TedToolkit.InterpolatedParser.ParseItems;

/// <summary>
///     The parse item.
/// </summary>
public interface IParseItem
{
    /// <summary>
    ///     Your pre modification.
    /// </summary>
    public PreModifyOptions PreModification { get; }
}