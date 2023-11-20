using System.Runtime.InteropServices;
using MemOps.Enums;
using MemOps.Ops;

namespace MemOps.Addresses;

public class PInvokeMemoryAddress<T>
    where T : unmanaged
{
    public SafeHandle Handle { get; init; }
    public ProcessAccessRights AccessRights { get; set; }
    public MemoryAddress<T> MemoryAddress { get; set; }

    public PInvokeMemoryAddress(MemoryAddress<T> memoryAddress, SafeHandle handle, ProcessAccessRights accessRights)
    {
        Handle = handle;
        AccessRights = accessRights;
        MemoryAddress = memoryAddress;
    }

    public unsafe void Read(out T v)
    {
        MemoryOps.Read(Handle, MemoryAddress.Pointer.ToPointer(), out v);
    }

    public unsafe void ReadMultiple(Span<T> span)
    {
        MemoryOps.ReadMultiple(Handle, MemoryAddress.Pointer.ToPointer(), span);
    }

    public unsafe void Write(ref T v)
    {
        MemoryOps.Write(Handle, MemoryAddress.Pointer.ToPointer(), ref v);
    }
}