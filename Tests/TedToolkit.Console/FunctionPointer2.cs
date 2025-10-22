namespace TedToolkit.Console.Wrapper;

unsafe partial class FunctionPointer2
{
    public void Test2(int value, FunctionPointer2_Test_method_Delegate method1)
    {
        SafeRun(loader =>
        {
            var method = (delegate* unmanaged[Cdecl]<FunctionPointer2.Data*, int, FunctionPointer2_Test_method_Delegate, global::System.IntPtr> )loader.GetFunctionPointer("FunctionPointer2_Test");
            return method(Ptr, value, method1);
        });
    }
}