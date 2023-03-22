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
    private const int Pid = 18600;
    private const nint BaseAddress = 0x4B4C9A00;
    private static readonly nint[] OffsetsFromBaseAddress = { 0x60, 0x1F0, 0x88, 0x38, 0x1A8, 0x158, 0x330 };
    
    public static void Main()
    {
        var proc = Process.GetProcessById(Pid);
        using var handle = proc.OpenProcessSafeHandle(ProcessAccessRights.ProcessAllAccess);
        var address = MemoryOps.FollowOffsets(handle, BaseAddress, OffsetsFromBaseAddress);

        var memWorker = new BufferedMemoryAddress<int>(handle, address, false);
        memWorker.Read();
        System.Console.WriteLine(memWorker.Buffer);
    }

    static void PrintProcessesAndNamesAlphabetical()
    {
        System.Console.WriteLine("Process name: ID");
        System.Console.WriteLine(string.Join('\n', Process.GetProcesses()
            .Select(p => $"{p.ProcessName}: {p.Id}")
            .Order()));
        System.Console.WriteLine();
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