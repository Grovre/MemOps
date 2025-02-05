using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using MemOps.Addresses;
using MemOps.Extensions;
using MemOps.Ops;

namespace Testing;

[TestFixture]
public unsafe class MemoryAddressTests
{
    const int Length = 0x40;

    private static unsafe void PerformReadWriteTest<T>(int length, Random rand)
        where T : unmanaged
    {
        using var hProc = ProcessOps.OpenProcessSafeHandle(
            Process.GetCurrentProcess(),
            MemOps.Enums.ProcessAccessRights.ProcessAllAccess);

        Span<T> src = stackalloc T[length];
        Span<T> dst = stackalloc T[length];

        fixed (T* pSrc = src)
        {
            dst.Clear();
            rand.NextBytes(MemoryMarshal.AsBytes(src));

            var srcMemAddr = new MemoryAddress<T>(new nint(pSrc), length);
            srcMemAddr.Read(dst);
            Assert.That(dst.SequenceEqual(src));

            dst.Clear();
            rand.NextBytes(MemoryMarshal.AsBytes(src));

            srcMemAddr = new PInvokeMemoryAddress<T>(hProc, new nint(pSrc), length);
            srcMemAddr.Read(dst);
            Assert.That(dst.SequenceEqual(src));
        }

        fixed (T* pDst = dst)
        {
            dst.Clear();
            rand.NextBytes(MemoryMarshal.AsBytes(src));

            var dstMemAddr = new MemoryAddress<T>(new nint(pDst), length);
            dstMemAddr.Write(src);
            Assert.That(dst.SequenceEqual(src));

            dst.Clear();
            rand.NextBytes(MemoryMarshal.AsBytes(src));

            dstMemAddr = new PInvokeMemoryAddress<T>(hProc, new nint(pDst), length);
            dstMemAddr.Write(src);
            Assert.That(dst.SequenceEqual(src));
        }
    }

    [Test]
    public unsafe void BasicReadWriteTest()
    {
        PerformReadWriteTest<byte>(Length, Random.Shared);
        PerformReadWriteTest<sbyte>(Length, Random.Shared);
        PerformReadWriteTest<short>(Length, Random.Shared);
        PerformReadWriteTest<ushort>(Length, Random.Shared);
        PerformReadWriteTest<int>(Length, Random.Shared);
        PerformReadWriteTest<uint>(Length, Random.Shared);
        PerformReadWriteTest<long>(Length, Random.Shared);
        PerformReadWriteTest<ulong>(Length, Random.Shared);
        PerformReadWriteTest<Int128>(Length, Random.Shared);
        PerformReadWriteTest<UInt128>(Length, Random.Shared);
        PerformReadWriteTest<Half>(Length, Random.Shared);
        PerformReadWriteTest<float>(Length, Random.Shared);
        PerformReadWriteTest<double>(Length, Random.Shared);
        PerformReadWriteTest<decimal>(Length, Random.Shared);
        PerformReadWriteTest<Vector2>(Length, Random.Shared);
        PerformReadWriteTest<Vector3>(Length, Random.Shared);
        PerformReadWriteTest<Vector4>(Length, Random.Shared);
        PerformReadWriteTest<Matrix4x4>(Length, Random.Shared);
        PerformReadWriteTest<Quaternion>(Length, Random.Shared);
    }
}