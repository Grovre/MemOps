using System.Runtime.InteropServices;
using MemOps.Ops;

namespace MemOps.DataStructures;

public unsafe struct MemoryAddress<T>
    where T : struct
{
    private readonly SafeHandle _handle;
    public void* Address { get; }
    public bool PrintOnReadOrWrite { get; set; }
    private T _buf;

    public T Buffer
    {
        get => _buf;
        set => _buf = value;
    }

    public MemoryAddress(SafeHandle handle, nint address, bool printOnReadOrWrite)
    {
        handle.IsMemoryValid();
        _handle = handle;
        Address = address.ToPointer();
        PrintOnReadOrWrite = printOnReadOrWrite;
    }

    public void Read()
    {
        MemoryOps.Read(_handle, Address, out _buf, PrintOnReadOrWrite);
    }

    public void StartReadCycle(TimeSpan delayBetweenReads, ReaderWriterLockSlim? rwLock, CancellationToken? cancelToken)
    {
        MemoryOps.ReadCycle(_handle, Address, ref _buf, delayBetweenReads, rwLock, cancelToken, PrintOnReadOrWrite);
    }

    public void Write()
    {
        MemoryOps.Write(_handle, Address, ref _buf, PrintOnReadOrWrite);
    }

    public MemoryAddress<T> Copy()
    {
        return new MemoryAddress<T>(_handle, (nint)Address, PrintOnReadOrWrite);
    }
}