using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Grasshopper.Kernel;

// ReSharper disable UnusedTypeParameter
#pragma warning disable CS9113 // Parameter is unread.

namespace TedToolkit.Grasshopper;

/// <summary>
///     You can set the type of <see cref="IGH_Param" /> by its <see cref="IGH_DocumentObject.ComponentGuid" />.
/// </summary>
/// <code>
/// public class Test
/// {
///     [DocObj]
///     public static int Add(int x, [ParamType("{2E3AB970-8545-46bb-836C-1C11E5610BCE}")]int y) => x + y;
/// }
/// </code>
/// <remarks>
///     <para><b>⚠ WARNING:</b> Please use <see cref="DocObjAttribute" /> first.</para>
///     <para><b>⚠ NOTICE:</b> It is RECOMMENDED to use <see cref="ParamTypeAttribute{T}" />.</para>
/// </remarks>
/// <param name="id">
///     the <see cref="IGH_DocumentObject.ComponentGuid" /> of the <see cref="IGH_Param" /> that you want to
///     set.
/// </param>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
[Conditional(Constant.KeepAttributes)]
public class ParamTypeAttribute([StringSyntax(StringSyntaxAttribute.GuidFormat)] string id) : Attribute;

/// <summary>
///     You can set the type of <see cref="IGH_Param" /> by the generic way.
/// </summary>
/// <code>
/// public class Test
/// {
///     [DocObj]
///     public static int Add([ParamType&lt;Param_Integer&gt;]int x, int y) => x + y;
/// }
/// </code>
/// <remarks>
///     <para><b>⚠ WARNING:</b> Please use <see cref="DocObjAttribute" /> first.</para>
/// </remarks>
/// <typeparam name="T">The type of the <see cref="IGH_Param" /> you want.</typeparam>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
[Conditional(Constant.KeepAttributes)]
public class ParamTypeAttribute<T> : Attribute where T : IGH_Param;