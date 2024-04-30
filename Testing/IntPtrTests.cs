using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MemOps.Addresses;
using MemOps.Extensions;

namespace Testing;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class IntPtrTests
{
    // USE WITH FIXED OFFSETS SPAN ONLY
    private unsafe MemoryAddress<nint> GeneratePointerChain(nint finalAddress, Span<nint> offsets)
    {
        var p0AddressesChain = Marshal.AllocHGlobal(sizeof(nint) * offsets.Length);
        var addressChain = new MemoryAddress<nint>(p0AddressesChain, offsets.Length, true);
        
        addressChain.GetSpan().Fill(p0AddressesChain);
        addressChain.GetSpan()[^1] = finalAddress;

        for (var i = 0; i < offsets.Length - 1; i++)
            offsets[i] = sizeof(nint) * (i + 1);
        offsets[^1] = 0;
        
        return addressChain;
    }
    
    [Test]
    public unsafe void TestChain()
    {
        var offsets = new nint[150_000_038];
        Console.WriteLine($"Memory used: {GC.GetTotalMemory(true) / 1024 / 1024} MB");
        var finalAddress = Marshal.AllocHGlobal(sizeof(ulong));
        *(ulong*)finalAddress = ulong.MinValue;
        var addressChain = GeneratePointerChain(finalAddress, offsets);
        
        var chainedAddress = addressChain.Pointer.Chain(offsets);
        
        Assert.Multiple(() =>
        {
            Assert.That(chainedAddress, Is.EqualTo(finalAddress));
            Assert.That(*(ulong*)chainedAddress, Is.EqualTo(ulong.MinValue));
        });
    }
}