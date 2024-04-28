using MemOps.Aob;
using MemOps.Aob.Strategies;

namespace Testing;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class AobTests
{
    ThreadLocal<byte[]> _100MbData = new(() => new byte[1024 * 1024 * 100]);
    private const int PatternLength = 19;

    private byte[] prepareScanTest(Span<byte> dataToScan, int patternLength)
    {
        var random = new Random();
        random.NextBytes(dataToScan);
        var pattern = new byte[patternLength];
        random.NextBytes(pattern);
        pattern[random.Next(patternLength)] = 0;
        pattern.AsSpan().CopyTo(dataToScan[random.Next(dataToScan.Length - patternLength)..]);
        return pattern;
    }
    
    [Test]
    public void TestLinearScan()
    {
        var data = _100MbData.Value;
        var pattern = prepareScanTest(data, PatternLength);
        
        var mask = new byte[pattern.Length];
        for (var i = 0; i < mask.Length; i++)
            mask[i] = 0xFF;
        
        var scanner = new AobScanner(data, new LinearScan());
        nint result = -1;
        
        Assert.DoesNotThrow(() => result = scanner.Scan(pattern, mask));
        Assert.That(result, Is.GreaterThanOrEqualTo(nint.Zero));
    }
}