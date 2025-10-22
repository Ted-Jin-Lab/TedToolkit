using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Grasshopper.Kernel;

#pragma warning disable CS9113 // Parameter is unread.

namespace TedToolkit.Grasshopper;

/// <summary>
///     For the case that you want to set the <see cref="IGH_DocumentObject.ComponentGuid" /> manually.
/// </summary>
/// <code>
/// // For the method, you can do things like this:
/// public class Test
/// {
///     [ObjGuid("71156816-F2C5-46B0-B6D9-E71F28CDF7A4")]
///     [DocObj]
///     public static int Add(int x, int y) => x + y;
/// }
/// // For the class or struct, you can do things like this:
/// [ObjGuid("692328E7-D68C-4248-B5E0-8879DFEA97B4")]
/// [DocObj]
/// public class MyType;
/// </code>
/// <remarks>
///     <para><b>⚠ WARNING:</b> Please use <see cref="DocObjAttribute" /> first.</para>
/// </remarks>
/// <param name="id">The <see cref="IGH_DocumentObject.ComponentGuid" />.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct)]
[Conditional(Constant.KeepAttributes)]
public class ObjGuidAttribute([StringSyntax(StringSyntaxAttribute.GuidFormat)] string id) : Attribute;