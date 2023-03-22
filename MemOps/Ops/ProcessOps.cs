#pragma warning disable CA1416

using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.System.Threading;
using MemOps.Enums;

namespace MemOps.Ops;

/// <summary>
/// Static class providing methods to work with processes, including external Win32 calls.
/// Functions that open handles do not inherit any handle.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class ProcessOps // Somehow this is inconsistent naming
{
    public static SafeHandle OpenProcessSafeHandle(uint processId, ProcessAccessRights combinedRights)
        => PInvoke.OpenProcess_SafeHandle((PROCESS_ACCESS_RIGHTS)combinedRights, false, processId);

    public static SafeHandle OpenProcessSafeHandle(this Process proc, ProcessAccessRights combinedRights)
        => OpenProcessSafeHandle((uint)proc.Id, combinedRights);

    /// <summary>
    /// Searches for the specified module ignoring case
    /// </summary>
    /// <param name="process">The process of the modules being searched</param>
    /// <param name="moduleName">The name of the module to find in the process</param>
    /// <returns>A module if found, otherwise null</returns>
    public static ProcessModule? SearchModuleByFileNameIgnoreCase(this Process process, string moduleName)
    {
        return process
            .Modules
            .Cast<ProcessModule>()
            .FirstOrDefault(mod => string.Equals(
                mod.ModuleName, 
                moduleName,
                StringComparison.OrdinalIgnoreCase));
    }
}
