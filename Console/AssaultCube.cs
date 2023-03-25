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
    public static nint[] FromEntityAddressToHeadYPosOffsets = new nint[] { 0xC };
    public static nint[] FromEntityAddressToHeadZPosOffsets = new nint[] { 0x8 };
    public static nint[] FromEntityAddressToFeetZPosOffsets = new nint[] { 0x2C };
}