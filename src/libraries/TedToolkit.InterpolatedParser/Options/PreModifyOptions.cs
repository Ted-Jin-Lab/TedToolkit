namespace TedToolkit.InterpolatedParser.Options;

/// <summary>
///     The options to pre modify the <see langword="string" />
/// </summary>
public readonly struct PreModifyOptions()
{
    /// <summary>
    ///     The type of pre modification.
    /// </summary>
    public TrimType TrimType { get; init; } = TrimType.Trim;

    /// <summary>
    ///     Your custom modification.
    /// </summary>
    public Func<string, string>? StringModification { get; init; }

    /// <summary>
    ///     Modify it.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public string ModifyString(string s)
    {
        return TrimType switch
        {
            TrimType.Trim => s.Trim(),
            TrimType.TrimStart => s.TrimStart(),
            TrimType.TrimEnd => s.TrimEnd(),
            TrimType.Custom => StringModification?.Invoke(s) ?? s,
            _ => s
        };
    }
#if NETCOREAPP
    /// <summary>
    ///     Span modification.
    /// </summary>
    public delegate ReadOnlySpan<char> ModifySpan(ReadOnlySpan<char> span);

    /// <summary>
    ///     Your custom modification.
    /// </summary>
    public ModifySpan? SpanModification { get; init; }

    /// <summary>
    ///     Modify it.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public ReadOnlySpan<char> ModifyString(ReadOnlySpan<char> s)
    {
        return TrimType switch
        {
            TrimType.Trim => s.Trim(),
            TrimType.TrimStart => s.TrimStart(),
            TrimType.TrimEnd => s.TrimEnd(),
            TrimType.Custom => SpanModification is null ? s : SpanModification.Invoke(s),
            _ => s
        };
    }
#endif
}