// Who would even use Windows below 7? On .NET 7? No way lol
#pragma warning disable CA1416

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;

namespace MemOps;

public static unsafe class MemoryOps
{
    public static void Read<T>(SafeHandle handle, void* baseAddress, out T bufferStruct, bool printOnRead = false)
        where T: struct
    {
        handle.IsMemoryValid();
        bufferStruct = default;
        var sz = (nuint)Marshal.SizeOf(bufferStruct);
        nuint byteReadCount = default;
        fixed (void* bufPtr = &bufferStruct)
        {
            var result = PInvoke.ReadProcessMemory(handle, baseAddress, bufPtr, sz, &byteReadCount);
            if (!result)
            {
                var code = Marshal.GetLastPInvokeError();
                var message = $"ReadProcessMemory failed with error {code}";
                throw new MemoryException(message, code);
            }
        }
        
        if (printOnRead)
            Console.WriteLine($"Read {byteReadCount} bytes at 0x{((nint)baseAddress):X}");
    }

    public static void ReadCycle<T>(SafeHandle handle, void* baseAddress, ref T bufferStruct, ReaderWriterLockSlim rwLock, TimeSpan delayBetweenReads, CancellationToken? cancelToken, bool writeOnRead = false)
        where T : struct
    {
        if (delayBetweenReads <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(delayBetweenReads),
                "Delay must be above 0. Little to no delay will be unpredictable.");
        }
        
        while (!cancelToken?.IsCancellationRequested ?? true)
        {
            Thread.Sleep(delayBetweenReads); // Use Task.Delay.Wait instead?

            // Just in case
            // https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes--0-499-
            Read(handle, baseAddress, out T internalBuffer, writeOnRead);

            rwLock.EnterWriteLock(); // TODO: Use try method for clarity to misusers
            bufferStruct = internalBuffer; // Do not inline Read, make lock as fast as possible
            rwLock.ExitWriteLock();
        }
    }

    public static void Write<T>(SafeHandle handle, void* baseAddress, ref T bufferStruct, bool printOnWrite = false)
        where T: struct
    {
        handle.IsMemoryValid();
        var sz = (nuint)Marshal.SizeOf(bufferStruct);
        nuint byteWriteCount = default;
        fixed (void* bufPtr = &bufferStruct)
        {
            var result = PInvoke.WriteProcessMemory(handle, baseAddress, bufPtr, sz, &byteWriteCount);
            if (!result)
            {
                var code = Marshal.GetLastPInvokeError();
                var message = $"WriteProcessMemory failed with error {code}. See MSDN for more info";
                throw new MemoryException(message, code);
            }
        }
        
        if (printOnWrite)
            Console.WriteLine($"Wrote {byteWriteCount} bytes at 0x{((nint)baseAddress):X}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void IsMemoryValid(this SafeHandle handle)
    {
        Debug.Assert(handle != null);
        Debug.Assert(!handle.IsInvalid);
        Debug.Assert(!handle.IsClosed);
    }
}