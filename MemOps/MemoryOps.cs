using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace MemOps;

public static unsafe class MemoryOps
{
    public static ref T Read<T>(SafeHandle handle, void* baseAddress, out T bufferStruct)
        where T: struct
    {
        handle.IsMemoryValid();
        bufferStruct = default;
        var sz = (nuint)Marshal.SizeOf(bufferStruct);
        nuint readBytes = default;
        fixed (void* bufPtr = &bufferStruct)
        {
            var result = PInvoke.ReadProcessMemory(handle, baseAddress, bufPtr, sz, &readBytes);
            Debug.Assert(result);
        }
        Console.WriteLine($"Read {readBytes} bytes at 0x{((nint)baseAddress):X}");
        return ref bufferStruct;
    }

    public static void Write<T>(SafeHandle handle, void* baseAddress, ref T bufferStruct)
        where T: struct
    {
        handle.IsMemoryValid();
        var sz = (nuint)Marshal.SizeOf(bufferStruct);
        nuint writtenBytes = default;
        fixed (void* bufPtr = &bufferStruct)
        {
            var result = PInvoke.WriteProcessMemory(handle, baseAddress, bufPtr, sz, &writtenBytes);
            Debug.Assert(result);
        }
        Console.WriteLine($"Wrote {writtenBytes} bytes at 0x{((nint)baseAddress):X}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void IsMemoryValid(this SafeHandle handle)
    {
        Debug.Assert(handle != null);
        Debug.Assert(!handle.IsInvalid);
        Debug.Assert(!handle.IsClosed);
    }
}