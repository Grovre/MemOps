using MemOps.Aob.Strategies;
using MemOps.Exceptions;

namespace MemOps.Aob;

public class AobScanner
{
    private Memory<byte> _data;
    private IScanStrategy _scanStrategy;

    public AobScanner(Memory<byte> data, IScanStrategy scanStrategy)
    {
        _data = data;
        _scanStrategy = scanStrategy;
    }

    public nint Scan(ReadOnlySpan<byte> pattern, ReadOnlySpan<byte> mask)
    {
        var i = _scanStrategy.Scan(pattern, mask, _data.Span);
        if (i < 0)
            throw new ScanFailedException("Pattern not found");

        return i;
    }
}