using System.Runtime.InteropServices;
using MemOps.DataStructures;

namespace MemOps.Extensions;

public static class IntPtrExtensions
{
    public static BufferedMemoryAddress<T> ToAddress<T>(
        this nint address, 
        SafeHandle handle,
        bool printOnReadOrWrite = false) 
        where T : struct => new BufferedMemoryAddress<T>(handle, address, printOnReadOrWrite);
}