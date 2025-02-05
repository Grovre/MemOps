using System.Runtime.InteropServices;
using MemOps.Enums;
using MemOps.Ops;

namespace MemOps.Addresses;

/// <summary>
/// A memory address for PInvoke reading/writing
/// </summary>
/// <typeparam name="T">Type of object pointed to</typeparam>
public class PInvokeMemoryAddress<T> : MemoryAddress<T>
    where T : unmanaged
{
    private SafeHandle Handle { get; }

    /// <summary>
    /// Creates a new PInvokeMemoryAddress for use with PInvoke
    /// </summary>
    /// <param name="hProc">A SafeHandle to another process</param>
    /// <param name="pointer">Address of memory</param>
    /// <param name="length">Length of T elements at the address</param>
    public PInvokeMemoryAddress(SafeHandle hProc, nint pointer, int length) : base(pointer, length)
    {
        Handle = hProc;
    }

    /// <summary>
    /// Reads the T span at the address into a buffer
    /// </summary>
    /// <param name="buffer">Buffer to read into</param>
    public override unsafe void Read(Span<T> buffer)
    {
        MemoryOps.ReadMultiple(Handle, Pointer.ToPointer(), buffer);
    }

    /// <summary>
    /// Writes the buffer to the T span at the memory address
    /// </summary>
    /// <param name="buffer">Buffer to read from</param>
    public override unsafe void Write(ReadOnlySpan<T> buffer)
    {
        MemoryOps.WriteMultiple(Handle, Pointer.ToPointer(), buffer);
    }

    /// <summary>
    /// Cannot get a span over memory in another process.
    /// </summary>
    /// <returns>Will not return</returns>
    /// <exception cref="NotSupportedException"></exception>
    public override Span<T> AsSpan()
    {
        throw new NotSupportedException(
            "Cannot get a span over memory in another process.");
    }
}