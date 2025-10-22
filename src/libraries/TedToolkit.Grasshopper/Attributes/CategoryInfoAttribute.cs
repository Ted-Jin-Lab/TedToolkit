using System.Diagnostics;

#pragma warning disable CS9113 // Parameter is unread.

namespace TedToolkit.Grasshopper;

/// <summary>
///     Set the category Symbol Name and Short Name.
/// </summary>
/// <code>
/// [assembly: CategoryInfo(null, "Short Name", 'S')]
/// </code>
/// <param name="recognizeName">
///     The key of the <see cref="CategoryAttribute" />, if you want it to control the default
///     category, leave it with <see langword="null" />
/// </param>
/// <param name="shortName">Short Name of the category</param>
/// <param name="symbolName">Symbol Name of the category</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[Conditional(Constant.KeepAttributes)]
public class CategoryInfoAttribute(string? recognizeName, string shortName = "", char symbolName = char.MinValue)
    : Attribute;