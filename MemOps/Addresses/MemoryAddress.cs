using System.Buffers;
using System.Runtime.InteropServices;

namespace MemOps.Addresses;

/// <summary>
/// A class used for keeping track of unmanaged objects
/// in memory in an object-oriented way. Unmanaged memory
/// should be disposed of manually when no longer needed.
/// </summary>
/// <typeparam name="T">Type pointed to</typeparam>
public unsafe class MemoryAddress<T>
    where T : unmanaged
{
    /// <summary>
    /// Pointer to the object(s)
    /// </summary>
    public readonly IntPtr Pointer;
    /// <summary>
    /// How many T objects are at the address
    /// </summary>
    public readonly int Length;

    /// <summary>
    /// Allargs constructor
    /// </summary>
    /// <param name="pointer">Pointer to the object(s)</param>
    /// <param name="length">Length in T of how many T objects exist at the address</param>
    public MemoryAddress(IntPtr pointer, int length)
    {
        Pointer = pointer;
        Length = length;
    }

    /// <summary>
    /// Reads the T span at the address into a buffer
    /// </summary>
    /// <param name="buffer">Buffer to write to</param>
    public virtual void Read(Span<T> buffer)
    {
        AsSpan().CopyTo(buffer);
    }

    /// <summary>
    /// Writes the buffer to the T span at the memory address
    /// </summary>
    /// <param name="buffer">Buffer to read from</param>
    public virtual void Write(ReadOnlySpan<T> buffer)
    {
        buffer.CopyTo(AsSpan());
    }

    /// <summary>
    /// Creates a T span of the given length over the address
    /// </summary>
    /// <returns>T span with the given length</returns>
    public virtual Span<T> AsSpan()
    {
        return new Span<T>(Pointer.ToPointer(), Length);
    }
}