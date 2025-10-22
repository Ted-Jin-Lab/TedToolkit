using System.Diagnostics;
using Grasshopper.Kernel.Types;

#pragma warning disable CS9113 // Parameter is unread.

namespace TedToolkit.Grasshopper;

/// <summary>
///     Set the <see cref="IGH_Goo.TypeName" /> and <see cref="IGH_Goo.TypeDescription" /> for the <see langword="class" />
///     or <see langword="struct" />.
/// </summary>
/// <code>
/// [TypeDesc("Type Name", "Type Description")]
/// [DocObj]
/// public class MyType;
/// </code>
/// <remarks>
///     <para><b>⚠ WARNING:</b> Please use <see cref="DocObjAttribute" /> first.</para>
/// </remarks>
/// <param name="typeName">The <see cref="IGH_Goo.TypeName" />.</param>
/// <param name="typeDesc">The <see cref="IGH_Goo.TypeDescription" /></param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
[Conditional(Constant.KeepAttributes)]
public class TypeDescAttribute(string typeName = "", string typeDesc = "") : Attribute;