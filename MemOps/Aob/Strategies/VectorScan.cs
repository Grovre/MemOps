using System.Numerics;
using System.Runtime.InteropServices;

namespace MemOps.Aob.Strategies;

public class VectorScan : IScanStrategy
{
    public VectorScan()
    {
        throw new NotImplementedException(
            "VectorScan is not implemented properly yet.");
    }

    public nint Scan(ReadOnlySpan<byte> pattern, ReadOnlySpan<byte> mask, ReadOnlySpan<byte> data)
    {
        var patternLength = pattern.Length;
        var maskLength = mask.Length;
        var dataLength = data.Length;

        if (patternLength != maskLength)
            throw new ArgumentException("Pattern and mask must be the same length");

        if (patternLength > dataLength)
            throw new ArgumentException("Pattern length must be less than or equal to data length");

        var patternVector = MemoryMarshal.Cast<byte, Vector<byte>>(pattern);
        var maskVector = MemoryMarshal.Cast<byte, Vector<byte>>(mask);
        var dataVector = MemoryMarshal.Cast<byte, Vector<byte>>(data);

        for (var i = 0; i < dataLength - patternLength; i++)
        {
            var found = true;
            for (var j = 0; j < patternVector.Length; j++)
            {
                var maskResult = Vector.Equals(maskVector[j], Vector<byte>.Zero);
                var patternResult = Vector.Equals(patternVector[j], dataVector[i + j]);

                var result = Vector.BitwiseAnd(maskResult, patternResult);
                if (result != Vector<byte>.Zero)
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