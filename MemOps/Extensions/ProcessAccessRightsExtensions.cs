using MemOps.Enums;

namespace MemOps.Extensions;

public static class ProcessAccessRightsExtensions
{
    public static bool HasFlagFast(this ProcessAccessRights value, ProcessAccessRights flag)
    {
        return (value & flag) != 0;
    }

    public static ProcessAccessRights CombineRights(this ProcessAccessRights right, params ProcessAccessRights[] rights)
    {
        foreach (var r in rights.AsSpan())
            right |= r;
        return right;
    }

    public static ProcessAccessRights CombineRights(this Span<ProcessAccessRights> rights)
    {
        var r = (ProcessAccessRights)0;
        foreach (var right in rights)
            r |= right;
        return r;
    }

    public static ProcessAccessRights CombineRights(this ProcessAccessRights[] rights)
        => CombineRights(rights.AsSpan());
}