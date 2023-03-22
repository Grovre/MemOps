// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Globalization;
using MemOps.DataStructures;
using MemOps.Enums;
using MemOps.Extensions;
using MemOps.Ops;

namespace Console;

public static unsafe class Program
{
    public static void Main()
    {
        PrintProcessesAndNamesAlphabetical();
        var proc = ReadProcess();
        var handleRights = new[]
        {
            ProcessAccessRights.ProcessVmOperation,
            ProcessAccessRights.ProcessVmRead,
            ProcessAccessRights.ProcessVmWrite
        }.CombineRights();
        var handle = proc.OpenProcessSafeHandle(handleRights);
        var address = ReadAddress();

        var memWorker = new MemoryAddress<int>(handle, address, false);
        var rwLock = new ReaderWriterLockSlim();
        var cancelToken = new CancellationTokenSource();
        var t = Task.Run(() => memWorker.StartReadCycle(TimeSpan.FromSeconds(0.1), rwLock, cancelToken.Token));
        for (var i = 0; i < 100; i++)
        {
            Thread.Sleep(TimeSpan.FromSeconds(0.1));
            rwLock.EnterReadLock();
            var v = memWorker.Buffer;
            rwLock.ExitReadLock();
            System.Console.WriteLine(v);
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