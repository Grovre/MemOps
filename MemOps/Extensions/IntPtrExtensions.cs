using System.Runtime.InteropServices;
using MemOps.Addresses;

namespace MemOps.Extensions;

public static class IntPtrExtensions
{
    /// <summary>
    /// Converts an IntPtr to a MemoryAddress
    /// </summary>
    /// <param name="ptr">Pointer to memory</param>
    /// <param name="length">Length of elements at pointer</param>
    /// <param name="hasOwnership">Memory disposed of when MemoryAddress disposed of</param>
    /// <typeparam name="T">Unmanaged type of elements</typeparam>
    /// <returns>New MemoryAddress of provided pointer and length</returns>
    public static MemoryAddress<T> ToMemoryAddress<T>(this IntPtr ptr, int length, bool hasOwnership) where T : unmanaged
    {
        return new MemoryAddress<T>(ptr, length, hasOwnership);
    }
}