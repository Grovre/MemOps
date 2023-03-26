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
    public T Buffer // Required for ref MemoryOp arguments, ignore SonarLint
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
    /// Reads the address. Results bypass this object's buffer and
    /// are stored in the referenced buffer. This can result in
    /// performance benefits by avoiding writing to the heap.
    /// </summary>
    /// <param name="buffer">The buffer to write to</param>
    public void Read(out T buffer)
    {
        MemoryOps.Read(_handle, _address, out buffer, PrintOnReadOrWrite);
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
    /// Continuously reads the address in a cycle. Results bypass this object's buffer
    /// and go to the reference buffer given instead. Supports locking buffer I/O
    /// using a ReaderWriterLockSlim and finishing with a cancellation token,
    /// but these are optional.
    ///
    /// If there is no lock, it will write straight to the buffer.
    /// If there is no cancel token, the thread will never exit the method
    /// </summary>
    /// <param name="buffer">The buffer reads are copied to</param>
    /// <param name="delayBetweenReads">The time a thread sleeps between reads</param>
    /// <param name="rwLock">Used for locking the buffer I/O</param>
    /// <param name="cancelToken">Exits the thread when flagged</param>
    public void StartReadCycle(ref T buffer, TimeSpan delayBetweenReads, ReaderWriterLockSlim? rwLock, CancellationToken? cancelToken)
    {
        MemoryOps.ReadCycle(_handle, _address, ref buffer, delayBetweenReads, rwLock, cancelToken, PrintOnReadOrWrite);
    }

    /// <summary>
    /// Writes to the address. The buffer is copied to the address
    /// </summary>
    public void Write()
    {
        MemoryOps.Write(_handle, _address, ref _buf, PrintOnReadOrWrite);
    }
    
    /// <summary>
    /// Writes to the address. Reads bypass this object's buffer and
    /// instead go to the referenced buffer. This can result in
    /// performance benefits by avoiding reading from the heap.
    /// </summary>
    /// <param name="buffer">The buffer to read from</param>
    public void Write(ref T buffer)
    {
        MemoryOps.Write(_handle, _address, ref buffer, PrintOnReadOrWrite);
    }

    /// <summary>
    /// Provides a shorthand for reading, doing something, then writing a value
    /// by reading a value, invoking the mutator function on it, and writing
    /// what was returned by the mutator.
    /// </summary>
    /// <param name="mutator">The function to mutate the value with</param>
    /// <param name="mutateOnBuffer">Whether or not to read to the buffer. False is recommended</param>
    public void MutateValue(Func<T, T> mutator, bool mutateOnBuffer = false)
    {
        T quickBuffer = default;
        ref var v = ref quickBuffer;
        if (mutateOnBuffer)
            v = ref _buf;
        
        mutator(v);
        Write(ref v);
    }

    public BufferedMemoryAddress<TNew> WithAddress<TNew>(nint newAddress)
        where TNew : struct
        => new BufferedMemoryAddress<TNew>(_handle, newAddress, PrintOnReadOrWrite);

    public BufferedMemoryAddress<T> FollowOffsets(params nint[] offsets)
    {
        var addr = MemoryOps.FollowOffsets(_handle, Address, offsets);
        return new BufferedMemoryAddress<T>(_handle, addr, PrintOnReadOrWrite);
    }

    public BufferedMemoryAddress<T> FollowOffsetsAndRead(params nint[] offsets)
    {
        var addr = FollowOffsets(offsets);
        addr.Read();
        return addr;
    }

    public BufferedMemoryAddress<TNew> WithType<TNew>()
        where TNew : struct
        => new BufferedMemoryAddress<TNew>(_handle, Address, PrintOnReadOrWrite);

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
    
    /// <summary>
    /// The builder class for BufferedMemoryAddress. This class can provides
    /// methods to shorten code and boilerplate by letting users avoid
    /// repeating entire constructor parameters for BufferedMemoryAddress.
    /// </summary>
    public class Builder : ICloneable
    {
        public nint Address { get; set; }
        public bool PrintOnReadOrWrite { get; set; }
        public T Buffer { get; set; }
        public SafeHandle Handle { get; set; }

        /// <summary>
        /// Creates the builder. A handle is required because
        /// all memory operations require a handle
        /// </summary>
        /// <param name="handle">Valid handle used for memory I/O</param>
        public Builder(SafeHandle handle)
        {
            Handle = handle;
        }

        #region Setters

        public Builder SetAddress(nint address)
        {
            Address = address;
            return this;
        }

        public Builder SetPrintOnReadOrWrite(bool printOnReadOrWrite)
        {
            PrintOnReadOrWrite = printOnReadOrWrite;
            return this;
        }

        public Builder SetBuffer(T buffer)
        {
            Buffer = buffer;
            return this;
        }

        public Builder SetHandle(SafeHandle handle)
        {
            Handle = handle;
            return this;
        }
        
        #endregion

        /// <summary>
        /// Creates a BufferedMemoryAddress for use by the building user
        /// </summary>
        /// <returns>A BufferedMemoryAddress using the builder's provided fields</returns>
        public BufferedMemoryAddress<T> Build()
        {
            return new BufferedMemoryAddress<T>(Handle, Address, PrintOnReadOrWrite)
            {
                _buf = Buffer
            };
        }

        /// <inheritdoc cref="Clone"/>
        public Builder CloneT()
            => new Builder(Handle)
            {
                Address = this.Address,
                PrintOnReadOrWrite = this.PrintOnReadOrWrite,
                Buffer = this.Buffer
            };

        public object Clone()
            => new Builder(Handle)
            {
                Address = this.Address,
                PrintOnReadOrWrite = this.PrintOnReadOrWrite,
                Buffer = this.Buffer
            };
    }
}