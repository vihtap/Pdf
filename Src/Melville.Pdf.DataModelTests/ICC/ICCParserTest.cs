﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Melville.Icc.Model;
using Melville.Icc.Model.Tags;
using Melville.Icc.Parser;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.ICC;

public class ICCParserTest
{
    [Fact]
    public async Task SizeField()
    {
      var source = LoadSampleData();
      var profile = await new IccParser(PipeReader.Create(source)).ParseAsync();

      Assert.Equal(60960u, profile.Header.Size);
      Assert.Equal(0u, profile.Header.CmmType);
      Assert.Equal(0x4200000u, profile.Header.Version);
      Assert.Equal(ProfileClass.ColorSpace, profile.Header.ProfileClass);
      Assert.Equal(ColorSpace.RGB, profile.Header.DeviceColorSpace);
      Assert.Equal(ColorSpace.Lab, profile.Header.ProfileConnectionColorSpace);
      Assert.Equal(new DateTime(2007,07, 25, 00, 05,37, DateTimeKind.Utc), profile.Header.CreatedDate);
      Assert.Equal(0x61637370u, profile.Header.Signature);
      Assert.Equal(0u, profile.Header.Platform);
      Assert.Equal((ProfileFlags)0, profile.Header.ProfileFlags);
      Assert.Equal(0u, profile.Header.Manufacturer);
      Assert.Equal(0u, profile.Header.Device);
      Assert.Equal((DeviceAttributes)0, profile.Header.DeviceAttributes);
      Assert.Equal(RenderIntent.Perceptual, profile.Header.RenderIntent);
      Assert.Equal(0.96420f, profile.Header.Illuminant.X,4);
      Assert.Equal(1f, profile.Header.Illuminant.Y);
      Assert.Equal(0.82491f, profile.Header.Illuminant.Z, 4);
      Assert.Equal(0u, profile.Header.Creator);
      Assert.Equal((ulong)0x34562ABF994CCd06, profile.Header.ProfileIdHigh);
      Assert.Equal((ulong)0x6D2C5721D0D68C5D, profile.Header.ProfileIdLow);

      Assert.Equal(9, profile.Tags.Count);
      Assert.Equal(StrEnc("desc"), profile.Tags[0].Tag);
      Assert.Equal(240u, profile.Tags[0].Offset);
      Assert.Equal(118u, profile.Tags[0].Size);
      Assert.Equal(StrEnc("chad"), profile.Tags[8].Tag);
      Assert.Equal(60916u, profile.Tags[8].Offset);
      Assert.Equal(44u, profile.Tags[8].Size);
    }

    private uint StrEnc(string s)
    {
        uint ret = 0;
        foreach (var character in s)
        {
            ret <<= 8;
            ret |= (byte)character;
        }

        return ret;
    }

    private Stream LoadSampleData() => 
        GetType().Assembly.GetManifestResourceStream("Melville.Pdf.DataModelTests.ICC.sample.icc")!;

    private static async Task<T> ParseTag<T>(string source) where T:ProfileData
    {
        var PdfString = (PdfString)await (source).ParseObjectAsync();
        var reader = new ReadOnlySequence<byte>(PdfString.Bytes);
        return (T)TagParser.Parse(reader);
    }

    [Fact]
    public async Task ChromacityParser()
    {
        var tag = await ParseTag<ChromacityTag>("<6368726d0000000000030002" +
                                                "0001 0001 0002 0002" +
                                                "000A0002 00002000" +
                                                "00B00003 00003000>");
        Assert.Equal(3, tag.Channels);
        Assert.Equal(Colorant.SMPTEP145, tag.Colorant);
        Assert.Equal((1.0000153f, 2.0000305f), tag.Coordinates[0]);
        Assert.Equal((10.0000305f, 0.1250019f), tag.Coordinates[1]);
        Assert.Equal((176.00005f, 0.18750286f), tag.Coordinates[2]);
    }

    [Fact]
    public async Task ParseColorOrder()
    {
        var tag = await ParseTag<ColorOrderTag>("<636c726f00000000 00000003 030201>");
        Assert.Equal(3, tag.Colors.Count);
        Assert.Equal(3, tag.Colors[0]);
        Assert.Equal(2, tag.Colors[1]);
        Assert.Equal(1, tag.Colors[2]);
    }

    [Fact]
    public async Task ColorantTableType()
    {
        var tag = await ParseTag<ColorantTableTag>("<636c727400000000 00000002" +
                                                   "6162636400000000000000000000000000000000000000000000000000000000" +
                                                   "0003 0002 0001" +
                                                   "6364656600000000000000000000000000000000000000000000000000000000" +
                                                   "0005 0006 0007" +
                                                   ">");
        Assert.Equal(2, tag.Colorants.Count);
        Assert.Equal("abcd", tag.Colorants[0].Name);
        Assert.Equal(3, tag.Colorants[0].X);
        Assert.Equal(2, tag.Colorants[0].Y);
        Assert.Equal(1, tag.Colorants[0].Z);
        Assert.Equal("cdef", tag.Colorants[1].Name);
        Assert.Equal(5, tag.Colorants[1].X);
        Assert.Equal(6, tag.Colorants[1].Y);
        Assert.Equal(7, tag.Colorants[1].Z);
    }

    [Fact]
    public async Task CurveTagTest()
    {
        var tag = await ParseTag<CurveTag>("<6375727600000000 00000005 0005 0004 0003 0002 0001>");
        Assert.Equal(5, tag.Values.Count);
        Assert.Equal(5, tag.Values[0]);
        Assert.Equal(4, tag.Values[1]);
        Assert.Equal(3, tag.Values[2]);
        Assert.Equal(2, tag.Values[3]);
        Assert.Equal(1, tag.Values[4]);
    }

