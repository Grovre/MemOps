namespace MemOps.Aob.Strategies;

public interface IScanStrategy
{
    nint Scan(ReadOnlySpan<byte> pattern, ReadOnlySpan<byte> mask, ReadOnlySpan<byte> data);
}