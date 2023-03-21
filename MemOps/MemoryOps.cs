using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;

namespace MemOps;

public static unsafe class MemoryOps
{
    public static void Read<T>(SafeHandle handle, void* baseAddress, out T bufferStruct)
        where T: struct
    {
        handle.IsMemoryValid();
        bufferStruct = default;
        var sz = (nuint)Marshal.SizeOf(bufferStruct);
        nuint readBytes = default;
        fixed (void* bufPtr = &bufferStruct)
        {
            var result = PInvoke.ReadProcessMemory(handle, baseAddress, bufPtr, sz, &readBytes);
            Debug.Assert(result, $"ReadProcessMemory failed with error {Marshal.GetLastPInvokeError()}");
        }
        Console.WriteLine($"Read {readBytes} bytes at 0x{((nint)baseAddress):X}");
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
            Debug.Assert(result, $"WriteProcessMemory failed with error {Marshal.GetLastPInvokeError()}");
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