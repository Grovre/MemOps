// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using MemOps.DataStructures;
using MemOps.Enums;
using MemOps.Extensions;
using MemOps.Ops;

namespace Console;

public static class Program
{
    /*
     * Message from me on 3/25 at 10:04 PM:
     * DON'T FORGET TO TARGET A 32-BIT BUILD FOR AC_CLIENT. JUST WASTED 2.5 HRS OF MY LIFE
     * TRYING TO FIGURE OUT WHY DEREFERENCING A POINTER WAS RESULTING
     * IN WEIRD AND IRRELEVANT ADDRESSES. THERE WAS NOTHING WRONG WITH THE CODE.
     * IT WAS BECAUSE IT WAS TARGETING A 64-BIT BUILD SO NINT WAS READING 4
     * MORE BYTES THAN IT WOULD ON A 32-BIT PROCESS. EVEN ADDED A RUNTIME
     * CHECK MAKING SURE YOU AREN'T STUPID LIKE ME.
     */
    public static void Main()
    {
        using var proc = Process.GetProcessesByName(AssaultCube.ModuleName)[0];
        var baseAddr = proc.MainModule!.BaseAddress;
        using var handle = proc.OpenProcessSafeHandle(ProcessAccessRights.ProcessAllAccess);
        Trace.Assert(Marshal.SizeOf<nint>() == 4, "Not 32-bit");
        
        BeforePlayerAddress: // Too lazy to loop it. Thanks Microsoft for "goto"
        
        var myPlayerAddr = MemoryOps.FollowOffsets(handle, baseAddr, AssaultCube.MyPlayerOffsets);
        
        
        var intMemPosBuilder = new BufferedMemoryAddress<int>.Builder(handle)
            .SetPrintOnReadOrWrite(false)
            .SetAddress(myPlayerAddr);
        
        var playerHpAddr = intMemPosBuilder
            .Build()
            .FollowOffsets(AssaultCube.FromEntityAddressToHealthOffsets);
        
        // Just restart the whole scanning thing if the player is dead
        // because when the player is dead, their pointer points to the
        // 4th dimension instead. The health there won't be 0-100.
        // I don't know what it points to but it seems to be
        // an entity structure template? Also too lazy to wrap this in a loop
        playerHpAddr.Read();
        if (playerHpAddr.Buffer is <= 0 or > 100)
            goto BeforePlayerAddress;
        
        var playerRifleAmmoAddr = intMemPosBuilder
            .SetBuffer(999)
            .Build()
            .FollowOffsets(AssaultCube.FromEntityAddressToRifleAmmoOffsets);
        
        
        var entityMemPosBuilder = new BufferedMemoryAddress<Vector3>.Builder(handle)
            .SetPrintOnReadOrWrite(false)
            .SetAddress(myPlayerAddr);
        
        var playerHeadPosAddr = entityMemPosBuilder
            .Build()
            .FollowOffsets(AssaultCube.FromEntityAddressToHeadXPosOffsets);
        
        var playerFeetPosAddr = entityMemPosBuilder
            .Build()
            .FollowOffsets(AssaultCube.FromEntityAddressToFeetXPosOffsets);

        for (var i = 0; i < 1_000; i++)
        {
            Thread.Sleep(10);

            playerHpAddr.Read(out var hp); // Only this reads here because it must be known and is always right
            var vh = Vector3.Zero;
            var vf = Vector3.Zero;
            

            if (hp is <= 0 or > 100)
            {
                System.Console.WriteLine("You are dead");
            }
            else
            {
                playerRifleAmmoAddr.Write();
                playerHeadPosAddr.Read(out vh);
                playerFeetPosAddr.Read(out vf);
            }


            System.Console.WriteLine($"Iteration: {i}\nHead: {vh}\nFeet: {vf}\n");
        }
    }
}