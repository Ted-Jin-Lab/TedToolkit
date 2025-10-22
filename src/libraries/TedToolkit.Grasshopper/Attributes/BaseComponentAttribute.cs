using System.Diagnostics;
using Grasshopper.Kernel;

namespace TedToolkit.Grasshopper;

/// <summary>
///     For specific the parent class. the parent class must have a constructor with 5 strings.
/// </summary>
/// <code>
/// file abstract class MyComponent(string name, string nickname, string description, string category, string subCategory)
/// : GH_Component(name, nickname, description, category, subCategory)
/// {
/// }
///
/// public class Test
/// {
///     [BaseComponent&lt;MyComponent&gt;]
///     [DocObj]
///     public static int Add(int x, int y) => x + y;
/// }
/// </code>
/// <remarks>
///     <para><b>⚠ WARNING:</b> Please use <see cref="DocObjAttribute" /> first.</para>
///     <para>
///         <b>⚠ NOTICE:</b> If you want to make all <see cref="IGH_Component" /> are based on <typeparamref name="T" />,
///         please use it on the assembly.
///     </para>
/// </remarks>
/// <typeparam name="T">your base component type.</typeparam>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Assembly)]
[Conditional(Constant.KeepAttributes)]
public class BaseComponentAttribute<T> : Attribute
    where T : IGH_Component;