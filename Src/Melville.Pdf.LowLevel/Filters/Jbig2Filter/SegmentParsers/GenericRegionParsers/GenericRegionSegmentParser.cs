﻿using System.Buffers;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.GenericRegionParsers;

public readonly struct GenericRegionSegmentFlags
{
    private readonly byte data;

    public GenericRegionSegmentFlags(byte data)
    {
        this.data = data;
    }

    /// <summary>
    /// In Spec MMR
    /// </summary>
    public bool UseMmr => BitOperations.CheckBit(data, 0x01);
    /// <summary>
    /// In Spec GBTEMPLATE
    /// </summary>
    public byte GBTemplate => (byte)BitOperations.UnsignedInteger(data, 1, 3);

    public bool Tpgdon => BitOperations.CheckBit(data, 8);  

}

public static class GenericRegionSegmentParser
{
    public static GenericRegionSegment Parse(SequenceReader<byte> reader, Segment[] empty)
    {
        var regionHead = RegionHeaderParser.Parse(ref reader);
        var flags = new GenericRegionSegmentFlags(reader.ReadBigEndianUint8());
        var bitmap = regionHead.CreateTargetBitmap();
        new GenericRegionReader(bitmap, flags.UseMmr).ReadFrom(ref reader, false);
       
        return new GenericRegionSegment(SegmentType.ImmediateLosslessGenericRegion,
            regionHead, bitmap);
    }
}