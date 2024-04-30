using System.Buffers;
using System.Runtime.InteropServices;

namespace MemOps.Addresses;

/// <summary>
/// A class used for keeping track of unmanaged objects
/// in memory in an object-oriented way. Unmanaged memory
/// should be disposed of manually when no longer needed.
/// </summary>
/// <typeparam name="T">Type pointed to</typeparam>
public unsafe class MemoryAddress<T> : MemoryManager<T>
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
    /// Reads a single T object at the address
    /// </summary>
    /// <returns></returns>
    public T Read()
    {
        return *(T*)Pointer;
    }

    /// <summary>
    /// Used to retrieve all T objects that exist at the address
    /// </summary>
    /// <returns>A span covering all T objects from the address and length</returns>
    public override Span<T> GetSpan()
    {
        return new Span<T>(Pointer.ToPointer(), Length);
    }

    public Span<TTo> GetSpan<TTo>() where TTo : unmanaged
    {
        return MemoryMarshal.Cast<T, TTo>(GetSpan());
    }
    
    /// <summary>
    /// Does nothing. Unmanaged memory should be disposed of manually
    /// to prevent unwanted side effects.
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
    }

    /// <summary>
    /// Not used
    /// </summary>
    /// <param name="elementIndex"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override MemoryHandle Pin(int elementIndex = 0)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Not used
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public override void Unpin()
    {
        throw new NotImplementedException();
    }

    public static implicit operator Span<T>(MemoryAddress<T> memAddr) => memAddr.GetSpan();
    public static implicit operator ReadOnlySpan<T>(MemoryAddress<T> memAddr) => memAddr.GetSpan();
}