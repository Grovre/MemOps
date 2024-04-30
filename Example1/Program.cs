using System.Runtime.InteropServices;
using MemOps.Extensions;

const int size = 1_000;
var longs = Marshal
    .AllocHGlobal(sizeof(long) * size)
    .ToMemoryAddress<long>(size);
GC.AddMemoryPressure(longs.GetSpan<byte>().Length);

for (var i = 0; i < longs.Length; i++)
{
    longs.GetSpan()[i] = i;
}

for (var i = 0; i < longs.Length; i++)
{
    Console.WriteLine(longs.GetSpan()[i]);
}