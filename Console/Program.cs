// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using MemOps.DataStructures;
using MemOps.Enums;
using MemOps.Ops;

namespace Console;

public static unsafe class Program
{
    public static void Main()
    {
        using var proc = Process.GetProcessesByName(AssaultCube.ModuleName)[0];
        var baseAddr = proc.MainModule!.BaseAddress;
        using var handle = proc.OpenProcessSafeHandle(ProcessAccessRights.ProcessAllAccess);

        var myPlayerAddr = MemoryOps.FollowOffsets(handle, baseAddr, AssaultCube.MyPlayerOffsets);
        System.Console.WriteLine(myPlayerAddr.ToString("X"));

        for (var i = 0; i < 1_000; i++)
        {
            Thread.Sleep(10);
            var rifleAmmo = new BufferedMemoryAddress<int>(
                handle, 
                MemoryOps.FollowOffsets(handle, myPlayerAddr, AssaultCube.FromEntityAddressToRifleAmmoOffsets),
                false);
            rifleAmmo.Buffer = 999;
            rifleAmmo.Write();
            
            var x = new BufferedMemoryAddress<float>(
                handle,
                MemoryOps.FollowOffsets(handle, myPlayerAddr, AssaultCube.FromEntityAddressToHeadXPosOffsets),
                false);
            var y = new BufferedMemoryAddress<float>(
                handle,
                MemoryOps.FollowOffsets(handle, myPlayerAddr, AssaultCube.FromEntityAddressToHeadYPosOffsets),
                false);
            var z = new BufferedMemoryAddress<float>(
                handle,
                MemoryOps.FollowOffsets(handle, myPlayerAddr, AssaultCube.FromEntityAddressToFeetZPosOffsets),
                false);
            var v = new Vector3();
            x.Read(out v.X);
            y.Read(out v.Y);
            z.Read(out v.Z);
            System.Console.WriteLine(v);
        }
    }
}