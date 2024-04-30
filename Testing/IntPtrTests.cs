﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MemOps.Addresses;
using MemOps.Extensions;

namespace Testing;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class IntPtrTests
{
    private ThreadLocal<nint[]> _offsets = new(() => new nint[1024 * 1024 * 10]);
    
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
        var finalAddress = Marshal.AllocHGlobal(sizeof(decimal));
        var addressChain = GeneratePointerChain(finalAddress, _offsets.Value!);
        Console.WriteLine($"Memory used: {GC.GetTotalMemory(true) / 1024 / 1024} MB");
        
        const decimal expectedValue = 0.987654321m;
        *(decimal*)finalAddress = expectedValue;
        var chainedAddress = addressChain.Pointer.Chain(_offsets.Value!);
        
        Assert.Multiple(() =>
        {
            Assert.That(chainedAddress, Is.EqualTo(finalAddress));
            Assert.That(*(decimal*)chainedAddress, Is.EqualTo(expectedValue));
        });
    }
}