using System.Diagnostics;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace TedToolkit.Grasshopper;

/// <summary>
///     For the case that you want to specify the <see cref="IGH_Goo" /> type
/// </summary>
/// <code>
/// [BaseGoo&lt;H_GeometricGoo&lt;MyType&gt;&gt;]
/// [DocObj]
/// public class MyType;
/// 
/// partial class Param_MyType
/// {
///     partial class Goo
///     {
///        public override IGH_GeometricGoo DuplicateGeometry()
///        {
///            throw new NotImplementedException();
///        }
/// 
///        public override BoundingBox GetBoundingBox(Transform xform)
///        {
///             throw new NotImplementedException();
///        }
/// 
///        public override IGH_GeometricGoo Transform(Transform xform)
///        {
///             throw new NotImplementedException();
///        }
/// 
///        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
///        {
///            throw new NotImplementedException();
///        }
/// 
///        public override BoundingBox Boundingbox => default;
///     }
/// }
/// </code>
/// <remarks>
///     <para><b>⚠ WARNING:</b> Please use <see cref="DocObjAttribute" /> first.</para>
///     <para><b>⚠ NOTICE:</b> Don't forget to use <see langword="partial" /> to implement your <typeparamref name="T" />.</para>
/// </remarks>
/// <typeparam name="T">your base goo type, it is better to make it as a <see cref="GH_PersistentParam{T}" />.</typeparam>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
[Conditional(Constant.KeepAttributes)]
public class BaseGooAttribute<T> : Attribute where T : IGH_Goo;