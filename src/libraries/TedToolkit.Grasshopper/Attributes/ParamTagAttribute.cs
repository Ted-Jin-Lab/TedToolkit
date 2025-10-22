using System.Diagnostics;
using Grasshopper.Kernel;

namespace TedToolkit.Grasshopper;

/// <summary>
///     The <see cref="IGH_StateTag" />
/// </summary>
[Flags]
public enum ParamTagType : byte
{
    /// <summary>
    ///     Nothing,
    /// </summary>
    None,

    /// <summary>
    ///     Set the <see cref="GH_Component.PrincipalParameterIndex" /> to the index of this parameter
    /// </summary>
    Principal = 1 << 0,

    /// <summary>
    ///     Set the <see cref="IGH_Param.Reverse" /> to <see langword="true" />
    /// </summary>
    Reverse = 1 << 1,

    /// <summary>
    ///     Set the <see cref="GH_DataMapping.Flatten" /> to the <see cref="IGH_Param.DataMapping" />
    /// </summary>
    Flatten = 1 << 2,

    /// <summary>
    ///     Set the <see cref="GH_DataMapping.Graft" /> to the <see cref="IGH_Param.DataMapping" />
    /// </summary>
    Graft = 1 << 3,

    /// <summary>
    ///     Set the <see cref="IGH_Param.Simplify" /> to <see langword="true" />
    /// </summary>
    Simplify = 1 << 4
}

/// <summary>
///     Add the Tags to the <see cref="IGH_StateTag" /> to the <see cref="IGH_Param" />
/// </summary>
/// <code>
/// public class Test
/// {
///     [DocObj]
///     public static int Add(int x, [ParamTag(ParamTagType.Flatten)]int y) => x + y;
/// }
/// </code>
/// <remarks>
///     <para><b>⚠ WARNING:</b> Please use <see cref="DocObjAttribute" /> first.</para>
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
[Conditional(Constant.KeepAttributes)]
public class ParamTagAttribute(ParamTagType type) : Attribute;