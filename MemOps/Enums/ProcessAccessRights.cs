namespace MemOps.Enums;

[Flags]
// ReSharper disable once InconsistentNaming
public enum ProcessAccessRights
{
    ProcessTerminate = 0x00000001,
    ProcessCreateThread = 0x00000002,
    ProcessSetSessionId = 0x00000004,
    ProcessVmOperation = 0x00000008,
    ProcessVmRead = 0x00000010,
    ProcessVmWrite = 0x00000020,
    ProcessDupHandle = 0x00000040,
    ProcessCreateProcess = 0x00000080,
    ProcessSetQuota = 0x00000100,
    ProcessSetInformation = 0x00000200,
    ProcessQueryInformation = 0x00000400,
    ProcessSuspendResume = 0x00000800,
    ProcessQueryLimitedInformation = 0x00001000,
    ProcessSetLimitedInformation = 0x00002000,
    ProcessAllAccess = 0x001FFFFF,
    ProcessDelete = 0x00010000,
    ProcessReadControl = 0x00020000,
    ProcessWriteDac = 0x00040000,
    ProcessWriteOwner = 0x00080000,
    ProcessSynchronize = 0x00100000,
    ProcessStandardRightsRequired = 0x000F0000,
}