using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace TedToolkit.Grasshopper.Instance;

public enum Ex
{
    A,
    B,
    C,
    D,
    E
}

[Category("Cate")]
public static class ExampleComponents
{
    [ObjGuid("692328E7-D68C-4248-B5E0-8879DFEA97B4")]
    [Subcategory("SubTest")]
    [Exposure(GH_Exposure.quarternary)]
    [ObjNames("Name", "Nickname", "Description")]
    [DocObj("Your Name")]
    [return: ObjNames("Result", "r", "Some interesting result")]
    public static List<int> TestClass(
        GH_Component component,
        IGH_DataAccess da,
        [Optional] [ObjNames("OK", "B", "C")] Ex e,
        [Optional] [Hidden] [ObjNames("Input", "I", "An input")]
        ref Arc i,
        [Optional] [ObjField(true)] ref int myData,
        [Optional] List<int> test,
        [ParamTag(ParamTagType.Principal | ParamTagType.Flatten)] [Optional] [ParamType<Param_Integer>]
        ref Io<GH_Structure<GH_Integer>> tree,
        out GH_Structure<GH_Number> numbers)
    {
        numbers = new GH_Structure<GH_Number>();
        numbers.Append(new GH_Number(1));

        return [];
    }
}