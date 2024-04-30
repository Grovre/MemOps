using System.Diagnostics;
using System.Runtime.InteropServices;
using MemOps.Addresses;
using MemOps.Enums;

namespace Testing;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public unsafe class PInvokeMemoryAddressTests
{
    public const int Size = 1_000_000;
    private readonly ThreadLocal<PInvokeMemoryAddress<byte>> UnmanagedMemory = new(() =>
    {
        var hproc = Process.GetCurrentProcess().SafeHandle;
        var memPtr = new nint(NativeMemory.Alloc(Size));
        var memAddr = new PInvokeMemoryAddress<byte>(
            memPtr, 
            Size, 
            hproc, 
            ProcessAccessRights.ProcessAllAccess);

        var rand = new Random();
        rand.NextBytes(memAddr);
        return memAddr;
    });
    
    private readonly ThreadLocal<byte[]> WriteSource = new(() =>
    {
        var bytes = new byte[Size];
        var rand = new Random();
        rand.NextBytes(bytes);
        return bytes;
    });
    
    private readonly ThreadLocal<byte[]> ReadDestination = new(() => new byte[Size]);
    
    [Test]
    public void ReadingWritingBytes()
    {
        const byte fillByte = 2;
        var src = WriteSource.Value!.AsSpan();
        src.Fill(fillByte);
        UnmanagedMemory.Value!.WriteMultiple(src);
        var dst = ReadDestination.Value!.AsSpan();
        UnmanagedMemory.Value!.ReadMultiple(dst);

        for (var i = 0; i < Size; i++)
        {
            Assert.That(src[i], Is.EqualTo(dst[i]));
        }
    }
    
    [Test]
    public void ReadingWritingInts()
    {
        const int fillInt = 83_758_973;
        var byteSrc = WriteSource.Value!.AsSpan();
        var src = MemoryMarshal.Cast<byte, int>(byteSrc);
        src.Fill(fillInt);
        UnmanagedMemory.Value!.WriteMultiple(byteSrc);
        var byteDst = ReadDestination.Value!.AsSpan();
        var dst = MemoryMarshal.Cast<byte, int>(byteDst);
        UnmanagedMemory.Value!.ReadMultiple(byteDst);

        for (var i = 0; i < src.Length; i++)
        {
            Assert.That(src[i], Is.EqualTo(dst[i]));
        }
    }
    
    [Test]
    public void ReadingWritingLongs()
    {
        const long fillLong = 990_999_999_998_765;
        var byteSrc = WriteSource.Value!.AsSpan();
        var src = MemoryMarshal.Cast<byte, long>(byteSrc);
        src.Fill(fillLong);
        UnmanagedMemory.Value!.WriteMultiple(byteSrc);
        var byteDst = ReadDestination.Value!.AsSpan();
        var dst = MemoryMarshal.Cast<byte, long>(byteDst);
        UnmanagedMemory.Value!.ReadMultiple(byteDst);

        for (var i = 0; i < src.Length; i++)
        {
            Assert.That(src[i], Is.EqualTo(dst[i]));
        }
    }
    
    [Test]
    public void ReadingWritingDoubles()
    {
        const double fillDouble = double.E * double.Pi;
        var byteSrc = WriteSource.Value!.AsSpan();
        var src = MemoryMarshal.Cast<byte, double>(byteSrc);
        src.Fill(fillDouble);
        UnmanagedMemory.Value!.WriteMultiple(byteSrc);
        var byteDst = ReadDestination.Value!.AsSpan();
        var dst = MemoryMarshal.Cast<byte, double>(byteDst);
        UnmanagedMemory.Value!.ReadMultiple(byteDst);

        for (var i = 0; i < src.Length; i++)
        {
            Assert.That(src[i], Is.EqualTo(dst[i]));
        }
    }
    
    [Test]
    public void ReadingWritingDecimals()
    {
        const decimal fillDecimal = (decimal)(double.E * double.Pi);
        var byteSrc = WriteSource.Value!.AsSpan();
        var src = MemoryMarshal.Cast<byte, decimal>(byteSrc);
        src.Fill(fillDecimal);
        UnmanagedMemory.Value!.WriteMultiple(byteSrc);
        var byteDst = ReadDestination.Value!.AsSpan();
        var dst = MemoryMarshal.Cast<byte, decimal>(byteDst);
        UnmanagedMemory.Value!.ReadMultiple(byteDst);

        for (var i = 0; i < src.Length; i++)
        {
            Assert.That(src[i], Is.EqualTo(dst[i]));
        }
    }
}