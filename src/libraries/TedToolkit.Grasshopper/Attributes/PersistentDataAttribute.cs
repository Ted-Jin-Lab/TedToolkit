using System.Diagnostics;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;

#pragma warning disable CS9113 // Parameter is unread.

namespace TedToolkit.Grasshopper;

/// <summary>
///     Set the <see cref="GH_PersistentParam{T}.PersistentData" /> by using
///     <see cref="GH_PersistentParam{T}.SetPersistentData(GH_Structure{T})" />
///     or <see cref="GH_PersistentParam{T}.SetPersistentData(T)" />
///     or <see cref="GH_PersistentParam{T}.SetPersistentData(IEnumerable{T})" />
///     or  <see cref="GH_PersistentParam{T}.SetPersistentData(object[])" />
/// </summary>
/// <code>
/// public class Test
/// {
///     internal static int xDefault = 0;
///     [DocObj]
///     public static int Add(
///         [PersistentData(nameof(xDefault))] int x,
///         int y)
///         => x + y;
/// }
/// </code>
/// <remarks>
///     <para><b>⚠ WARNING:</b> Please use <see cref="DocObjAttribute" /> first.</para>
///     <para>
///         <b>⚠ WARNING:</b> Please make sure that the <see cref="IGH_Param" /> of your parameter is a type of
///         <see cref="GH_PersistentParam{T}" />.
///     </para>
/// </remarks>
/// <param name="propertyName">
///     The <see langword="static" /> property or field that you want to set to your
///     <see cref="GH_PersistentParam{T}" />.
/// </param>
[AttributeUsage(AttributeTargets.Parameter)]
[Conditional(Constant.KeepAttributes)]
public class PersistentDataAttribute(string propertyName) : Attribute;

/// <summary>
///     <inheritdoc cref="PersistentDataAttribute" />
/// </summary>
/// <code>
/// public class Others
/// {
///     internal static int yDefault = 1;
/// }
/// 
/// public class Test
/// {
///     [DocObj]
///     public static int Add(
///         int x,
///         [PersistentData&lt;Others&gt;(nameof(Others.yDefault))] int y)
///         => x + y;
/// }
/// </code>
/// <remarks>
///     <inheritdoc cref="PersistentDataAttribute" />
/// </remarks>
/// <param name="propertyName">
///     <inheritdoc cref="PersistentDataAttribute" />
/// </param>
/// <typeparam name="T">
///     The <see langword="class" /> or <see langword="struct" /> that contains your
///     <paramref name="propertyName" />.
/// </typeparam>
[AttributeUsage(AttributeTargets.Parameter)]
[Conditional(Constant.KeepAttributes)]
public class PersistentDataAttribute<T>(string propertyName) : Attribute;