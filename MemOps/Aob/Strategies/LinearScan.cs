using MemOps.Exceptions;

namespace MemOps.Aob.Strategies;

public class LinearScan : IScanStrategy
{
    public nint Scan(ReadOnlySpan<byte> pattern, ReadOnlySpan<byte> mask, ReadOnlySpan<byte> data)
    {
        if (mask[0] == 0 || mask[^1] == 0)
            throw new ScanFailedException("Mask starts or ends with 0 (unmasked/unknown)");

        Span<bool> fastMask = stackalloc bool[mask.Length];
        for (var i = 0; i < mask.Length; i++)
            fastMask[i] = mask[i] != 0;

        var searchEndIndex = data.Length - pattern.Length;
        var p0 = pattern[0];
        for (var i = 0; i < searchEndIndex; i++)
        {
            if (p0 != data[i])
                continue;

            var found = true;
            for (var j = 1; j < pattern.Length; j++)
            {
                if (pattern[j] != data[i + j] && fastMask[j])
                {
                    found = false;
                    break;
                }
            }

            if (found)
                return i;
        }

        return -1;
    }
}