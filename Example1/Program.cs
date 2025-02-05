using System.Runtime.InteropServices;
using MemOps.Extensions;

unsafe
{
    const int size = 1_000;
    var pLongs = NativeMemory.Alloc(sizeof(long) * size);
    var longs = new nint(pLongs).ToMemoryAddress<long>(size);

    for (var i = 0; i < longs.Length; i++)
    {
        longs.AsSpan()[i] = i;
    }

    for (var i = 0; i < longs.Length; i++)
    {
        Console.WriteLine(longs.AsSpan()[i]);
    }

    NativeMemory.Free(pLongs);
}