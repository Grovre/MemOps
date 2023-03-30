// Who would even use Windows below 7? On .NET 7? No way lol
#pragma warning disable CA1416

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;
using MemOps.Exceptions;

namespace MemOps.Ops;

/// <summary>
/// Static class providing access to external Win32 calls.
/// It is strongly recommended to use BufferedMemoryAddress.
/// </summary>
public static unsafe class MemoryOps
{
    /// <summary>
    /// The lowest form of the Read functions provided by MemoryOps. It is strongly recommended to use
    /// the "ReadMultiple" function over this which, if the final type you intend to get out of this
    /// is not actually byte or sbyte, will abstract away the whole process of going from a span of
    /// the given type to a span of bytes.
    /// </summary>
    /// <param name="handle">Handle with access to read with</param>
    /// <param name="baseAddress">Address in memory to read</param>
    /// <param name="span">Span to write the read bytes to</param>
    /// <param name="printOnRead">Whether to print a message to the console immediately after reading</param>
    /// <exception cref="MemoryException">Thrown if the ReadProcessMemory function returned nonzero (failed)</exception>
    /// <returns>The amount of bytes that were read</returns>
    public static nuint ReadBytes(SafeHandle handle, void* baseAddress, Span<byte> span, bool printOnRead = false)
    {
        handle.IsHandleValid();
        var sz = (nuint)span.Length;
        nuint byteReadCount = default;
        ref var b0 = ref MemoryMarshal.GetReference(span);
        fixed (void* bufPtr = &b0)
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
        
        return byteReadCount;
    }
    
    /// <summary>
    /// Provides a way to interact with the lower ReadProcessMemory function from the
    /// Win32 API.
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
    /// Provides a way to interact with the lower ReadProcessMemory function from the
    /// Win32 API. This will read contiguously the same amount of values as there are
    /// indices in the given span, to the given span starting from the base address.
    /// </summary>
    /// <param name="handle">Handle to read memory with</param>
    /// <param name="baseAddress">Address to read into the buffer</param>
    /// <param name="bufferSpan">Buffer for reading into</param>
    /// <param name="printOnRead">Whether or not to print immediately after reading from memory</param>
    /// <typeparam name="T">The struct type to interpret the memory as</typeparam>
    public static void ReadMultiple<T>(SafeHandle handle, void* baseAddress, Span<T> bufferSpan, bool printOnRead = false)
        where T : unmanaged
    {
        var byteSpan = MemoryMarshal.AsBytes(bufferSpan);
        ReadBytes(handle, baseAddress, byteSpan, printOnRead);
    }

    /// <summary>
    /// Continuously reads an address until stopped. This should be started in a
    /// separate thread or task.
    ///
    /// This function accepts a ReaderWriterLockSlim in order to synchronize reads from writes.
    /// It uses an internal buffer of T to read into, only locking to copy the internal buffer to
    /// the given reference buffer to achieve the shortest write locking time possible.
    ///
    /// Another important parameter is for a cancellation token to stop the loop. If one is not
    /// given, the loop will go on forever until killed. The token is always checked before
    /// the thread goes to sleep for the given duration. If the token is called while asleep,
    /// one last read will be done.
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
            Read(handle, baseAddress, out T internalBuffer, printOnRead);

            var entered = rwLock?.TryEnterWriteLock(TimeSpan.FromSeconds(10));
            if (entered.HasValue && !entered.Value) // If no value then there was no lock
                throw new ArgumentException("Timed out after 10 seconds trying to enter write lock");
            bufferStruct = internalBuffer; // Do not inline Read, make lock as fast as possible
            rwLock?.ExitWriteLock();
        }
    }
    
    /// <summary>
    /// Writes from the buffer into the given address.
    /// </summary>
    /// <param name="handle">Handle to write with</param>
    /// <param name="baseAddress">Address to write to</param>
    /// <param name="bufferStruct">Buffer to write with</param>
    /// <param name="printOnWrite">Whether or not to print a message immediately after writing</param>
    /// <typeparam name="T">Struct type the memory is interpreted as</typeparam>
    /// <exception cref="MemoryException">Thrown if WriteProcessMemory failed</exception>
    public static void Write<T>(SafeHandle handle, void* baseAddress, ref T bufferStruct, bool printOnWrite = false)
        where T : unmanaged
    {
        handle.IsHandleValid();
        var sz = (nuint)Marshal.SizeOf(bufferStruct);
        nuint byteWriteCount = default;
        fixed (void* bufPtr = &bufferStruct)
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
            Console.WriteLine($"Wrote {byteWriteCount} bytes at 0x{((nint)baseAddress):X}");
    }

/// <summary>
/// Follows offsets for a nint by doing offset-pointer traversal.
/// This adds the offset to the address, dereferences it, and repeats until
/// all offsets have been added to the dereferenced pointers and the final
/// address has been reached.
///
/// If there is only one offset provided for params, this will only add the offset.
/// 
/// This function works exactly like how Cheat Engine follows pointers.
/// </summary>
/// <param name="handle">Handle to read with</param>
/// <param name="baseAddress">Address to start from</param>
/// <param name="offsets">Offsets to follow</param>
/// <returns>The final address as nint</returns>
    public static nint FollowOffsets(SafeHandle handle, nint baseAddress, params nint[] offsets)
    {
        if (offsets.Length <= 0)
            return baseAddress;
        if (offsets.Length == 1)
            return baseAddress + offsets[0];
        
        // Easier to debug than LINQ
        for (var i = 0; i < offsets.Length - 1; i++)
        {
            baseAddress += offsets[i];
            Read(handle, baseAddress.ToPointer(), out baseAddress);
        }
        
        return baseAddress + offsets[^1];
    }

    /// <summary>
    /// Purely for debugging, this ensures all boolean methods
    /// that check for an invalid handle pass. This uses a
    /// conditional attribute to avoid being called in release.
    /// If the program is in debug, it is aggressively inlined.
    /// After all, this is only three lines of code.
    ///
    /// Specifically:
    /// Makes sure a handle is not null,
    /// A handle is not invalid,
    /// And a handle is not closed.
    /// </summary>
    /// <param name="handle"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Conditional("DEBUG")]
    public static void IsHandleValid(this SafeHandle handle)
    {
        Debug.Assert(handle != null);
        Debug.Assert(!handle.IsInvalid);
        Debug.Assert(!handle.IsClosed);
    }
}