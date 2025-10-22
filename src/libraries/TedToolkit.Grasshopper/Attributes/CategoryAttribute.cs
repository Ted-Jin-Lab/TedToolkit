using System.Diagnostics;
using Grasshopper.Kernel;

#pragma warning disable CS9113 // Parameter is unread.

namespace TedToolkit.Grasshopper;

/// <summary>
///     Set the <see cref="GH_InstanceDescription.Category" /> of it.
/// </summary>
/// <remarks>
///     <para>
///         <b>⚠ NOTICE:</b> If you want to make a default <see cref="GH_InstanceDescription.Category" />, please use it
///         on the assembly.
///     </para>
/// </remarks>
/// <param name="recognizeName">The key for localization.</param>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method)]
[Conditional(Constant.KeepAttributes)]
public class CategoryAttribute(string recognizeName) : Attribute;