using MemOps.Aob;
using MemOps.Aob.Strategies;

namespace Testing;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class AobTests
{
    private readonly ThreadLocal<byte[]> _gigabyteData = new(() => new byte[1024 * 1024 * 1024]);
    private const int PatternLength = 19;
    
    private record ScanTest(byte[] Data, byte[] Pattern, byte[] Mask, nint ExpectedResult);

    private ScanTest PrepareScanTest()
    {
        var random = new Random();
        
        var dataToScan = _gigabyteData.Value!;
        random.NextBytes(dataToScan);
        
        var pattern = new byte[PatternLength];
        random.NextBytes(pattern);
        var patternIndex = random.Next(dataToScan.Length - PatternLength);
        pattern.AsSpan().CopyTo(dataToScan.AsSpan()[patternIndex..]);
        
        var mask = new byte[pattern.Length];
        for (var i = 0; i < mask.Length; i++)
            mask[i] = 0xFF;

        var randomMaskIndex = random.Next(mask.Length);
        while (randomMaskIndex == 0 || randomMaskIndex == pattern.Length - 1)
            randomMaskIndex = random.Next(mask.Length);

        mask[randomMaskIndex] = 0;
        
        return new ScanTest(dataToScan, pattern, mask, patternIndex);
    }
    
    [Test]
    public void TestLinearScan()
    {
        var test = PrepareScanTest();
        
        var scanner = new AobScanner(test.Data, new LinearScan());
        nint result = -1;
        
        Assert.DoesNotThrow(() => result = scanner.Scan(test.Pattern, test.Mask));
        Assert.That(result, Is.EqualTo(test.ExpectedResult));
    }
}