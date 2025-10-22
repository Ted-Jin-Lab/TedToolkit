using System.Diagnostics;
using Grasshopper.Kernel;

#pragma warning disable CS9113 // Parameter is unread.

namespace TedToolkit.Grasshopper;

/// <summary>
///     if the data is a field of the <see cref="IGH_Component" />, please use it.
/// </summary>
/// <code>
/// public class Test
/// {
///     [DocObj]
///     public static int Add([ObjField(true)]int x, int y)=> x + y;
/// }
/// </code>
/// <remarks>
///     <para><b>⚠ WARNING:</b> Please use <see cref="DocObjAttribute" /> first.</para>
///     <para><b>⚠ WARNING:</b> The data must not be <see langword="out" />.</para>
/// </remarks>
/// <param name="saveToFile">if it should be saved into the file</param>
[AttributeUsage(AttributeTargets.Parameter)]
[Conditional(Constant.KeepAttributes)]
public class ObjFieldAttribute(bool saveToFile = false) : Attribute;