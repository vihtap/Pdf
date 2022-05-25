﻿using System;
using System.Buffers;
using Melville.Icc.Model.Tags;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.TextRegions;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class TextRegionParserTest
{
    private static TextRegionSegment Parse(byte[] bits)
    {
        var reader = new SequenceReader<byte>(
            new ReadOnlySequence<byte>(
                bits
            )
        );
        return TextRegionSegmentParser.Parse(reader,  ReferredDictionary().AsSpan());
    }

    private static Segment[] ReferredDictionary()
    {
        var d1 = @"
BBBB.
B...B
B...B
B...B
BBBB.
B....
B....
B....".AsBinaryBitmap(8, 5);
        var d2 = @"
.BBBB..BBBB.
B....B.....B
B......BBBBB
B.....B....B
B....BB....B
.BBBB..BBBBB".AsBinaryBitmap(6, 12);
        return new Segment[]
        {
            new SymbolDictionarySegment(new IBinaryBitmap[]{d1}),
            new SymbolDictionarySegment(new IBinaryBitmap[]
            {
                new HorizontalStripBitmap(d2, 0,6),
                new HorizontalStripBitmap(d2, 6,6)
            })
        };
    }

    [Fact]
    public void ParseTextSegment()
    {
        var data = "00000025 00000008 00000004 00000001 00 0C09 0010 00000005 01100000000000000000000000000000000C" +
                   "4007087041D0";
        var sut = Parse(data.BitsFromHex());
        Assert.Equal(0x25, sut.Bitmap.Width);
        Assert.Equal(8, sut.Bitmap.Height);
    }

    [Fact]
    public void ParseRegionHeader()
    {
        var data = "00000025 00000008 00000004 00000001 01";
        var reader = ReaderFromHexString(data);
        var region = RegionHeaderParser.Parse(ref reader);
        Assert.Equal(0x25u, region.Width);
        Assert.Equal(0x08u, region.Height);
        Assert.Equal(0x04u, region.X);
        Assert.Equal(0x01u, region.Y);
        Assert.Equal(CombinationOperator.And, region.CombinationOperator);
    }

    private static SequenceReader<byte> ReaderFromHexString(string data) =>
        new(new ReadOnlySequence<byte>(data.BitsFromHex()));

    private static SequenceReader<byte> ReaderFromBinaryString(string data) =>
        new(new ReadOnlySequence<byte>(data.BitsFromBinary()));

    [Fact]
    public unsafe void ParseSymbolDictionaryTest()
    {
        var data = "50033532530000000000000000000000350F8B309EB85F1DD28300";
        var reader = ReaderFromHexString(data);
        var ptr = stackalloc HuffmanLine[32];
        var destination = new Span<HuffmanLine>(ptr, 32);
        TextSegmentSymbolTableParser.Parse(ref reader, destination);

        Assert.Equal(0, destination[28].PrefixLengh);
        Assert.Equal(0, destination[29].PrefixLengh);
        Assert.Equal(0, destination[30].PrefixLengh);
        Assert.Equal(0, destination[31].PrefixLengh);

        CheckRead(destination, 3, 0, 8);
        CheckRead(destination, 3, 1, 26);
        CheckRead(destination, 4, 8, 13);
        CheckRead(destination, 4, 10, 29);
        CheckRead(destination, 4, 4, 9);
        CheckRead(destination, 4, 5, 10);
        CheckRead(destination, 4, 6, 11);
        CheckRead(destination, 4, 7, 12);
        CheckRead(destination, 4, 9, 14);
        CheckRead(destination, 5, 27, 25);
        CheckRead(destination, 5, 26, 24);
        CheckRead(destination, 5, 25, 23);
        CheckRead(destination, 5, 24, 22);
        CheckRead(destination, 5, 23, 21);
        CheckRead(destination, 5, 22, 20);
        CheckRead(destination, 6, 60, 27);
        CheckRead(destination, 6, 59, 7);
        CheckRead(destination, 6, 58, 6);
        CheckRead(destination, 6, 57, 5);
        CheckRead(destination, 6, 56, 4);
        CheckRead(destination, 7, 122, 16);
        CheckRead(destination, 7, 123, 19);
        CheckRead(destination, 7, 124, 28);
        CheckRead(destination, 7, 125, 30);
        CheckRead(destination, 7, 126, 31);
        CheckRead(destination, 8, 254, 18);
        CheckRead(destination, 9, 511, 17);
        CheckRead(destination, 9, 510, 3);
    }

    private void CheckRead(in Span<HuffmanLine> destination, int bits, int bitData, int result)
    {
        int leadingBits = bitData << (16 - bits);
        byte[] data = { (byte)(leadingBits >> 8), (byte)leadingBits };
        var reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(data));
        var source = new BitSource(reader);
        Assert.Equal(result, source.ReadHuffmanInt(destination));
    }
}