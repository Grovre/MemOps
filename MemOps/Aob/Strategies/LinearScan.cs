using MemOps.Exceptions;

namespace MemOps.Aob.Strategies;

public class LinearScan : ScanStrategy
{
    public nint Scan(ReadOnlySpan<byte> pattern, ReadOnlySpan<byte> mask, ReadOnlySpan<byte> data)
    {
        for (var i = 0; i < data.Length - pattern.Length; i++)
        {
            var found = true;
            for (var j = 0; j < pattern.Length; j++)
            {
                if (mask[j] == 0x00)
                {
                    continue;
                }

                if (pattern[j] != data[i + j])
                {
                    found = false;
                    break;
                }
            }

            if (found)
            {
                return i;
            }
        }

        throw new ScanFailedException("No AOB pattern found");
    }
}