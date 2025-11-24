namespace TedToolkit.Console.Wrapper;

unsafe partial class FunctionPointer2
{
    public void Test2(int value, FunctionPointer2_Test_method_Delegate method1)
    {
        var __method =
            (delegate* unmanaged[Cdecl]<FunctionPointer2.Data**, global::System.IntPtr>)Loader.GetFunctionPointer(
                "FunctionPointer2_Create_0");
        fixed (Data** ptr = &Ptr)
            ThrowIfError(__method(ptr));
    }
}