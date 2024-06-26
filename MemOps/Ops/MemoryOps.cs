﻿// Who would even use Windows below 7? On .NET 7? No way lol

#pragma warning disable CA1416

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;
using MemOps.Exceptions;

namespace MemOps.Ops;

/// <summary>
///     Static class providing access to external Win32 calls.
///     It is strongly recommended to use BufferedMemoryAddress.
/// </summary>
public static unsafe class MemoryOps
{
    /// <summary>
    ///     The lowest form of the Read functions provided by MemoryOps. It is strongly recommended to use
    ///     the "ReadMultiple" function over this which, if the final type you intend to get out of this
    ///     is not actually byte or sbyte, will abstract away the whole process of going from a span of
    ///     the given type to a span of bytes.
    /// </summary>
    /// <param name="handle">Handle with access to read with</param>
    /// <param name="baseAddress">Address in memory to read</param>
    /// <param name="span">Span to write the read bytes to</param>
    /// <param name="printOnRead">Whether to print a message to the console immediately after reading</param>
    /// <exception cref="MemoryException">Thrown if the ReadProcessMemory function returned nonzero (failed)</exception>
    /// <returns>The amount of bytes that were read</returns>
    public static nuint ReadBytes(SafeHandle handle, void* baseAddress, Span<byte> span, bool printOnRead = false)
    {
        handle.AssertHandleIsValidDebug();
        var sz = (nuint)span.Length;
        nuint byteReadCount = default;
        fixed (void* bufPtr = span)
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
            Console.WriteLine($"Read {byteReadCount} bytes at 0x{(nint)baseAddress:X}");

        return byteReadCount;
    }

    /// <summary>
    ///     Provides a way to interact with the lower ReadProcessMemory function from the
    ///     Win32 API.
    /// </summary>
    /// <param name="handle">Handle to read memory with</param>
    /// <param name="baseAddress">Address to read into the buffer</param>
    /// <param name="bufferStruct">Buffer for reading into</param>
    /// <param name="printOnRead">Whether or not to print immediately after reading from memory</param>
    /// <typeparam name="T">The struct type to interpret the memory as</typeparam>
    public static void Read<T>(SafeHandle handle, void* baseAddress, out T bufferStruct, bool printOnRead = false)
        where T : unmanaged
    {
        bufferStruct = default;
        var bufferSpan = MemoryMarshal.CreateSpan(ref bufferStruct, 1);
        ReadMultiple(handle, baseAddress, bufferSpan, printOnRead);
    }

    /// <summary>
    ///     Provides a way to interact with the lower ReadProcessMemory function from the
    ///     Win32 API. This will read contiguously the same amount of values as there are
    ///     indices in the given span, to the given span starting from the base address.
    /// </summary>
    /// <param name="handle">Handle to read memory with</param>
    /// <param name="baseAddress">Address to read into the buffer</param>
    /// <param name="bufferSpan">Buffer for reading into</param>
    /// <param name="printOnRead">Whether or not to print immediately after reading from memory</param>
    /// <typeparam name="T">The struct type to interpret the memory as</typeparam>
    public static void ReadMultiple<T>(SafeHandle handle, void* baseAddress, Span<T> bufferSpan,
        bool printOnRead = false)
        where T : unmanaged
    {
        var byteSpan = MemoryMarshal.Cast<T, byte>(bufferSpan);
        ReadBytes(handle, baseAddress, byteSpan, printOnRead);
    }

