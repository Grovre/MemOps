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
    /// <summary>
    /// Safe handle used to PInvoke Read/WriteProcessMemory
    /// </summary>
    public SafeHandle Handle { get; init; }
    /// <summary>
    /// Access rights used to read/write
    /// </summary>
    public ProcessAccessRights AccessRights { get; set; }

    /// <summary>
    /// Initializes all necessary members to use PInvoke calls for reading/writing
    /// </summary>
    /// <param name="ptr">Pointer to object(s)</param>
    /// <param name="length">Length of how many T objects exist at the address</param>
    /// <param name="hasOwnership">Should memory be released when this instance is disposed?</param>
    /// <param name="handle">Handle to process</param>
    /// <param name="accessRights">Access rights needed</param>
    public PInvokeMemoryAddress(IntPtr ptr, int length, bool hasOwnership, SafeHandle handle, ProcessAccessRights accessRights) : base(ptr, length, hasOwnership)
    {
        Handle = handle;
        AccessRights = accessRights;
    }

    /// <summary>
    /// Reads 1 T at the address
    /// </summary>
    /// <param name="v">The ref to read to</param>
    public unsafe void Read(out T v)
    {
        MemoryOps.Read(Handle, Pointer.ToPointer(), out v);
    }

    /// <summary>
    /// Reads an amount of T depending on the length of the span
    /// </summary>
    /// <param name="span">Buffer to store read values</param>
    public unsafe void ReadMultiple(Span<T> span)
    {
        MemoryOps.ReadMultiple(Handle, Pointer.ToPointer(), span);
    }

    // TODO: Make in instead of ref
    /// <summary>
    /// Writes 1 T to the address
    /// </summary>
    /// <param name="v">T object to write</param>
    public unsafe void Write(ref T v)
    {
        MemoryOps.Write(Handle, Pointer.ToPointer(), ref v);
    }
    
    /// <summary>
    /// Writes an amount of T depending on the length of the span
    /// </summary>
    /// <param name="span">Source buffer</param>
    public unsafe void WriteMultiple(ReadOnlySpan<T> span)
    {
        MemoryOps.WriteMultiple(Handle, Pointer.ToPointer(), span);
    }
}