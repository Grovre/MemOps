namespace Console;

public class AssaultCube
{
    public const string ModuleName = "ac_client";
    public const int MaxPlayers = 32;
    public static nint[] MyPlayerOffsets = new nint[] { 0x0017B0B8, 0x0 };
    public static nint[] EntityListOffsets = new nint[] { 0x187C10, 0x0 };
    public static nint[] FromEntityAddressToNameOffsets = new nint[] { 0x205 };
    public static nint[] FromEntityAddressToRifleAmmoOffsets = new nint[] { 0x140 };
    public static nint[] FromEntityAddressToHeadXPosOffsets = new nint[] { 0x4 };
    public static nint[] FromEntityAddressToFeetXPosOffsets = new nint[] { 0x28 };
    public static nint[] FromEntityAddressToHealthOffsets = new nint[] { 0xEC };
    public static nint[] LobbyPlayerCountOffsets = new nint[] { 0x187C18 };
}