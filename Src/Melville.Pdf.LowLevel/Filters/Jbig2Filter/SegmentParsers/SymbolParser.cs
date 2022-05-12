﻿using System;
using System.Buffers;
using System.Diagnostics;
using Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;

public ref struct SymbolParser
{
    private SequenceReader<byte> reader = default;
    private readonly SymbolDictionaryFlags headerFlags;
    private readonly HuffmanTable heightReader;
    private readonly HuffmanTable widthReader;
    private readonly HuffmanTable sizeReader;
    private readonly IBinaryBitmap[] result;
    private int bitmapsDecoded = 0;
    private int height = 0;

    public SymbolParser(SymbolDictionaryFlags headerFlags,
        HuffmanTable heightReader, HuffmanTable widthReader, HuffmanTable sizeReader, 
        IBinaryBitmap[] result)
    {
        Debug.Assert(widthReader.HasOutOfBandRow());
        Debug.Assert(!heightReader.HasOutOfBandRow());
        Debug.Assert(!sizeReader.HasOutOfBandRow());
        this.headerFlags = headerFlags;
        this.heightReader = heightReader;
        this.widthReader = widthReader;
        this.sizeReader = sizeReader;
        this.result = result;
    }

    public void Parse(ref SequenceReader<byte> reader)
    {
        this.reader = reader;
        Parse();
        reader = this.reader;
    }

    private void Parse()
    {
        if (headerFlags.AggregateRefinement | !headerFlags.UseHuffmanEncoding)
            throw new NotImplementedException("Only type 1 dictionary parsing is implemented");
        do
        {
            ReadHeightClass();
        } while (bitmapsDecoded < result.Length);
    }

    private void ReadHeightClass()
    {
        var source = new BitSource(reader);
        height += heightReader.GetInteger(ref source);
        var rowBitmap = ConstructCompositeBitmap(ref source);
        var bitmapLength = sizeReader.GetInteger(ref source);
        reader = source.Source;
        ReadBitmap(rowBitmap, bitmapLength);
    }

    private BinaryBitmap ConstructCompositeBitmap(ref BitSource source)
    {
        Span<int> widths = stackalloc int[result.Length - bitmapsDecoded];
        int totalWidth = 0;
        int localCount = 0;
        var priorWidth = 0;
        while (TryGetWidth(ref source, out var widthDelta))
        {
            widths[localCount] = (priorWidth += widthDelta);
            totalWidth += priorWidth;
            localCount++;
        }
        var rowBitmap = new BinaryBitmap(height, totalWidth);
        AddBitmaps(widths[..localCount], result.AsSpan(bitmapsDecoded..), rowBitmap);
        bitmapsDecoded += localCount;
        return rowBitmap;
    }

    private static void AddBitmaps(
        in Span<int> widths, in Span<IBinaryBitmap> dest, IBinaryBitmap rowBitmap)
    {
        if (widths.Length == 1)
            dest[0] = rowBitmap;
        else
            AddMultiBitmap(widths, dest, rowBitmap);
    }

    private static void AddMultiBitmap(Span<int> widths, Span<IBinaryBitmap> dest, IBinaryBitmap rowBitmap)
    {
        var offset = 0;
        for (int i = 0; i < widths.Length; i++)
        {
            dest[i] = new VerticalStripBitmap(rowBitmap, offset, widths[i]);
            offset += widths[i];
        }
    }

    private void ReadBitmap(BinaryBitmap rowBitmap, int bitmapLength)
    {
        if (bitmapLength == 0)
            throw new NotImplementedException("Unencoded bitmaps are not implemented");
        CreateMmrDecoder(rowBitmap).Convert(ref reader, rowBitmap.AsByteSpan());
    }

    private const int KValueThatGetsIgnored = 1000;
    private static CcittType4Decoder CreateMmrDecoder(BinaryBitmap rowBitmap) => new(
        new CcittParameters(KValueThatGetsIgnored, 
            encodedByteAlign:false, rowBitmap.Width, rowBitmap.Height, endOfBlock:false, blackIs1: true), 
            new TwoDimensionalLineCodeDictionary());

    private readonly bool TryGetWidth(ref BitSource source, out int value)
    {
        value = widthReader.GetInteger(ref source);
        return !widthReader.IsOutOfBand(value);
    }
}