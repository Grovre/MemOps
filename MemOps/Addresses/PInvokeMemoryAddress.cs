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

    public unsafe bool TryRead(out T v)
    {
        MemoryOps.Read(Handle, MemoryAddress.Pointer.ToPointer(), out v);
        return true;
    }

    public unsafe bool TryReadMultiple(Span<T> span)
    {
        MemoryOps.ReadMultiple(Handle, MemoryAddress.Pointer.ToPointer(), span);
        return true;
    }

    public unsafe bool TryWrite(ref T v)
    {
        MemoryOps.Write(Handle, MemoryAddress.Pointer.ToPointer(), ref v);
        return true;
    }
}