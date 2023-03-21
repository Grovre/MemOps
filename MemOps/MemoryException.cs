using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace MemOps;

public sealed class MemoryException : ExternalException
{
    public static MemoryException FromMostRecentPInvokeError()
        => new(Marshal.GetLastPInvokeErrorMessage(), Marshal.GetLastPInvokeError());

    public MemoryException()
    {
    }

    public MemoryException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public MemoryException(string? message) : base(message)
    {
    }

    public MemoryException(string? message, Exception? inner) : base(message, inner)
    {
    }

    public MemoryException(string? message, int errorCode) : base(message, errorCode)
    {
    }
}