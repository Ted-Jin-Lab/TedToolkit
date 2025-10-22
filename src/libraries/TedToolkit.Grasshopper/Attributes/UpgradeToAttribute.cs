using System.Diagnostics;
using Grasshopper.Kernel;

#pragma warning disable CS9113 // Parameter is unread.

namespace TedToolkit.Grasshopper;

/// <summary>
///     Generate a <see cref="IGH_UpgradeObject" /> for you to upgrade from this <see cref="IGH_Component" />.
/// </summary>
/// <code>
/// public class Test
/// {
///     [UpgradeTo&lt;Component_Add&gt;(2025, 3, 26)]
///     [DocObj]
///     public static int OldAdd(int x, int y) => x + y;
/// 
///     [DocObj]
///     public static int Add(int x, int y) => x + y;
/// }
/// </code>
/// <remarks>
///     <para><b>⚠ WARNING:</b> Please use <see cref="DocObjAttribute" /> first.</para>
///     <para><b>⚠ WARNING:</b> You may need to set the FullName with Namespace to the <typeparamref name="T" />.</para>
/// </remarks>
/// <param name="year">The <see cref="DateTime.Year" /> of the <see cref="IGH_UpgradeObject.Version" /></param>
/// <param name="month">The <see cref="DateTime.Month" /> of the <see cref="IGH_UpgradeObject.Version" /></param>
/// <param name="day">The <see cref="DateTime.Day" /> of the <see cref="IGH_UpgradeObject.Version" /></param>
/// <param name="hour">The <see cref="DateTime.Hour" /> of the <see cref="IGH_UpgradeObject.Version" /></param>
/// <param name="minute">The <see cref="DateTime.Minute" /> of the <see cref="IGH_UpgradeObject.Version" /></param>
/// <param name="second">The <see cref="DateTime.Second" /> of the <see cref="IGH_UpgradeObject.Version" /></param>
/// <typeparam name="T">The component to upgrade to.</typeparam>
[AttributeUsage(AttributeTargets.Method)]
[Conditional(Constant.KeepAttributes)]
public class UpgradeToAttribute<T>(int year, int month, int day = 1, int hour = 0, int minute = 0, int second = 0)
    : Attribute where T : IGH_Component, new();