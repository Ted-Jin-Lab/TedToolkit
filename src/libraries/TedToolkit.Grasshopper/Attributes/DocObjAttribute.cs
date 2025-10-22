// ReSharper disable RedundantUsingDirective

using System;
using System.Diagnostics;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

#pragma warning disable CS9113 // Parameter is unread.

namespace TedToolkit.Grasshopper;

/// <summary>
///     To create a document object.
///     <para>
///         It'll be an <see cref="IGH_Param" /> when it is tagged on a <see langword="class" /> or a
///         <see langword="struct" />.
///     </para>
///     <para>It'll be an <see cref="IGH_Component" /> when it is tagged on a method.</para>
/// </summary>
/// <code>
/// // For the method, you can do things like this:
/// public class Test
/// {
///    [DocObj]
///    public static int Add(int x, int y) => x + y;
/// }
/// 
/// // For the class or struct, you can do things like this:
/// [DocObj]
/// public class MyType;
/// </code>
/// <param name="recognizeName">The key for localization, leave it for auto-generation.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct)]
[Conditional(Constant.KeepAttributes)]
public class DocObjAttribute(string recognizeName = "") : Attribute;

/// <summary>
///     Generate the <see cref="IGH_Param" /> by the specific <see langword="class" /> or <see langword="struct" />.
/// </summary>
/// <code>
/// [assembly: DocObj&lt;RemoteType&gt;]
/// public class RemoteType;
/// </code>
/// <remarks>
///     <para><b>⚠ NOTICE:</b> It is RECOMMENDED to use <see cref="DocObjAttribute" />.</para>
/// </remarks>
/// <param name="recognizeName">The key for localization, leave it for auto-generation.</param>
/// <param name="name">the <see cref="GH_InstanceDescription.Name" /> of the <see cref="IGH_Param" />.</param>
/// <param name="nickName">the <see cref="GH_InstanceDescription.NickName" /> of the <see cref="IGH_Param" />.</param>
/// <param name="description">the <see cref="GH_InstanceDescription.Description" /> of the <see cref="IGH_Param" />.</param>
/// <param name="typeName">the <see cref="IGH_Goo.TypeName" /> of the <see cref="IGH_Goo" />.</param>
/// <param name="typeDescription">the <see cref="IGH_Goo.TypeDescription" /> of the <see cref="IGH_Goo" />.</param>
/// <typeparam name="T">your type that want to be an <see cref="IGH_Param" />.</typeparam>
[AttributeUsage(AttributeTargets.Assembly)]
[Conditional(Constant.KeepAttributes)]
public class DocObjAttribute<T>(
    string recognizeName = "",
    string name = "",
    string nickName = "",
    string description = "",
    string typeName = "",
    string typeDescription = "") : Attribute;