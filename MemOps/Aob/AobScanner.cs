﻿using MemOps.Aob.Strategies;

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
        return _scanStrategy.Scan(pattern, mask, _data.Span);
    }
}