using MemOps.Enums;

namespace MemOps.Extensions;

public static class ProcessAccessRightsExtensions
{
    public static bool HasFlagFast(this ProcessAccessRights value, ProcessAccessRights flag)
    {
        return (value & flag) != 0;
    }
}