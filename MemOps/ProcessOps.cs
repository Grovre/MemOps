using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.System.Threading;

namespace MemOps;

public static class ProcessOps
{
    public static SafeHandle OpenProcessSafeHandle(uint processId, params ProcessAccessRights[] rights)
    {
        var finalRights = (PROCESS_ACCESS_RIGHTS)rights.Aggregate((ar1, ar2) => ar1 | ar2);
        return PInvoke.OpenProcess_SafeHandle
            (finalRights, false, processId);

#if DEBUG
        Console.WriteLine("Opened process with the following rights: ");
        var pars = Enum
            .GetValues<ProcessAccessRights>()
            .Where(v => ((ProcessAccessRights)finalRights).HasFlagFast(v));
        Console.WriteLine(string.Join(", ", pars));
#endif
    }

    public static SafeHandle OpenProcessSafeHandle(this Process proc, params ProcessAccessRights[] rights)
    {
        return OpenProcessSafeHandle((uint)proc.Id, rights);
    }
}

