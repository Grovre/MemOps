using System.Runtime.InteropServices;
using MemOps.Ops;

namespace MemOps.DataStructures;

public unsafe class BufferedMemoryAddress<T> : ICloneable
    where T : struct
{
    private readonly SafeHandle _handle;
    public bool PrintOnReadOrWrite { get; set; }
    private readonly void* _address;
    private T _buf;

    public nint Address => (nint)_address;

    public T Buffer
    {
        get => _buf;
        set => _buf = value;
    }

    public BufferedMemoryAddress(SafeHandle handle, nint address, bool printOnReadOrWrite)
    {
        handle.IsMemoryValid();
        _handle = handle;
        _address = address.ToPointer();
        PrintOnReadOrWrite = printOnReadOrWrite;
    }

    public void Read()
    {
        MemoryOps.Read(_handle, _address, out _buf, PrintOnReadOrWrite);
    }

    public void StartReadCycle(TimeSpan delayBetweenReads, ReaderWriterLockSlim? rwLock, CancellationToken? cancelToken)
    {
        MemoryOps.ReadCycle(_handle, _address, ref _buf, delayBetweenReads, rwLock, cancelToken, PrintOnReadOrWrite);
    }

    public void Write()
    {
        MemoryOps.Write(_handle, _address, ref _buf, PrintOnReadOrWrite);
    }

    public object Clone()
    {
        var memAddr = new BufferedMemoryAddress<T>(_handle, Address, PrintOnReadOrWrite);
        memAddr.Buffer = _buf;
        return memAddr;
    }
}