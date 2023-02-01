// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using MemOps;

namespace Console;

public static unsafe class Program
{
    public static void Main()
    {
        var handle = Process.GetCurrentProcess().SafeHandle;
        var n = 24;
        MemoryOps.Read(handle, &n, out int read); // Reading from n to read
        System.Console.WriteLine(read);
        n = int.MaxValue;
        MemoryOps.Write(handle, &read, ref n); // Writing from n to read
        System.Console.WriteLine(read);
    }
}