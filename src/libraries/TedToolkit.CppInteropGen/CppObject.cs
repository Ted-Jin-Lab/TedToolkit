using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace TedToolkit.CppInteropGen;

public abstract class CppObject(bool disposed = false) : IDisposable
{
    private bool _disposed = disposed;
    protected abstract string FileName { get; }

    private string FileFullPath
    {
        get
        {
            var fileName = GetLibraryFileName(FileName);
            var basePath = AppContext.BaseDirectory;
            var fullPath = Path.Combine(basePath, fileName);
            if (File.Exists(fullPath)) return fullPath;

            var ridPath = Path.Combine(basePath, "runtimes", GetRid(), "native", fileName);
            if (File.Exists(ridPath)) return ridPath;

            throw new DllNotFoundException($"Native library not found: {fullPath} or {ridPath}");

            static string GetRid()
            {
                var prefix =
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" :
                    RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" :
                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" :
                    throw new PlatformNotSupportedException("Unsupported OS platform");

                return prefix + "-" + RuntimeInformation.OSArchitecture.ToString().ToLower();
            }

            static string GetLibraryFileName(string baseName)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return baseName + ".dll";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return "lib" + baseName + ".so";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return "lib" + baseName + ".dylib";

                throw new PlatformNotSupportedException("Unsupported OS platform");
            }
        }
    }

    void IDisposable.Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
        Dispose();
        Delete();
    }

    [DebuggerStepThrough]
    protected void SafeRun(Func<NativeFunctionLoader, IntPtr> func)
    {
        var loader = NativeFunctionLoader.GetLoader(FileFullPath);
        var errorPtr = func(loader);
        if (errorPtr == IntPtr.Zero) return;
        throw new CppException(GetStringFromIntPtr(errorPtr, loader));

        static unsafe string GetStringFromIntPtr(IntPtr errorPtr, NativeFunctionLoader loader)
        {
            var message = Marshal.PtrToStringAnsi(errorPtr) ?? "Unknown error";
            ((delegate* unmanaged[Cdecl]<IntPtr, void>)loader.GetFunctionPointer("free_error"))(errorPtr);
            return message;
        }
    }

    protected virtual void Dispose()
    {
    }

    protected abstract void Delete();

    public sealed class CppException(string message) : Exception(message)
    {
        private static readonly Regex Matcher = new(@"\[(.*?)\]");

        public string ExceptionType
        {
            get
            {
                var match = Matcher.Match(Message);
                return match.Success ? match.Groups[1].Value : "Unknown";
            }
        }
    }
}