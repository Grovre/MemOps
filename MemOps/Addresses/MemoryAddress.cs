using System.Buffers;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace MemOps.Addresses;

/// <summary>
/// A class used for keeping track of unmanaged objects
/// in memory in an object-oriented way
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
    /// Determines if the memory should be released when
    /// the MemoryAddress object is disposed of.
    /// </summary>
    public readonly bool HasOwnership;

    /// <summary>
    /// Allargs constructor
    /// </summary>
    /// <param name="pointer">Pointer to the object(s)</param>
    /// <param name="length">Length in T of how many T objects exist at the address</param>
    /// <param name="hasOwnershipOfMemory">Releases the memory if this is true when this instance is disposed</param>
    public MemoryAddress(IntPtr pointer, int length, bool hasOwnershipOfMemory)
    {
        Pointer = pointer;
        Length = length;
        HasOwnership = hasOwnershipOfMemory;
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
    /// Disposes of the memory if this object has ownersihp
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        if (HasOwnership)
        {
            Marshal.FreeHGlobal(Pointer);
        }
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