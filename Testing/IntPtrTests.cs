using System.Runtime.InteropServices;
using MemOps.Addresses;
using MemOps.Extensions;

namespace Testing;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class IntPtrTests
{
    private unsafe MemoryAddress<nint> GeneratePointerChain(nint finalAddress, Span<nint> offsets)
    {
        var p0AddressesChain = new nint(NativeMemory.Alloc((nuint)(sizeof(nint) * offsets.Length)));
        var addressChain = new MemoryAddress<nint>(p0AddressesChain, offsets.Length);
        
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
        var finalAddress = new nint(NativeMemory.Alloc(sizeof(decimal)));
        var addressChain = GeneratePointerChain(finalAddress, offsets);
        Console.WriteLine($"Memory used: {GC.GetTotalMemory(true) / 1024 / 1024} MB");
        
        const decimal expectedValue = 0.987654321m;
        *(decimal*)finalAddress = expectedValue;
        var chainedAddress = addressChain.Pointer.Chain(offsets);
        
        Assert.Multiple(() =>
        {
            Assert.That(chainedAddress, Is.EqualTo(finalAddress));
            Assert.That(*(decimal*)chainedAddress, Is.EqualTo(expectedValue));
        });
        
        NativeMemory.Free(finalAddress.ToPointer());
        NativeMemory.Free(addressChain.Pointer.ToPointer());
    }
}