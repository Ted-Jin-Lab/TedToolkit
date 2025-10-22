using System.Diagnostics;
using Grasshopper.Kernel;

// ReSharper disable UnusedTypeParameter

namespace TedToolkit.Grasshopper;

/// <summary>
///     Set the <see cref="IGH_DocumentObject.Attributes" /> by using <see cref="IGH_DocumentObject.CreateAttributes" />.
///     You can do it on a <see langword="class" /> or a <see langword="struct" />, but I think that it is unnecessary.
/// </summary>
/// <code>
/// file class MyAttribute(IGH_Component component) : GH_ComponentAttributes(component);
/// 
/// public class Test
/// {
///     [ObjAttr&lt;MyAttribute&gt;]
///     [DocObj]
///     public static int Add(int x, int y) => x + y;
/// }
/// </code>
/// <remarks>
///     <para><b>⚠ WARNING:</b> Please use <see cref="DocObjAttribute" /> first.</para>
///     <para>
///         <b>⚠ NOTICE:</b> If you want to make the <see cref="IGH_DocumentObject.Attributes" /> of all
///         <see cref="IGH_Component" /> are <typeparamref name="T" />, please use it on the assembly.
///     </para>
/// </remarks>
/// <typeparam name="T">the attribute you want to use.</typeparam>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly)]
[Conditional(Constant.KeepAttributes)]
public class ObjAttrAttribute<T> : Attribute
    where T : IGH_Attributes;