    [Fact]
    public async Task DataTagTest()
    {
        var tag = await ParseTag<DataTag>("<6461746100000000 00000000 616263646500>");
        Assert.Equal(DataType.String, tag.Type);
        Assert.Equal(new byte[]{0x61, 0x62, 0x63, 0x64, 0x65,0}, tag.Data);
        Assert.Equal("abcde", tag.AsString());
        
    }

    [Fact]
    public async Task DateTimeTagTest()
    {
        var tag = await ParseTag<DateTimeTag>("<6474696d 00000000 000A00010002 000300040005>");
        Assert.Equal(new DateTime(10,1,2,3,4,5,0, DateTimeKind.Utc), tag.DateTime);
    }

    [Fact]
    public async Task Lut16TagTest()
    {
        var tag = await ParseTag<LutXTag>("<6d66743200000000 02010300 " +
           // matrix
          "00010000 00000000 00000000" +
          "00000000 00010000 00000000" +
          "00000000 00000000 00010000" +
           //input and output table size
          "00030004" +
           // input table 2 inputs * 3 entries = 6
           "0001 0002 0003 0004 0005 0006" +
           // clut = 3 grid points ^ 2 inputs * 1 output = 9
           "0001 0002 0003 0004 0005 0006 0007 0008 0009" +
           // output table = 4 output table entries * 1 output
           "0001 0002 0003 0004>");
        
        Assert.Equal(2, tag.Inputs);
        Assert.Equal(1, tag.Outputs);
        Assert.Equal(3, tag.GridPoints);
        
        Assert.Equal(1f, tag.Matrix.M11);
        Assert.Equal(0f, tag.Matrix.M12);
        Assert.Equal(0f, tag.Matrix.M13);
        Assert.Equal(0f, tag.Matrix.M21);
        Assert.Equal(1f, tag.Matrix.M22);
        Assert.Equal(0f, tag.Matrix.M23);
        Assert.Equal(0f, tag.Matrix.M31);
        Assert.Equal(0f, tag.Matrix.M32);
        Assert.Equal(1f, tag.Matrix.M33);

        Assert.Equal(3, tag.InputTableEntries);
        Assert.Equal(4, tag.OutputTableEntries);

        Assert.Equal(IncrementingFloatArray(6,16), tag.InputTables);
        Assert.Equal(IncrementingFloatArray(9,16), tag.Clut);
        Assert.Equal(IncrementingFloatArray(4,16), tag.OutputTables);
    }

    private float[] IncrementingFloatArray(int len, int bits)
    {
        float epsilon = 1.0f / ((1 << bits) - 1);
        return Enumerable.Range(1, len).Select(i => i * epsilon).ToArray();
    }
    [Fact]
    public async Task Lut8TagTest()
    {
        var tag = await ParseTag<LutXTag>("<6d66743100000000 02010300 " +
                                          // matrix
                                          "00010000 00000000 00000000" +
                                          "00000000 00010000 00000000" +
                                          "00000000 00000000 00010000" +
                                          //input and output table size
                                          "00030004" +
                                          // input table 2 inputs * 3 entries = 6
                                          "01 02 03 04 05 06" +
                                          // clut = 3 grid points ^ 2 inputs * 1 output = 9
                                          "01 02 03 04 05 06 07 08 09" +
                                          // output table = 4 output table entries * 1 output
                                          "01 02 03 04>");
        
        Assert.Equal(2, tag.Inputs);
        Assert.Equal(1, tag.Outputs);
        Assert.Equal(3, tag.GridPoints);
        
        Assert.Equal(1f, tag.Matrix.M11);
        Assert.Equal(0f, tag.Matrix.M12);
        Assert.Equal(0f, tag.Matrix.M13);
        Assert.Equal(0f, tag.Matrix.M21);
        Assert.Equal(1f, tag.Matrix.M22);
        Assert.Equal(0f, tag.Matrix.M23);
        Assert.Equal(0f, tag.Matrix.M31);
        Assert.Equal(0f, tag.Matrix.M32);
        Assert.Equal(1f, tag.Matrix.M33);

        Assert.Equal(3, tag.InputTableEntries);
        Assert.Equal(4, tag.OutputTableEntries);

        AssertFloatArraySame(IncrementingFloatArray(6,8), tag.InputTables, 0.0000001);
        AssertFloatArraySame(IncrementingFloatArray(9,8), tag.Clut, 0.00000001);
        AssertFloatArraySame(IncrementingFloatArray(4,8), tag.OutputTables, 0.00000001);
    }

    private void AssertFloatArraySame(float[] f1, IReadOnlyList<float> f2, double tolerence)
    {
        Assert.Equal(f1.Length, f2.Count);
        Assert.True(f1.Zip(f2, (i,j)=>Math.Abs(i-j) < tolerence).All(i=>i));
    }

    [Fact]
    public async Task MeasurementTypeTest()
    {
        var tag = await ParseTag<MeasurementTYpeTag>("<6d65617300000000 00000001 " +
                                                     "0001 0000 00020000 00030000" +
                                                     "00000002 00010000 00000005>");

        Assert.Equal(StandardObserver.Cie1931, tag.Observer);
        Assert.Equal(1, tag.MeasurementBacking.X);
        Assert.Equal(2, tag.MeasurementBacking.Y);
        Assert.Equal(3, tag.MeasurementBacking.Z);
        Assert.Equal(MeasurmentGeomenty.a0, tag.Geometry);
        Assert.Equal(MeasurmentFlare.f100, tag.Flare);
        Assert.Equal(StandardIllumination.D55, tag.Illumination);
    }
}