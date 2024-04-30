using System.Runtime.InteropServices;

namespace MemOps.Exceptions;

/// <summary>
///     Exceptions relating to memory I/O using external Win32 memory I/O calls
/// </summary>
public sealed class MemoryException : ExternalException
{
    public MemoryException()
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

    /// <summary>
    ///     Creates a MemoryException based off of the last PInvoke errors
    /// </summary>
    /// <returns>MemoryException using last PInvoke errors</returns>
    public static MemoryException FromMostRecentPInvokeError(string? hint = null)
    {
        var message = Marshal.GetLastPInvokeErrorMessage();
        if (hint != null)
            message = $"{message}\n{hint}";
        return new MemoryException(message, Marshal.GetLastPInvokeError());
    }
}