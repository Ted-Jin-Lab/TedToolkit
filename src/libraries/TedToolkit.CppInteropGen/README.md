# CppInteropGen

> [!WARNING]  
> This tool is still work in progress.

## Brief

This is an easy tool to wrap the cpp file to c#.

## CPP

You should include the file `csharp_interop.h` to your cpp project.
And Define the Macro `WRAP_CALL_CUSTOM_CATCH` before include the `csharp_interop.h`.
Use the Macro `CSHARP_WRAPPER` To define the method you like.

use `//DLL_NAME:` to define the dll name.
ust `//STRUCT:` to define the struct layout.

### Namings

- The method name must be `%CLASSNAME%_%METHODNAME%_%EXTRA%`.
- And the file name must be `%CLASSNAME%_C` with the extension `h`, `cpp`, `hxx`, or `cxx`.
- Every file must have the definitions of `Create` and `Delete` for the ctor and the dtor.

### Example

Define your own `WRAP_CALL_CUSTOM_CATCH`.

```c++
#ifndef EXPORT_MACRO_H
#define EXPORT_MACRO_H
#include <Standard_Failure.hxx>
#include <string>
#ifndef WRAP_CALL_CUSTOM_CATCH
#define WRAP_CALL_CUSTOM_CATCH \
catch (const Standard_Failure &ex) { \
const std::string typeName = ex.DynamicType()->Name(); \
const std::string message = ex.GetMessageString(); \
return copy_to_heap("OCCT [" + typeName + "]: " + message); \
}
#endif
#include <csharp_interop.h>
#endif //EXPORT_MACRO_H
```

Add your own methods

```c++
#include <gp_Pnt.hxx>
#include "export_macro.h"

//DLL_NAME: EXAMPLE.dll
//STRUCT: gp_XYZ.Data coord

CSHARP_WRAPPER(gp_Pnt_Create_0(gp_Pnt*& handle), {
                    handle= new gp_Pnt();
                    })

CSHARP_WRAPPER(gp_Pnt_Create_coord(const gp_XYZ& xyz, gp_Pnt*& handle), {
                    handle= new gp_Pnt(xyz);
                    })

CSHARP_WRAPPER(gp_Pnt_Create_xyz(const double x, const double y, const double z, gp_Pnt*& handle), {
                    handle= new gp_Pnt(x, y, z);
                    })

CSHARP_WRAPPER(gp_Pnt_Delete(const gp_Pnt *self), {
                    delete self;
                    })

CSHARP_WRAPPER(gp_Pnt_ChangeCoord(gp_Pnt *self, gp_XYZ*& xyz), {
                    xyz = &self->ChangeCoord();
                    })

CSHARP_WRAPPER(gp_Pnt_Distance(const gp_Pnt *self, const gp_Pnt& other, double& distance), {
                    distance = self->Distance(other);
                    })

CSHARP_WRAPPER(gp_Pnt_SquareDistance(const gp_Pnt *self, const gp_Pnt& other, double& squareDistance), {
                    squareDistance = self->SquareDistance(other);
                    })
```

## C#

In C#, it'll generate the Wrappers for you. You can use `partial` keyword to add more things.