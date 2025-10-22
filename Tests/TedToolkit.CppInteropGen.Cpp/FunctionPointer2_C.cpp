#include <csharp_interop.h>

//FILE_NAME: TedToolkit_CppInteropGen_Cpp

typedef int FunctionPointer2;

CSHARP_WRAPPER(FunctionPointer2_Create_0(FunctionPointer2*& handle), {
               handle = new FunctionPointer2();
               })

CSHARP_WRAPPER(FunctionPointer2_Test(const FunctionPointer2& handle, const int value, int(*method)(int a, int b)), {
               method(1, 2);
               })

CSHARP_WRAPPER(FunctionPointer2_Delete(const FunctionPointer2* self), {
               delete self;
               })
