// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using MemOps;

namespace Console;

public static unsafe class Program
{
    public static void Main()
    {
        var procNamesAndIds = Process.GetProcesses()
            .Select(p => $"{p.ProcessName} PID: {p.Id}")
            .Order();
        System.Console.WriteLine($"Process names and PIDs:\n{string.Join('\n', procNamesAndIds)}");
        
        System.Console.WriteLine("Enter PID: ");
        int.TryParse(System.Console.ReadLine(), NumberStyles.Number, null, out var pid);
        var proc = Process.GetProcessById(pid);
        using var handle = ProcessOps.OpenProcessSafeHandle(proc, ProcessAccessRights.ProcessAllAccess);
        
        System.Console.WriteLine("Enter hex address to read: ");
        nint.TryParse(System.Console.ReadLine()!.TrimStart('0', 'x', 'X'), NumberStyles.HexNumber, null, out var address);
        System.Console.WriteLine(address);

        // ReadOnce(handle, address);
        ReadCycle(handle, address);
    }

    static void ReadOnce(SafeHandle handle, nint address)
    {
        MemoryOps.Read(handle, address.ToPointer(), out int n, true);
        System.Console.WriteLine(n);
    }

    static void ReadCycle(SafeHandle handle, nint address)
    {
        int n = default;
        var rwLock = new ReaderWriterLockSlim();
        new Thread(() => MemoryOps.ReadCycle(
                handle, 
                address.ToPointer(), 
                ref n, 
                rwLock, 
                TimeSpan.FromSeconds(0.75), 
                null,
                true))
            .Start();

        while (true)
        {
            Thread.Sleep(500);
            rwLock.EnterReadLock();
            System.Console.WriteLine(n);
            rwLock.ExitReadLock();
        }
    }
}