#ifndef CSHARP_INTEROP_H
#define CSHARP_INTEROP_H

#ifndef WRAP_CALL_CUSTOM_CATCH
#define WRAP_CALL_CUSTOM_CATCH
#endif

#ifdef _WIN32
#define API_EXPORT extern "C" __declspec(dllexport)
#else
#define API_EXPORT extern "C" __attribute__((visibility("default")))
#endif

#define CSHARP_WRAPPER(FUNC_DECL, BODY)   \
API_EXPORT inline char* FUNC_DECL {       \
return (wrap_call([&] BODY));             \
}

#include <string>
#include <functional>

inline char* copy_to_heap(const std::string& msg)
{
    const auto buffer = new char[msg.size() + 1];
    std::memcpy(buffer, msg.c_str(), msg.size() + 1);
    return buffer;
}

inline char* wrap_call(const std::function<void()>& action)
{
    try
    {
        action();
        return nullptr;
    }
    WRAP_CALL_CUSTOM_CATCH
    catch
    (const std::exception&
    ex
    )
    {
        const char* typeName = typeid(ex).name();
        return copy_to_heap(std::string("STD [") + typeName + "]: " + ex.what());
    }
    catch
    (...)
    {
        return copy_to_heap("Unknown error occurred.");
    }
}

API_EXPORT inline void free_error(const char* ptr)
{
    delete[] ptr;
}

#endif //CSHARP_INTEROP_H
