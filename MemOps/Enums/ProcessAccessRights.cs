using PAR = Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

namespace MemOps.Enums;

[Flags]
// ReSharper disable once InconsistentNaming
public enum ProcessAccessRights : uint
{
    ProcessTerminate = PAR.PROCESS_TERMINATE,
    ProcessCreateThread = PAR.PROCESS_CREATE_THREAD,
    ProcessSetSessionId = PAR.PROCESS_SET_SESSIONID,
    ProcessVmOperation = PAR.PROCESS_VM_OPERATION,
    ProcessVmRead = PAR.PROCESS_VM_READ,
    ProcessVmWrite = PAR.PROCESS_VM_WRITE,
    ProcessDupHandle = PAR.PROCESS_DUP_HANDLE,
    ProcessCreateProcess = PAR.PROCESS_CREATE_PROCESS,
    ProcessSetQuota = PAR.PROCESS_SET_QUOTA,
    ProcessSetInformation = PAR.PROCESS_SET_INFORMATION,
    ProcessQueryInformation = PAR.PROCESS_QUERY_INFORMATION,
    ProcessSuspendResume = PAR.PROCESS_SUSPEND_RESUME,
    ProcessQueryLimitedInformation = PAR.PROCESS_QUERY_LIMITED_INFORMATION,
    ProcessSetLimitedInformation = PAR.PROCESS_SET_LIMITED_INFORMATION,
    ProcessAllAccess = PAR.PROCESS_ALL_ACCESS,
    ProcessDelete = PAR.PROCESS_DELETE,
    ProcessReadControl = PAR.PROCESS_READ_CONTROL,
    ProcessWriteDac = PAR.PROCESS_WRITE_DAC,
    ProcessWriteOwner = PAR.PROCESS_WRITE_OWNER,
    ProcessSynchronize = PAR.PROCESS_SYNCHRONIZE,
    ProcessStandardRightsRequired = PAR.PROCESS_STANDARD_RIGHTS_REQUIRED,
}