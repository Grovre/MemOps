// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using MemOps.DataStructures;
using MemOps.Enums;
using MemOps.Ops;

namespace Console;

public static class Program
{
    public static void Main()
    {
        using var proc = Process.GetProcessesByName(AssaultCube.ModuleName)[0];
        var baseAddr = proc.MainModule!.BaseAddress;
        using var handle = proc.OpenProcessSafeHandle(ProcessAccessRights.ProcessAllAccess);

        var myPlayerAddr = MemoryOps.FollowOffsets(handle, baseAddr, AssaultCube.MyPlayerOffsets);
        System.Console.WriteLine(myPlayerAddr.ToString("X"));

        var rifleAmmo = new BufferedMemoryAddress<int>.Builder(handle)
            .SetAddress(MemoryOps.FollowOffsets(handle, myPlayerAddr, AssaultCube.FromEntityAddressToRifleAmmoOffsets))
            .SetBuffer(999)
            .Build();
        var entityMemPosBuilder = new BufferedMemoryAddress<Vector3>.Builder(handle).SetPrintOnReadOrWrite(false);
        var playerHeadPosAddr = entityMemPosBuilder
            .SetAddress(MemoryOps.FollowOffsets(handle, myPlayerAddr, AssaultCube.FromEntityAddressToHeadXPosOffsets))
            .Build();
        var playerFeetPosAddr = entityMemPosBuilder
            .SetAddress(MemoryOps.FollowOffsets(handle, myPlayerAddr, AssaultCube.FromEntityAddressToFeetXPosOffsets))
            .Build();
        
        for (var i = 0; i < 1_000; i++)
        {
            Thread.Sleep(10);
            
            rifleAmmo.Write();
            playerHeadPosAddr.Read(out var vh);
            playerFeetPosAddr.Read(out var vf);
            System.Console.WriteLine($"Head: {vh}, Feet: {vf}");
        }
    }
}