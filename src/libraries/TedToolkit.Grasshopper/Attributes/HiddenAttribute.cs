using System.Diagnostics;
using Grasshopper.Kernel;

namespace TedToolkit.Grasshopper;

/// <summary>
///     Set the <see cref="IGH_PreviewObject.Hidden" /> for the <see cref="IGH_Param" />
/// </summary>
/// <code>
/// public class Test
/// {
///     public static int Add([Hidden]Arc arc, int y) => (int)arc.Radius + y;
/// }
/// </code>
/// <remarks>
///     <para><b>⚠ WARNING:</b> Please use <see cref="DocObjAttribute" /> first.</para>
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
[Conditional(Constant.KeepAttributes)]
public class HiddenAttribute : Attribute;