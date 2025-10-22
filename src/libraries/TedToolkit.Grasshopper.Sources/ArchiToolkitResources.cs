using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

// ReSharper disable ArrangeTypeModifiers
// ReSharper disable PartialTypeWithSinglePart

namespace TedToolkit.Grasshopper;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal static partial class TedToolkitResources
{
    public static Bitmap? GetIcon(string resourceName)
    {
        using var stream = typeof(TedToolkitResources).Assembly.GetManifestResourceStream(resourceName);
        if (stream is null) return null;
        var bitmap = new Bitmap(stream);
        if (bitmap.Width < 2 || bitmap.Height < 2) return null!;
        return bitmap;
    }

    private static string GetKey(string key, string value)
    {
        if (!string.IsNullOrEmpty(key)) return key;
        var method = new StackFrame(2).GetMethod();
        if (method is null) return value;
        var className = method.DeclaringType?.FullName ?? string.Empty;
        var methodName = method.Name;
        return className + "." + methodName + "." + value;
    }

    /// <summary>
    ///     Localization your string
    /// </summary>
    /// <code>
    /// "Localization String".Loc("Optional Key");
    /// </code>
    /// <param name="value">The default string</param>
    /// <param name="key">The local key</param>
    /// <returns>Localized string</returns>
    public static partial string Loc(this string value, string key = "");
}