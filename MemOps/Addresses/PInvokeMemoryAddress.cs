using System.Runtime.InteropServices;
using MemOps.Enums;
using MemOps.Ops;

namespace MemOps.Addresses;

/// <summary>
/// A memory address for PInvoke reading/writing
/// </summary>
/// <typeparam name="T">Type of object pointed to</typeparam>
public class PInvokeMemoryAddress<T>
    where T : unmanaged
{
    /// <summary>
    /// Safe handle used to PInvoke Read/WriteProcessMemory
    /// </summary>
    public SafeHandle Handle { get; init; }
    /// <summary>
    /// Access rights used to read/write
    /// </summary>
    public ProcessAccessRights AccessRights { get; set; }
    /// <summary>
    /// Unmanaged memory address that returns a span over the memory instead of
    /// individual PInvoke calls for each read
    /// </summary>
    public MemoryAddress<T> MemoryAddress { get; set; }

    /// <summary>
    /// Initializes all necessary members to use PInvoke calls for reading/writing
    /// </summary>
    /// <param name="memoryAddress">Address to store</param>
    /// <param name="handle">Handle to process</param>
    /// <param name="accessRights">Access rights needed</param>
    public PInvokeMemoryAddress(MemoryAddress<T> memoryAddress, SafeHandle handle, ProcessAccessRights accessRights)
    {
        Handle = handle;
        AccessRights = accessRights;
        MemoryAddress = memoryAddress;
    }

    /// <summary>
    /// Reads 1 T at the address
    /// </summary>
    /// <param name="v">The ref to read to</param>
    public unsafe void Read(out T v)
    {
        MemoryOps.Read(Handle, MemoryAddress.Pointer.ToPointer(), out v);
    }

    /// <summary>
    /// Reads an amount of T depending on the length of the span
    /// </summary>
    /// <param name="span">Buffer to store read values</param>
    public unsafe void ReadMultiple(Span<T> span)
    {
        MemoryOps.ReadMultiple(Handle, MemoryAddress.Pointer.ToPointer(), span);
    }

    // TODO: Make in instead of ref
    /// <summary>
    /// Writes 1 T to the address
    /// </summary>
    /// <param name="v">T object to write</param>
    public unsafe void Write(ref T v)
    {
        MemoryOps.Write(Handle, MemoryAddress.Pointer.ToPointer(), ref v);
    }
}