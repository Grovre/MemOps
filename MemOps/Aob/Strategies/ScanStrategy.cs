namespace MemOps.Aob.Strategies;

public interface ScanStrategy
{
    nint Scan(ReadOnlySpan<byte> pattern, ReadOnlySpan<byte> mask, ReadOnlySpan<byte> data);
}