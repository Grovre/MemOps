using System.Runtime.InteropServices;
using MemOps.Addresses;
using MemOps.Extensions;

namespace Testing;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class Tests
{
    private const int Size = 1_000_000;

    private ThreadLocal<MemoryAddress<byte>> UnmanagedMemory = new (() =>
    {
        var memAddr = Marshal.AllocHGlobal(Size)
            .ToMemoryAddress<byte>(Size, true);

        var rand = new Random();
        rand.NextBytes(memAddr);
        return memAddr;
    });

    [Test]
    public void ReadingWritingBytes()
    {
        const byte fillByte = 2;
        var span = UnmanagedMemory.Value!.GetSpan();
        span.Fill(fillByte);
        foreach (var b in span)
            Assert.That(b, Is.EqualTo(fillByte));
    }

    [Test]
    public void ReadingWritingInts()
    {
        const int fillInt = 83_758_973;
        var span = UnmanagedMemory.Value!.GetSpan<int>();
        span.Fill(fillInt);
        foreach (var i in span)
            Assert.That(i, Is.EqualTo(fillInt));
    }

    [Test]
    public void ReadingWritingLongs()
    {
        const long fillLong = 990_999_999_998_765;
        var span = UnmanagedMemory.Value!.GetSpan<long>();
        span.Fill(fillLong);
        foreach (var l in span)
            Assert.That(l, Is.EqualTo(fillLong));
    }
    
    [Test]
    public void ReadingWritingDoubles()
    {
        const double fillDouble = double.E * double.Pi;
        var span = UnmanagedMemory.Value!.GetSpan<double>();
        span.Fill(fillDouble);
        foreach (var d in span)
            Assert.That(d, Is.EqualTo(fillDouble));
    }

    [Test]
    public void ReadingWritingDecimals()
    {
        const decimal fillDecimal = (decimal)(double.E * double.Pi);
        var span = UnmanagedMemory.Value!.GetSpan<decimal>();
        span.Fill(fillDecimal);
        foreach (var d in span)
            Assert.That(d, Is.EqualTo(fillDecimal));
    }
}