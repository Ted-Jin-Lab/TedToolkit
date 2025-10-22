using System.Diagnostics;
using Grasshopper.Kernel;

#pragma warning disable CS9113 // Parameter is unread.

namespace TedToolkit.Grasshopper;

/// <summary>
///     Set the <see cref="GH_InstanceDescription.SubCategory" /> of it.
/// </summary>
/// <remarks>
///     <para>
///         <b>⚠ NOTICE:</b> If you want to make a default <see cref="GH_InstanceDescription.SubCategory" />, please use
///         it on the assembly.
///     </para>
/// </remarks>
/// <param name="recognizeName">The key for localization.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Assembly)]
[Conditional(Constant.KeepAttributes)]
public class SubcategoryAttribute(string recognizeName) : Attribute;