using System.Diagnostics;
using Grasshopper.Kernel;

namespace TedToolkit.Grasshopper;

/// <summary>
///     Set the <see cref="IGH_Param.Optional" /> for the parameters.
/// </summary>
/// <code>
/// public class Test
/// {
///     [DocObj]
///     public static int Add(int x, [Optional]int y) => x + y;
/// }
/// </code>
/// <remarks>
///     <para><b>⚠ WARNING:</b> Please use <see cref="DocObjAttribute" /> first.</para>
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter)]
[Conditional(Constant.KeepAttributes)]
public class OptionalAttribute : Attribute;