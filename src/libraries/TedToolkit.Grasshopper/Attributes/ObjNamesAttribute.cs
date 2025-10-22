using System.Diagnostics;
using Grasshopper.Kernel;

#pragma warning disable CS9113 // Parameter is unread.

namespace TedToolkit.Grasshopper;

/// <summary>
///     Set the default data for the <see cref="GH_InstanceDescription" />
/// </summary>
/// <code>
/// // For the method, you can do things like this:
/// public class Test
/// {
///     [ObjNames("Addition", "Add", "Normal adding")]
///     [DocObj]
///     [return: ObjNames("Result", "r", "Description of the result.")]
///     public static int Add([ObjNames("X", "x", "Description of x")]int x, int y) => x + y;
/// }
/// // For the class or struct, you can do things like this:
/// [ObjNames("My Type", "T", "My type description")]
/// [DocObj]
/// public class MyType;
/// </code>
/// <remarks>
///     <para><b>⚠ WARNING:</b> Please use <see cref="DocObjAttribute" /> first.</para>
/// </remarks>
/// <param name="name">the <see cref="GH_InstanceDescription.Name" /> of the <see cref="IGH_Param" />.</param>
/// <param name="nickName">the <see cref="GH_InstanceDescription.NickName" /> of the <see cref="IGH_Param" />.</param>
/// <param name="description">the <see cref="GH_InstanceDescription.Description" /> of the <see cref="IGH_Param" />.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.ReturnValue
                | AttributeTargets.Class | AttributeTargets.Struct)]
[Conditional(Constant.KeepAttributes)]
public class ObjNamesAttribute(string name, string nickName, string description) : Attribute;