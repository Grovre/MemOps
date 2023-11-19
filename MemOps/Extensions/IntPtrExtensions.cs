using System.Runtime.InteropServices;
using MemOps.DataStructures;

namespace MemOps.Extensions;

public static class IntPtrExtensions
{
    /// <summary>
    ///     Creates a BufferedMemoryAddress from the nint address.
    ///     The buffer will be empty.
    /// </summary>
    /// <param name="address">Address to set for BufferedMemoryAddress</param>
    /// <param name="handle">Handle for memory ops with</param>
    /// <param name="printOnReadOrWrite">Whether or not to print immediately after a read or write</param>
    /// <typeparam name="T">Struct type to interpret memory at address as</typeparam>
    /// <returns>New BufferedMemoryAddress instance</returns>
    public static BufferedMemoryAddress<T> ToAddress<T>(
        this nint address,
        SafeHandle handle,
        bool printOnReadOrWrite = false)
        where T : unmanaged
    {
        return new BufferedMemoryAddress<T>(handle, address, printOnReadOrWrite);
    }
}