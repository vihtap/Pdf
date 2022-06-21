﻿using System.Diagnostics;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

public readonly record struct ContextBitRun(sbyte X, sbyte Y, byte Length, byte MinBit)
{
    public int NextBit() => MinBit + Length;
}

public readonly struct BitmapTemplate
{
    private readonly ContextBitRun[] runs;

    public int BitsRequired() => runs[0].NextBit();

    public BitmapTemplate(ContextBitRun[] runs)
    {
        this.runs = runs;
    }
    public ushort ReadContext(IBinaryBitmap bitmap, int row, int col)
    {
        ushort ret = 0;
        foreach (var run in runs)
        {
            var runPtr = bitmap.PointerFor(row + run.Y, col + run.X); 
            for (int i = 0; i < run.Length; i++)
            {
                ret <<= 1;
                ret |= runPtr.CurrentValue;
                runPtr.Increment();
            }
        }
        return ret;
    }
}