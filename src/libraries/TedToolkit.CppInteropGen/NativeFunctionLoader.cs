using System.Collections.Concurrent;
#if NETSTANDARD || NETFRAMEWORK
#else
using System.Runtime.InteropServices;
#endif

namespace TedToolkit.CppInteropGen;

public sealed class NativeFunctionLoader : IDisposable
{
    private static readonly ConcurrentDictionary<string, NativeFunctionLoader> Loaders = [];
    private readonly ConcurrentDictionary<string, IntPtr> _exportCache = new(StringComparer.Ordinal);
    private readonly Lazy<IntPtr> _libHandle;
    private bool _disposed;

    private NativeFunctionLoader(string libraryPath)
    {
        _libHandle = new Lazy<IntPtr>(() =>
        {
            var handle = NativeLibrary.Load(libraryPath);
            if (handle != IntPtr.Zero) return handle;
            throw new InvalidOperationException($"Failed to load native library: {libraryPath}");
        });
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_libHandle.IsValueCreated) NativeLibrary.Free(_libHandle.Value);
    }

    internal static NativeFunctionLoader GetLoader(string libName)
    {
        if (string.IsNullOrEmpty(libName))
            throw new ArgumentException("The libName is null or empty.", nameof(libName));
        var loader = Loaders.GetOrAdd(libName, n => new NativeFunctionLoader(n));
        if (!loader._disposed) return loader;
        return Loaders[libName] = new NativeFunctionLoader(libName);
    }

    public IntPtr GetFunctionPointer(string name)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(NativeFunctionLoader));
        return _exportCache.GetOrAdd(name, n => NativeLibrary.GetExport(_libHandle.Value, n));
    }
}

#if NETSTANDARD || NETFRAMEWORK
internal static  class NativeLibrary
{
    private static readonly ConcurrentDictionary<IntPtr, NativeLibraryLoader.NativeLibrary> LoadedLibraries = new();

    public static IntPtr Load(string libraryPath)
    {
        var lib = new NativeLibraryLoader.NativeLibrary(libraryPath);
        var handle = lib.Handle;
        LoadedLibraries[handle] = lib;
        return handle;
    }

    public static IntPtr GetExport(IntPtr handle, string name)
    {
        if (!LoadedLibraries.TryGetValue(handle, out var lib))
            throw new InvalidOperationException("Library not found. Make sure it's loaded with NativeLibrary.Load.");

        return lib.LoadFunction(name);
    }

    public static void Free(IntPtr handle)
    {
        if (LoadedLibraries.TryRemove(handle, out var lib))
        {
            lib.Dispose();
        }
        else
        {
            throw new InvalidOperationException("Library handle not found or already freed.");
        }
    }
}
#endif