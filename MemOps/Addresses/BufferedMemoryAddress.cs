﻿using System.Runtime.InteropServices;
using MemOps.Ops;

namespace MemOps.Addresses;

/// <summary>
///     A class that contains an internal buffer for reading and writing to an address through a handle.
///     Can be used in a safe context for unsafe ops and greatly abstracts the external Win32 calls made.
/// </summary>
/// <typeparam name="T">Non-null type used for the buffer</typeparam>
[Obsolete("Use MemoryAddress type", false)]
public sealed unsafe class BufferedMemoryAddress<T> : ICloneable
    where T : unmanaged
{
    private readonly void* _address;
    private readonly SafeHandle _handle;
    private T _buf; // Required for ref MemoryOp arguments, ignore SonarLint

    /// <summary>
    ///     Creates a BufferedMemoryAddress for usage
    /// </summary>
    /// <param name="handle">Handle with the required access rights</param>
    /// <param name="address">Address to read/write</param>
    /// <param name="printOnReadOrWrite">Print on a read/write</param>
    public BufferedMemoryAddress(SafeHandle handle, nint address, bool printOnReadOrWrite)
    {
        handle.AssertHandleIsValidDebug();
        _handle = handle;
        _address = address.ToPointer();
        PrintOnReadOrWrite = printOnReadOrWrite;
    }

    /// <summary>
    ///     If true, reading or writing to the pointer will print a message to the console
    /// </summary>
    public bool PrintOnReadOrWrite { get; set; }

    /// <summary>
    ///     The address as a nint
    /// </summary>
    public nint Address => (nint)_address;

    /// <summary>
    ///     A buffer used for the memory operations.
    ///     Read ops write to this, and write ops read from this.
    /// </summary>
    public T Buffer // Required for ref MemoryOp arguments, ignore SonarLint
    {
        get => _buf;
        set => _buf = value;
    }

    /// <summary>
    ///     Clones everything from this object into a new one, including the buffer.
    ///     The same handle will be used.
    /// </summary>
    /// <returns>Copied BufferedMemoryAddress using T</returns>
    public object Clone()
    {
        var memAddr = new BufferedMemoryAddress<T>(_handle, Address, PrintOnReadOrWrite)
        {
            Buffer = _buf
        };
        
        return memAddr;
    }

    /// <summary>
    ///     Reads the address. Results are stored in the buffer.
    /// </summary>
    public void Read()
    {
        MemoryOps.Read(_handle, _address, out _buf, PrintOnReadOrWrite);
    }

    /// <summary>
    ///     Reads the address. Results bypass this object's buffer and
    ///     are stored in the referenced buffer. This can result in
    ///     performance benefits by avoiding writing to the heap.
    /// </summary>
    /// <param name="buffer">The buffer to write to</param>
    public void Read(out T buffer)
    {
        MemoryOps.Read(_handle, _address, out buffer, PrintOnReadOrWrite);
    }

    /// <summary>
    ///     Reads the ith element into the ith index of the buffer span.
    ///     The structs must be contiguous. This object's buffer is
    ///     bypassed and values are written straight into the span.
    /// </summary>
    /// <param name="bufferSpan">Buffer to read to</param>
    public void ReadMultiple(Span<T> bufferSpan)
    {
        MemoryOps.ReadMultiple(_handle, _address, bufferSpan, PrintOnReadOrWrite);
    }

    /// <summary>
    ///     Continuously reads the address in a cycle. Results are stored in the buffer.
    ///     Supports locking buffer I/O using a ReaderWriterLockSlim and finishing
    ///     with a cancellation token, but these are optional.
    ///     If there is no lock, it will write straight to the buffer.
    ///     If there is no cancel token, the thread will never exit the method
    /// </summary>
    /// <param name="delayBetweenReads">The time a thread sleeps between reads</param>
    /// <param name="rwLock">Used for locking the buffer I/O</param>
    /// <param name="cancelToken">Exits the thread when flagged</param>
    public void StartReadCycle(TimeSpan delayBetweenReads, ReaderWriterLockSlim? rwLock, CancellationToken? cancelToken)
    {
        MemoryOps.ReadCycle(_handle, _address, ref _buf, delayBetweenReads, rwLock, cancelToken, PrintOnReadOrWrite);
    }

    /// <summary>
    ///     Continuously reads the address in a cycle. Results bypass this object's buffer
    ///     and go to the reference buffer given instead. Supports locking buffer I/O
    ///     using a ReaderWriterLockSlim and finishing with a cancellation token,
    ///     but these are optional.
    ///     If there is no lock, it will write straight to the buffer.
    ///     If there is no cancel token, the thread will never exit the method
    /// </summary>
    /// <param name="buffer">The buffer reads are copied to</param>
    /// <param name="delayBetweenReads">The time a thread sleeps between reads</param>
    /// <param name="rwLock">Used for locking the buffer I/O</param>
    /// <param name="cancelToken">Exits the thread when flagged</param>
    public void StartReadCycle(ref T buffer, TimeSpan delayBetweenReads, ReaderWriterLockSlim? rwLock,
        CancellationToken? cancelToken)
    {
        MemoryOps.ReadCycle(_handle, _address, ref buffer, delayBetweenReads, rwLock, cancelToken, PrintOnReadOrWrite);
    }

    /// <summary>
    ///     Writes to the address. The buffer is copied to the address
    /// </summary>
    public void Write()
    {
        MemoryOps.Write(_handle, _address, ref _buf, PrintOnReadOrWrite);
    }

    /// <summary>
    ///     Writes to the address. Reads bypass this object's buffer and
    ///     instead go to the referenced buffer. This can result in
    ///     performance benefits by avoiding reading from the heap.
    /// </summary>
    /// <param name="buffer">The buffer to read from</param>
    public void Write(ref T buffer)
    {
        MemoryOps.Write(_handle, _address, ref buffer, PrintOnReadOrWrite);
    }

    /// <summary>
    ///     Provides a shorthand for reading, doing something, then writing a value
    ///     by reading a value, invoking the mutator function on it, and writing
    ///     what was returned by the mutator.
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

    /// <summary>
    ///     Creates a new BufferedMemoryAddress with the given address
    ///     interpreted as the given type
    /// </summary>
    /// <param name="newAddress">Address to replace with</param>
    /// <typeparam name="TNew">New type to interpret memory as</typeparam>
    /// <returns>Reinterpreted BufferedMemoryAddress at address</returns>
    public BufferedMemoryAddress<TNew> WithAddress<TNew>(nint newAddress)
        where TNew : unmanaged
    {
        return new BufferedMemoryAddress<TNew>(_handle, newAddress, PrintOnReadOrWrite);
    }

    /// <summary>
    ///     Follows offsets by doing some offset-pointer traversal.
    ///     This adds the ith offset to the current pointer, dereferences and repeats.
    ///     At the last offset, it only adds.
    ///     If there is only one offset provided for the params, it will only add
    ///     the offset to the address.
    ///     Works exactly like how Cheat Engine follows pointers and offsets them.
    /// </summary>
    /// <param name="offsets">The offsets to follow</param>
    /// <returns>The final address of the followed offsets</returns>
    public BufferedMemoryAddress<T> FollowOffsets(params nint[] offsets)
    {
        var addr = MemoryOps.FollowOffsets(_handle, Address, offsets);
        return new BufferedMemoryAddress<T>(_handle, addr, PrintOnReadOrWrite)
        {
            _buf = _buf
        };
    }

    /// <summary>
    ///     Follows offsets by doing some offset-pointer traversal.
    ///     This adds the ith offset to the current pointer, dereferences and repeats.
    ///     At the last offset, it only adds. This method will also read into the buffer
    ///     at the end.
    ///     If there is only one offset provided for the params, it will only add
    ///     the offset to the address.
    ///     Works exactly like how Cheat Engine follows pointers and offsets them.
    /// </summary>
    /// <param name="offsets">The offsets to follow</param>
    /// <returns>The final address of the followed offsets with a filled buffer</returns>
    public BufferedMemoryAddress<T> FollowOffsetsAndRead(params nint[] offsets)
    {
        var addr = FollowOffsets(offsets);
        addr.Read();
        return addr;
    }

    /// <summary>
    ///     Reinterprets the type at the memory address in the BufferedMemoryAddress.
    ///     This copies the BufferedMemoryAddress without the buffer for safety. In
    ///     other words, the buffer will be empty.
    /// </summary>
    /// <typeparam name="TNew">The new type to reinterpret to</typeparam>
    /// <returns>Reinterpreted memory address</returns>
    public BufferedMemoryAddress<TNew> WithType<TNew>()
        where TNew : unmanaged
    {
        return new BufferedMemoryAddress<TNew>(_handle, Address, PrintOnReadOrWrite);
    }

    /// <summary>
    ///     The builder class for BufferedMemoryAddress. This class can provides
    ///     methods to shorten code and boilerplate by letting users avoid
    ///     repeating entire constructor parameters for BufferedMemoryAddress.
    /// </summary>
    public class Builder : ICloneable
    {
        /// <summary>
        ///     Creates the builder. A handle is required because
        ///     all memory operations require a handle
        /// </summary>
        /// <param name="handle">Valid handle used for memory I/O</param>
        public Builder(SafeHandle handle)
        {
            Handle = handle;
        }

        public nint Address { get; set; }
        public bool PrintOnReadOrWrite { get; set; }
        public T Buffer { get; set; }
        public SafeHandle Handle { get; set; }

        public object Clone()
        {
            return new Builder(Handle)
            {
                Address = Address,
                PrintOnReadOrWrite = PrintOnReadOrWrite,
                Buffer = Buffer
            };
        }

        /// <summary>
        ///     Creates a BufferedMemoryAddress for use by the building user
        /// </summary>
        /// <returns>A BufferedMemoryAddress using the builder's provided fields</returns>
        public BufferedMemoryAddress<T> Build()
        {
            return new BufferedMemoryAddress<T>(Handle, Address, PrintOnReadOrWrite)
            {
                _buf = Buffer
            };
        }

        /// <inheritdoc cref="Clone" />
        public Builder CloneT()
        {
            return new Builder(Handle)
            {
                Address = Address,
                PrintOnReadOrWrite = PrintOnReadOrWrite,
                Buffer = Buffer
            };
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
    }
}