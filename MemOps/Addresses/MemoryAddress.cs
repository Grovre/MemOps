using System.Buffers;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace MemOps.Addresses;

public unsafe class MemoryAddress<T> : MemoryManager<T>
    where T : unmanaged
{
    public readonly IntPtr Pointer;
    public readonly int Length;
    public readonly bool HasOwnership;

    public MemoryAddress(IntPtr pointer, int length, bool hasOwnershipOfMemory)
    {
        Pointer = pointer;
        Length = length;
        HasOwnership = hasOwnershipOfMemory;
    }

    public override Span<T> GetSpan()
    {
        return new Span<T>(Pointer.ToPointer(), Length);
    }
    
    protected override void Dispose(bool disposing)
    {
        if (HasOwnership)
        {
            Marshal.FreeHGlobal(Pointer);
        }
    }

    public override MemoryHandle Pin(int elementIndex = 0)
    {
        
        throw new NotImplementedException();
    }

    public override void Unpin()
    {
        throw new NotImplementedException();
    }
}