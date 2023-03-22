using MemOps.Enums;

namespace MemOps.Extensions;

/// <summary>
/// A set of extensions for common uses of the access right flags
/// </summary>
public static class ProcessAccessRightsExtensions
{
    /// <summary>
    /// Allegedly faster than the provided HasFlag method.
    /// Does a bitwise AND then compares with 0
    /// </summary>
    /// <param name="value">First flag</param>
    /// <param name="flag">Second flag</param>
    /// <returns>True if both flags have a flag set, otherwise false</returns>
    public static bool HasFlagFast(this ProcessAccessRights value, ProcessAccessRights flag)
    {
        return (value & flag) != 0;
    }

    /// <summary>
    /// Combines multiple flags for process access rights
    /// </summary>
    /// <param name="right">Initial flag</param>
    /// <param name="rights">Flags being added</param>
    /// <returns>Flags combined</returns>
    public static ProcessAccessRights CombineRights(this ProcessAccessRights right, params ProcessAccessRights[] rights)
    {
        foreach (var r in rights.AsSpan())
            right |= r;
        return right;
    }

    /// <summary>
    /// Combines multiple flags for process access rights
    /// </summary>
    /// <param name="rights">Flags to combine</param>
    /// <returns>Flags combined</returns>
    public static ProcessAccessRights CombineRights(this Span<ProcessAccessRights> rights)
    {
        var r = (ProcessAccessRights)0;
        foreach (var right in rights)
            r |= right;
        return r;
    }

    /// <summary>
    /// Combines multiple flags for process access rights
    /// </summary>
    /// <param name="rights">Flags to combine</param>
    /// <returns>Flags combined</returns>
    public static ProcessAccessRights CombineRights(this ProcessAccessRights[] rights)
        => CombineRights(rights.AsSpan());
}