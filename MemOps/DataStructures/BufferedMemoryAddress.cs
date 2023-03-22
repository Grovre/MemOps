using System.Runtime.InteropServices;
using MemOps.Ops;

namespace MemOps.DataStructures;

/// <summary>
/// A class that contains an internal buffer for reading and writing to an address through a handle.
/// Can be used in a safe context for unsafe ops and greatly abstracts the external Win32 calls made.
/// </summary>
/// <typeparam name="T">Non-null type used for the buffer</typeparam>
public sealed unsafe class BufferedMemoryAddress<T> : ICloneable
    where T : struct
{
    private readonly SafeHandle _handle;
    /// <summary>
    /// If true, reading or writing to the pointer will print a message to the console
    /// </summary>
    public bool PrintOnReadOrWrite { get; set; }
    private readonly void* _address;
    private T _buf; // Required for ref MemoryOp arguments, ignore SonarLint

    /// <summary>
    /// The address as a nint
    /// </summary>
    public nint Address => (nint)_address;

    /// <summary>
    /// A buffer used for the memory operations.
    /// Read ops write to this, and write ops read from this.
    /// </summary>
    /// TODO: Add buffer bypass methods to avoid heap writes
    public T Buffer
    {
        get => _buf;
        set => _buf = value;
    }

    /// <summary>
    /// Creates a BufferedMemoryAddress for usage
    /// </summary>
    /// <param name="handle">Handle with the required access rights</param>
    /// <param name="address">Address to read/write</param>
    /// <param name="printOnReadOrWrite">Print on a read/write</param>
    public BufferedMemoryAddress(SafeHandle handle, nint address, bool printOnReadOrWrite)
    {
        handle.IsMemoryValid();
        _handle = handle;
        _address = address.ToPointer();
        PrintOnReadOrWrite = printOnReadOrWrite;
    }

    /// <summary>
    /// Reads the address. Results are stored in the buffer.
    /// </summary>
    public void Read()
    {
        MemoryOps.Read(_handle, _address, out _buf, PrintOnReadOrWrite);
    }

    /// <summary>
    /// Continuously reads the address in a cycle. Results are stored in the buffer.
    /// Supports locking buffer I/O using a ReaderWriterLockSlim and finishing
    /// with a cancellation token, but these are optional.
    ///
    /// If there is no lock, it will write straight to the buffer.
    /// If there is no cancel token, the thread will never exit the method
    /// </summary>
    /// <param name="delayBetweenReads">The time a thread sleeps between reads</param>
    /// <param name="rwLock">Used for locking the buffer I/O</param>
    /// <param name="cancelToken">Exits the thread when flagged</param>
    public void StartReadCycle(TimeSpan delayBetweenReads, ReaderWriterLockSlim? rwLock, CancellationToken? cancelToken)
    {
        MemoryOps.ReadCycle(_handle, _address, ref _buf, delayBetweenReads, rwLock, cancelToken, PrintOnReadOrWrite);
    }

    /// <summary>
    /// Writes to the address. The buffer is copied to the address
    /// </summary>
    public void Write()
    {
        MemoryOps.Write(_handle, _address, ref _buf, PrintOnReadOrWrite);
    }

    /// <summary>
    /// Clones everything from this object into a new one, including the buffer.
    /// The same handle will be used.
    /// </summary>
    /// <returns>Copied BufferedMemoryAddress using T</returns>
    public object Clone()
    {
        var memAddr = new BufferedMemoryAddress<T>(_handle, Address, PrintOnReadOrWrite);
        memAddr.Buffer = _buf;
        return memAddr;
    }
}