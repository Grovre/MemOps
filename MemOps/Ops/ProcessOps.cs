#pragma warning disable CA1416

using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.System.Threading;
using MemOps.Enums;

namespace MemOps.Ops;

/// <summary>
///     Static class providing methods to work with processes, including external Win32 calls.
///     Functions that open handles do not inherit any handle.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class ProcessOps // Somehow this is inconsistent naming, wants to be "Ops"
{
    /// <summary>
    ///     Opens a safe handle from the Win32 OpenProcess function.
    ///     Handles are not inherited.
    /// </summary>
    /// <param name="processId">The PID of the process to open</param>
    /// <param name="combinedRights">Rights to open handle with</param>
    /// <returns>Opened handle to process with rights</returns>
    public static SafeHandle OpenProcessSafeHandle(uint processId, ProcessAccessRights combinedRights)
    {
        return PInvoke.OpenProcess_SafeHandle((PROCESS_ACCESS_RIGHTS)combinedRights, false, processId);
    }

    /// <summary>
    ///     Opens a safe handle from the Win32 OpenProcess function.
    ///     Handles are not inherited.
    /// </summary>
    /// <param name="proc">The process to open</param>
    /// <param name="combinedRights">Rights to open handle with</param>
    /// <returns>Opened handle to process with rights</returns>
    public static SafeHandle OpenProcessSafeHandle(this Process proc, ProcessAccessRights combinedRights)
    {
        return OpenProcessSafeHandle((uint)proc.Id, combinedRights);
    }

    /// <summary>
    ///     Searches for the specified module ignoring case
    /// </summary>
    /// <param name="process">The process of the modules being searched</param>
    /// <param name="moduleName">The name of the module to find in the process</param>
    /// <returns>A module if found, otherwise null</returns>
    public static ProcessModule? SearchModuleByFileNameIgnoreCase(this Process process, string moduleName)
    {
        return process.Modules
            .Cast<ProcessModule>()
            .FirstOrDefault(mod => string.Equals(
                mod.ModuleName,
                moduleName,
                StringComparison.OrdinalIgnoreCase));
    }
}