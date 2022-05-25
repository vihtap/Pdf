﻿using System;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;

public interface IBinaryBitmap
{
    int Width { get; }
    int Height { get; }
    bool this[int row, int column] { get; set; }
    int Stride { get; }
    (byte[] Array, BitOffset Offset) ColumnLocation (int column);
}

public interface IBitmapCopyTarget : IBinaryBitmap
{
    void PasteBitsFrom(int row, int column, IBinaryBitmap source, CombinationOperator combOp);
}

public class BinaryBitmap: IBitmapCopyTarget
{
    public int Stride { get; }
    public int Width { get; }
    public int Height { get; }
    private readonly byte[] bits;

    public bool this[int row, int column]
    {
        get => ComputeBitPosition(row, column).GetBit(bits);
        set => ComputeBitPosition(row, column).WriteBit(bits, value);
    }


    public void PasteBitsFrom(int row, int column, IBinaryBitmap source, CombinationOperator combOp)
    {
        var copyRegion = new BinaryBitmapCopyRegion(row, column, source, this);
        if (copyRegion.UseSlowAlgorithm)
            PasteBitsFromSlow(source, combOp, copyRegion);
        else
        {
            PasteBitsFromFast(source, combOp, copyRegion);
        }
    }


    [Obsolete("This is a temporary method for profiling only")]
    public void PasteBitsFromSlow(int row, int column, IBinaryBitmap source, CombinationOperator combOp)
    {
        var copyRegion = new BinaryBitmapCopyRegion(row, column, source, this);
        PasteBitsFromSlow(source, combOp, copyRegion);
    }

    private void PasteBitsFromFast(IBinaryBitmap source, CombinationOperator combOp, 
        BinaryBitmapCopyRegion copyRegion)
    {
        var srcLocation = source.ColumnLocation(copyRegion.SourceFirstCol);
        var destLocation = this.ColumnLocation(copyRegion.DestinationFirstCol);
        unsafe
        {
            fixed(byte* srcPointer = srcLocation.Array)
            fixed (byte* destPointer = destLocation.Array)
            {
                var plan = BitCopierFactory.Create(
                    srcLocation.Offset.BitOffsetRightOfMsb, destLocation.Offset.BitOffsetRightOfMsb,
                    copyRegion.RowLength, combOp);
                var rows = copyRegion.Height;
                var sourceStride = source.Stride;
                var destStride = Stride;
                var currentSrc = srcPointer + srcLocation.Offset.ByteOffset + (
                    copyRegion.SourceFirstRow * sourceStride);
                var currentDest = destPointer + destLocation.Offset.ByteOffset + (
                        copyRegion.DestinationFirstRow * destStride);
                for (int i = 0; i < rows; i++)
                {
                    plan.Copy(currentSrc, currentDest);
                    currentSrc += sourceStride;
                    currentDest += destStride;
                }

            }
        }
            
//        PasteBitsFromSlow(source, combOp, copyRegion);
    }
    private void PasteBitsFromSlow(IBinaryBitmap source, CombinationOperator combOp, BinaryBitmapCopyRegion copyRegion)
    {
        var destRow = copyRegion.DestinationFirstRow;
        for (var i = copyRegion.SourceFirstRow; i < copyRegion.SourceExclusiveEndRow; i++, destRow++)
        {
            int destColumn = copyRegion.DestinationFirstCol;
            for (int j = copyRegion.SourceFirstCol; j < copyRegion.SourceExclusiveEndCol; j++, destColumn++)
            {
                AssignPixel(destRow, destColumn, source[i, j], combOp);
            }
        }
    }

    private void AssignPixel(int outputRow, int outputCol, bool sourcePixel, CombinationOperator combinationOperator)
    {
        switch (combinationOperator)
        {
            case CombinationOperator.Or:    
                this[outputRow, outputCol] |= sourcePixel;                
                break;
            case CombinationOperator.And:
                this[outputRow, outputCol] &= sourcePixel;                
                break;
            case CombinationOperator.Xor:
                this[outputRow, outputCol] ^= sourcePixel;                
                break;
            case CombinationOperator.Xnor:
                this[outputRow, outputCol] = !(this[outputRow, outputCol] ^ sourcePixel);                
                break;
            case CombinationOperator.Replace:
                this[outputRow, outputCol] = sourcePixel;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(combinationOperator), combinationOperator, null);
        }
    }

    private BitOffset ComputeBitPosition(int row, int col)
    {
        Debug.Assert(row >= 0);
        Debug.Assert(row < Height);
        Debug.Assert(col >= 0);
        Debug.Assert(col<= Width);
        return new((uint)((row * Stride) + (col >> 3)), (byte)(col & 0b111));
    }

    public (byte[] Array, BitOffset Offset) ColumnLocation(int column) => (bits, ComputeBitPosition(0, column));

    public BinaryBitmap(int height, int width)
    {
        Width = width;
        Height = height;
        Stride = (width + 7) / 8;
        bits = new byte[Stride * Height];
    }

    public Span<byte> AsByteSpan() => bits.AsSpan();

    public void FillBlack()
    {
        AsByteSpan().Fill(0xFF);
    }
}