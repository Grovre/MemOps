using System.Diagnostics;
using System.Runtime.InteropServices;
using MemOps.Addresses;
using MemOps.Extensions;

namespace Testing;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class PointerChainTests
{
    private readonly ThreadLocal<nint[]> _offsets = new(() => new nint[1024 * 1024 * 10]);
    
    private static unsafe MemoryAddress<nint> GeneratePointerChain(nint finalAddress, Span<nint> offsets)
    {
        var p0AddressesChain = new nint(NativeMemory.Alloc((nuint)(sizeof(nint) * offsets.Length)));
        var addressChain = new MemoryAddress<nint>(p0AddressesChain, offsets.Length);
        
        addressChain.AsSpan().Fill(p0AddressesChain);
        addressChain.AsSpan()[^1] = finalAddress;

        for (var i = 0; i < offsets.Length - 1; i++)
            offsets[i] = sizeof(nint) * (i + 1);
        offsets[^1] = 0;
        
        return addressChain;
    }
    
    private unsafe (MemoryAddress<IntPtr> PointerChainP0, IntPtr[] Offsets, IntPtr FinalAddress) PrepareTest<T>(in T expectedValue) where T : unmanaged
    {
        var finalAddress = new nint(NativeMemory.Alloc((nuint)sizeof(T)));
        *(T*)finalAddress = expectedValue;
        var ptrChain = GeneratePointerChain(finalAddress, _offsets.Value!);
        Console.WriteLine($"Memory used: {GC.GetTotalMemory(true) / 1024 / 1024:N2} MB");
        
        return (ptrChain, _offsets.Value!, finalAddress);
    }
    
    [Test]
    public unsafe void TestChain()
    {
        const decimal expectedValue = 0.987654321m;
        var testPrep = PrepareTest(expectedValue);
        var chainedAddress = testPrep.PointerChainP0.Pointer.Chain(testPrep.Offsets);
        
        Assert.Multiple(() =>
        {
            Assert.That(chainedAddress, Is.EqualTo(testPrep.FinalAddress));
            Assert.That(*(decimal*)testPrep.FinalAddress, Is.EqualTo(expectedValue)); // Just in case... Will never fail though
            Assert.That(*(decimal*)chainedAddress, Is.EqualTo(*(decimal*)testPrep.FinalAddress));
        });
        
        NativeMemory.Free(testPrep.PointerChainP0.Pointer.ToPointer());
        NativeMemory.Free(testPrep.FinalAddress.ToPointer());
    }

    [Test]
    public unsafe void TestPInvokeChain()
    {
        const decimal expectedValue = 0.123456789m;
        var testPrep = PrepareTest(expectedValue);
        var hProc = Process.GetCurrentProcess().SafeHandle;
        var chainedAddress = testPrep.PointerChainP0.Pointer.PInvokeChain(hProc, _offsets.Value!);
        
        Assert.Multiple(() =>
        {
            Assert.That(chainedAddress, Is.EqualTo(testPrep.FinalAddress));
            Assert.That(*(decimal*)testPrep.FinalAddress, Is.EqualTo(expectedValue));
            Assert.That(*(decimal*)chainedAddress, Is.EqualTo(*(decimal*)testPrep.FinalAddress));
        });
        
        NativeMemory.Free(testPrep.PointerChainP0.Pointer.ToPointer());
        NativeMemory.Free(testPrep.FinalAddress.ToPointer());
    }
}