// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Globalization;
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
        var handle = ProcessOps.OpenProcessSafeHandle((uint)pid, ProcessAccessRights.ProcessAllAccess);
        
        System.Console.WriteLine("Enter 0x address to read: ");
        nint.TryParse(System.Console.ReadLine(), NumberStyles.HexNumber, null, out var address);
        MemoryOps.Read(handle, address.ToPointer(), out int read);
        System.Console.WriteLine(read);
    }
}