    /// <summary>
    ///     Continuously reads an address until stopped. This should be started in a
    ///     separate thread or task.
    ///     This function accepts a ReaderWriterLockSlim in order to synchronize reads from writes.
    ///     It uses an internal buffer of T to read into, only locking to copy the internal buffer to
    ///     the given reference buffer to achieve the shortest write locking time possible.
    ///     Another important parameter is for a cancellation token to stop the loop. If one is not
    ///     given, the loop will go on forever until killed. The token is always checked before
    ///     the thread goes to sleep for the given duration. If the token is called while asleep,
    ///     one last read will be done.
    /// </summary>
    /// <param name="handle">Handle to read memory with</param>
    /// <param name="baseAddress">Address to read memory at</param>
    /// <param name="bufferStruct">Buffer for reading into</param>
    /// <param name="delayBetweenReads">Time between cycles</param>
    /// <param name="rwLock">Optional slim lock to allow multiple reads but only a single write at a time</param>
    /// <param name="cancelToken">Token to stop the reading cycle</param>
    /// <param name="printOnRead">Whether or not to print a message to the console immediately after reading</param>
    /// <typeparam name="T">Struct type the memory is interpreted as</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the cycle interval is too short (le 0)</exception>
    /// <exception cref="ArgumentException">Thrown if the lock took more than 10 seconds to enter</exception>
    public static void ReadCycle<T>(
        SafeHandle handle,
        void* baseAddress,
        ref T bufferStruct,
        TimeSpan delayBetweenReads,
        ReaderWriterLockSlim? rwLock = null,
        CancellationToken? cancelToken = null,
        bool printOnRead = false)
        where T : unmanaged
    {
        if (delayBetweenReads <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(
                nameof(delayBetweenReads),
                "Delay must be above 0. Little to no delay will be unpredictable.");

        while (!cancelToken?.IsCancellationRequested ?? true)
        {
            Thread.Sleep(delayBetweenReads); // Use Task.Delay.Wait instead?

            // Just in case
            // https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes--0-499-
            Read(handle, baseAddress, out T internalBuffer, printOnRead);

            var entered = rwLock?.TryEnterWriteLock(TimeSpan.FromSeconds(10));
            if (entered.HasValue && !entered.Value) // If no value then there was no lock
                throw new ArgumentException("Timed out after 10 seconds trying to enter write lock");
            bufferStruct = internalBuffer; // Do not inline Read, make lock as fast as possible
            rwLock?.ExitWriteLock();
        }
    }

    /// <summary>
    ///     The lowest form of the Write functions provided by MemoryOps. It is strongly recommended to use
    ///     the "ReadMultiple" function over this which, if the final type you intend to write is not
    ///     actually byte or sbyte, it will abstract away the whole process of going from a span of
    ///     the given type to a span of bytes.
    /// </summary>
    /// <param name="handle">Handle with access to write with</param>
    /// <param name="baseAddress">Address in memory to write</param>
    /// <param name="bytes">Byte source</param>
    /// <param name="printOnWrite">Whether to print a message to the console immediately after writing</param>
    /// <exception cref="MemoryException">Thrown if the WriteProcessMemory function returned nonzero (failed)</exception>
    /// <returns>The amount of bytes that were written</returns>
    public static nuint WriteBytes(SafeHandle handle, void* baseAddress, ReadOnlySpan<byte> bytes,
        bool printOnWrite = false)
    {
        handle.AssertHandleIsValidDebug();
        var sz = (nuint)bytes.Length;
        nuint byteWriteCount = default;
        fixed (void* bufPtr = bytes)
        {
            var result = PInvoke.WriteProcessMemory(handle, baseAddress, bufPtr, sz, &byteWriteCount);
            if (!result)
            {
                var code = Marshal.GetLastPInvokeError();
                var message = $"WriteProcessMemory failed with error {code} (0x{code:X}). See MSDN for more info";
                throw new MemoryException(message, code);
            }
        }

        if (printOnWrite)
            Console.WriteLine($"Wrote {byteWriteCount} bytes at 0x{(nint)baseAddress:X}");

        return byteWriteCount;
    }

    /// <summary>
    ///     Provides a way to interact with the lower WriteProcessMemory function from the
    ///     Win32 API.
    /// </summary>
    /// <param name="handle">Handle to write memory with</param>
    /// <param name="baseAddress">Address to write to</param>
    /// <param name="bufferStruct">Data source</param>
    /// <param name="printOnWrite">Whether or not to print immediately after writing from memory</param>
    /// <typeparam name="T">The struct type to interpret the memory as</typeparam>
    public static void Write<T>(SafeHandle handle, void* baseAddress, ref T bufferStruct, bool printOnWrite = false)
        where T : unmanaged
    {
        var span = MemoryMarshal.CreateReadOnlySpan(ref bufferStruct, 1);
        WriteMultiple(handle, baseAddress, span, printOnWrite);
    }

    /// <summary>
    ///     Provides a way to interact with the lower WriteProcessMemory function from the
    ///     Win32 API. This will write contiguously the same amount of values as there are
    ///     indices in the given span, to the given span starting from the base address.
    /// </summary>
    /// <param name="handle">Handle to write memory with</param>
    /// <param name="baseAddress">Address to write from buffer source</param>
    /// <param name="bufferSpan">Writing buffer source</param>
    /// <param name="printOnWrite">Whether or not to print immediately after writing from memory</param>
    /// <typeparam name="T">The struct type to interpret the memory as</typeparam>
    public static void WriteMultiple<T>(SafeHandle handle, void* baseAddress, ReadOnlySpan<T> bufferSpan,
        bool printOnWrite = false) where T : unmanaged
    {
        var byteSpan = MemoryMarshal.AsBytes(bufferSpan);
        WriteBytes(handle, baseAddress, byteSpan, printOnWrite);
    }

    /// <summary>
    ///     Makes sure a handle is not null,
    ///     A handle is not invalid,
    ///     And a handle is not closed.
    ///     If any of the above are true, returns false.
    ///     Otherwise, returns true.
    /// </summary>
    /// <param name="handle"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsHandleValid(this SafeHandle handle)
    {
        return handle is { IsInvalid: false, IsClosed: false };
    }

    /// <summary>
    ///     Asserts a handle is not valid according
    ///     to the <code>IsHandleValid</code> function.
    /// </summary>
    /// <param name="handle">The handle to check</param>
    /// <exception cref="MemoryException">Thrown when the handle is invalid</exception>
    public static void AssertHandleIsValid(this SafeHandle handle)
    {
        if (!IsHandleValid(handle))
            throw new MemoryException("Invalid handle");
    }

    /// <summary>
    ///     Asserts a handle is not valid according
    ///     to the <code>IsHandleValid</code> function.
    ///     Only runs when the DEBUG constant is defined.
    /// </summary>
    /// <param name="handle">The handle to check</param>
    /// <exception cref="MemoryException">Thrown when the handle is invalid</exception>
    [Conditional("DEBUG")]
    public static void AssertHandleIsValidDebug(this SafeHandle handle)
    {
        AssertHandleIsValid(handle);
    }
}