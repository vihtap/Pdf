﻿using System;
using System.Buffers;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;
using Melville.Pdf.LowLevel.Filters.LzwFilter;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

public record struct ContextBitRun(sbyte X, sbyte Y, byte Length, byte MinBit)
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
    public ushort ReadContext(BinaryBitmap bitmap, int row, int col)
    {
        ushort ret = 0;
        foreach (var run in runs)
        {
            var runRow = run.Y + row;
            var baseColumn = run.X + col;
            for (int i = 0; i < run.Length; i++)
            {
                ret <<= 1;
                if (GetBit(bitmap, runRow, baseColumn + i)) ret |= 1;
            }
        }
        return ret;
    }

    private bool GetBit(BinaryBitmap bitmap, int row, int col) => 
        bitmap.ContainsPixel(row, col) && bitmap[row, col];
}

public unsafe ref struct BitmapTemplateFactory
{
    private const int MaxRuns = 7;
    public int Length { get; private set; }
    private fixed sbyte xs[MaxRuns];
    private fixed sbyte ys[MaxRuns];
    private fixed byte bitlengths[MaxRuns];
    private readonly SymbolDictionaryTemplate template;
    public BitmapTemplateFactory(SymbolDictionaryTemplate i)
    {
        Length = 0;
        template = i;
        AddDefaultSpans();
    }

    private void AddDefaultSpans()
    {
        switch (template)
        {
            case SymbolDictionaryTemplate.V0:
                AddRange(-2, -1, 3);
                AddRange(-1, -2, 5);
                AddRange(0, -4, 4);
                break;
            case SymbolDictionaryTemplate.V1:
                AddRange(-2, -1, 4);
                AddRange(-1, -2, 5);
                AddRange(0, -3, 3);
                break;
            case SymbolDictionaryTemplate.V2:
                AddRange(-2, -1, 3);
                AddRange(-1, -2, 4);
                AddRange(0, -2, 2);
                break;
            case SymbolDictionaryTemplate.V3:
                AddRange(-1, -3, 5);
                AddRange(0, -4, 4);
                break;
        }  
    }

    public void AddRange(sbyte y, sbyte x, byte length)
    {
        ys[Length] = y;
        xs[Length] = x;
        bitlengths[Length] = length;
        Length++;
    }
    public void AddPoint(sbyte row, sbyte column)
    {
        for (int i = 0; i < Length; i++)
        {
            if (WrongRow(row, i)) continue;
            var columnDelta = ColumnDelta(column, i);
            switch (columnDelta, columnDelta - bitlengths[i])
            {
                case (<-1,_): break; // > 1 below
                case (-1,_):  // 1 below
                    AddToLeftOfRun(i);
                    return;
                case (_,0): // 1 above
                    AddToRightOfRun(i);
                    return;
                case (_, > 0): // > 1 above
                    break;
                default: return; // inside a current range -- so we can ignore adding this pixel
                
            }
        }
        // if we get to here, we did not fit into any of the existing ranges.
        AddRange(row, column, 1);
    }

    private void AddToRightOfRun(int i) => bitlengths[i]++;

    private void AddToLeftOfRun(int i)
    {
        xs[i]--;
        AddToRightOfRun(i);
    }

    private int ColumnDelta(sbyte column, int i) => column - xs[i];

    private bool WrongRow(sbyte row, int i) => ys[i] != row;

    public BitmapTemplate Create() => 
        new BitmapTemplate(CreatRunCollection(CountTotalBits()));

    private ContextBitRun[] CreatRunCollection(int totalBitLen)
    {
        var runStorage = new ContextBitRun[Length];
        for (int i = 0; i < Length; i++)
        {
            totalBitLen -= bitlengths[i];
            runStorage[i] = new ContextBitRun(xs[i], ys[i], bitlengths[i], (byte)totalBitLen);
        }

        return runStorage;
    }

    private int CountTotalBits()
    {
        int totalBitLen = 0;
        for (int i = 0; i < Length; i++)
        {
            totalBitLen += bitlengths[i];
        }

        return totalBitLen;
    }

    public static BitmapTemplate ReadContext(ref SequenceReader<byte> source, SymbolDictionaryTemplate template)
    {
        var fact = new BitmapTemplateFactory(template);
        for (int i = 0; i < fact.ExpectedAdaptivePixels(); i++)
        {
            var x = source.ReadBigEndianInt8();
            var y = source.ReadBigEndianInt8();
            fact.AddPoint(y, x);
        }

        return fact.Create();
    }

    private int ExpectedAdaptivePixels() => (template == SymbolDictionaryTemplate.V0) ?4:1;
}