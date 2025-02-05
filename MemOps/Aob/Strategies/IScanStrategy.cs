namespace MemOps.Aob.Strategies;

public interface IScanStrategy
{
    /// <summary>
    /// Scans for an array of bytes
    /// </summary>
    /// <param name="pattern">Pattern to find</param>
    /// <param name="mask">Mask of KNOWN bytes. If a mask byte is 0, the pattern byte is UNKNOWN. In other words: true if known, false otherwise.</param>
    /// <param name="data">Data to search for pattern in</param>
    /// <returns>First index of array</returns>
    nint Scan(ReadOnlySpan<byte> pattern, ReadOnlySpan<byte> mask, ReadOnlySpan<byte> data);
}