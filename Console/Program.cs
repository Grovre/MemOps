// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using MemOps.DataStructures;
using MemOps.Enums;
using MemOps.Extensions;
using MemOps.Ops;

namespace Console;

public static class Program
{
    public static void Main()
    {
        PrintProcessesAndNamesAlphabetical();
        var proc = ReadProcess();
        var handleRights = new[]
        {
            ProcessAccessRights.ProcessVmRead,
        }.CombineRights();
        var handle = proc.OpenProcessSafeHandle(handleRights);
        var address = ReadAddress();

        var memWorker = new BufferedMemoryAddress<int>(handle, address, false);
        var cancelToken = new CancellationTokenSource();
        var t = Task.Run(() => memWorker.StartReadCycle(TimeSpan.FromSeconds(0.1), null, cancelToken.Token));
        for (var i = 0; i < 100; i++)
        {
            Thread.Sleep(TimeSpan.FromSeconds(0.1));
            var v = memWorker.Buffer;
            System.Console.WriteLine(v);
            System.Console.WriteLine($"Bytes: {string.Join(", ", MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref v, 1)).ToArray())}");
        }
        
        cancelToken.Cancel();
        System.Console.WriteLine("Token flagged");
        t.Wait();
    }

    static void PrintProcessesAndNamesAlphabetical()
    {
        System.Console.WriteLine("Process name: ID");
        System.Console.WriteLine(string.Join('\n', Process.GetProcesses()
            .Select(p => $"{p.ProcessName}: {p.Id}")
            .Order()));
    }

    static Process ReadProcess()
    {
        System.Console.WriteLine("Enter PID: ");
        var n = int.Parse(System.Console.ReadLine()!);
        return Process.GetProcessById(n);
    }

    static nint ReadAddress()
    {
        System.Console.WriteLine("Enter hex address: ");
        return nint.Parse(
            System.Console.ReadLine()!.TrimStart('0', 'x', 'X'),
            NumberStyles.HexNumber,
            null); 
    }
}