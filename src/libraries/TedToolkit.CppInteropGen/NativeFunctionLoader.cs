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
	private readonly IntPtr _libHandle;
	private bool _disposed;

	private NativeFunctionLoader(string libraryPath)
	{
		_libHandle = NativeLibrary.Load(libraryPath);
		if (_libHandle != IntPtr.Zero) return;
		throw new InvalidOperationException($"Failed to load native library: {libraryPath}");
	}

	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;
		NativeLibrary.Free(_libHandle);
	}

	internal static NativeFunctionLoader GetLoader(string libName)
	{
		if (string.IsNullOrEmpty(libName))
			throw new ArgumentException("The libName is null or empty.", nameof(libName));

		return Loaders.AddOrUpdate(
			key: libName,
			addValueFactory: n => new(n),
			updateValueFactory: (n, existingLoader) => existingLoader._disposed ? new(n) : existingLoader);
	}

	public IntPtr GetFunctionPointer(string name)
	{
		if (_disposed) throw new ObjectDisposedException(nameof(NativeFunctionLoader));
		return _exportCache.GetOrAdd(name, n => NativeLibrary.GetExport(_libHandle, n));
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