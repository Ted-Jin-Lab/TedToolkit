using System.Diagnostics;
using Grasshopper.Kernel;

#pragma warning disable CS9113 // Parameter is unread.

namespace TedToolkit.Grasshopper;

/// <summary>
///     Set the custom <see cref="GH_Exposure" /> in <see cref="IGH_DocumentObject.Exposure" />.
/// </summary>
/// <code>
/// // For the method, you can do things like this:
/// public class Test
/// {
///     [Exposure(GH_Exposure.secondary)]
///     [DocObj]
///     public static int Add(int x, int y) => x + y;
/// }
/// // For the class or struct, you can do things like this:
/// [Exposure(GH_Exposure.secondary)]
/// [DocObj]
/// public class MyType;
/// </code>
/// <remarks>
///     <para><b>⚠ WARNING:</b> Please use <see cref="DocObjAttribute" /> first.</para>
/// </remarks>
/// <param name="exposure">just the exposure</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method)]
[Conditional(Constant.KeepAttributes)]
public class ExposureAttribute(GH_Exposure exposure) : Attribute;