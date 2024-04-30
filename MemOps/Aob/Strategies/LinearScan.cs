namespace MemOps.Aob.Strategies;

public class LinearScan : IScanStrategy
{
    public nint Scan(ReadOnlySpan<byte> pattern, ReadOnlySpan<byte> mask, ReadOnlySpan<byte> data)
    {
        for (var i = 0; i < data.Length - pattern.Length; i++)
        {
            var found = true;
            for (var j = 0; j < pattern.Length; j++)
            {
                if (mask[j] == 0)
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

        return -1;
    }
}