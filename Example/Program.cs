// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using MemOps.DataStructures;
using MemOps.Enums;
using MemOps.Ops;

namespace Example;

public static class Program
{
    /*
     * Message from me on 3/25 at 10:04 PM:
     * DON'T FORGET TO TARGET A 32-BIT BUILD FOR 32-BIT APPS. Just wasted hours of my life
     * trying to figure out why dereferencing a pointer was resulting
     * in weird and irrelevant addresses. THERE WAS NOTHING WRONG WITH THE CODE.
     * Tt was because I was targeting a 64-bit build so NINT WAS READING 4
     * MORE BYTES THAN IT WOULD ON A 32-BIT PROCESS. even added a runtime
     * check making sure you aren't stupid like me. Don't be like me.
     */
    public static void Main()
    {
        Console.WriteLine($"This process is x64: {Environment.Is64BitProcess}");
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
        // an entity structure template or an existing entity?
        // Also too lazy to wrap this in a loop
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
            Thread.Sleep(15);

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
                
                // Read multiple test done here,
                // Player assumed to be standing still.
                // It works btw, and if it throws because
                // someone wanted to play with this example
                // then they can be smart enough to
                // debug this and remove it when they play.
                // I don't condone cheating in games
                unsafe
                {
                    var headFloats = new float[3];
                    MemoryOps.ReadMultiple<float>(handle, playerHeadPosAddr.Address.ToPointer(), headFloats);
                    var vht = new Vector3(headFloats);
                    Debug.Assert(vh == vht);

                    var feetFloats = headFloats;
                    MemoryOps.ReadMultiple<float>(handle, playerFeetPosAddr.Address.ToPointer(), feetFloats);
                    var vft = new Vector3(feetFloats);
                    Debug.Assert(vf == vft);
                }
            }


            System.Console.WriteLine($"Iteration: {i}\nHead: {vh}\nFeet: {vf}\n");

            // Commented because we don't want to read the entity list, but keep the code because it works
            /*
            var entListAddr = baseAddr
                .ToAddress<nint>(handle)
                .FollowOffsets(AssaultCube.EntityListOffsets);
            for (var j = 1; j < AssaultCube.MaxPlayers; j++)
            {
                try
                {
                    var entListEntAddr = entListAddr.FollowOffsets(j * 0x4, 0x0);
                    var entHp = entListEntAddr.FollowOffsetsAndRead(AssaultCube.FromEntityAddressToHealthOffsets);
                    if (entHp.Buffer is <= 0 or > 100)
                        continue;
                    var headPos = entListEntAddr.WithType<Vector3>()
                        .FollowOffsetsAndRead(AssaultCube.FromEntityAddressToHeadXPosOffsets);
                    var feetPos = entListEntAddr.WithType<Vector3>()
                        .FollowOffsetsAndRead(AssaultCube.FromEntityAddressToFeetXPosOffsets);
                    System.Example.WriteLine($"Entity {j}:\nHead: {headPos.Buffer}\nFeet: {feetPos.Buffer}\n");
                }
                catch (MemoryException e)
                {
                    System.Example.WriteLine(e.Message);
                    // Entity doesn't exist, is dead, broken or this code is broken
                }
            }
            */
        }
    }
}