using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace MemOps.Exceptions;

/// <summary>
/// Exceptions relating to memory I/O using external Win32 memory I/O calls
/// </summary>
[Serializable]
public sealed class MemoryException : ExternalException
{
    /// <summary>
    /// Creates a MemoryException based off of the last PInvoke errors
    /// </summary>
    /// <returns>MemoryException using last PInvoke errors</returns>
    public static MemoryException FromMostRecentPInvokeError(string? hint = null)
    {
        var message = Marshal.GetLastPInvokeErrorMessage();
        if (hint != null)
            message = message + "\n" + hint;
        return new(message, Marshal.GetLastPInvokeError());
    }

    public MemoryException()
    {
    }

    // SonarLint said make private, so made private
    private MemoryException(SerializationInfo info, StreamingContext context) : base(info, context)
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