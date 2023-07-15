﻿using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Postscript.Interpreter.Values;
using Microsoft.CodeAnalysis;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public class S7_3NewObjects
{
    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void CreateBooleanValue(bool value, string str)
    {
        var pdfValue = ((PdfDirectValue)value);
        Assert.Equal(value, pdfValue.Get<bool>());
        Assert.Equal(str, pdfValue.Get<string>());
        Assert.Equal(str, pdfValue.ToString());

        Assert.True(pdfValue.IsBool);
        Assert.False(pdfValue.IsNull);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NullTests(bool explicitNull)
    {
        PdfDirectValue value = explicitNull?PdfDirectValue.CreateNull() : default;
        Assert.Equal("null", value.ToString());
        Assert.Equal("null", value.Get<string>());
        Assert.True(value.IsNull);
        Assert.False(value.IsBool);
    }

    [Theory]
    [InlineData(1, "1")]
    [InlineData(1024, "1024")]
    [InlineData(long.MaxValue, "9223372036854775807")]
    public void IntegerTests(long value, string str)
    {
        var valueLong = (PdfDirectValue)(long)value;
        Assert.Equal((long)value, valueLong.Get<long>());
        Assert.Equal((double)value, valueLong.Get<double>());
        Assert.Equal(str, valueLong.ToString());
        Assert.True(valueLong.IsNumber);
        Assert.True(valueLong.IsInteger);
        Assert.False(valueLong.IsDouble);

        if (value < int.MaxValue)
        {
            var valueInt = (PdfDirectValue)value;
            Assert.Equal(value, valueInt.Get<int>());
            Assert.Equal(str, valueInt.ToString());
            Assert.Equal(valueInt, valueLong);
        }

    }

    [Theory]
    [InlineData(0.0, 0, "0")]
    [InlineData(10.0, 10, "10")]
    [InlineData(10.49, 10, "10.49")]
    [InlineData(10.51, 11, "10.51")]
    public void DoubleTests(double value, int intValue, string str)
    {
        var pdfValue = (PdfDirectValue)value;
        Assert.Equal(value, pdfValue.Get<double>());
        Assert.Equal(intValue, pdfValue.Get<int>());
        Assert.Equal(str, pdfValue.ToString());
        Assert.True(pdfValue.IsNumber);
        Assert.False(pdfValue.IsInteger);
        Assert.True(pdfValue.IsDouble);
    }

    [Theory]
    [InlineData("abc","abc", false)]
    [InlineData("/abc","abc", true)]
    public void StringAndNameTests(string input, string value, bool isName)
    {
        var pdfValue = (PdfDirectValue)input;
        Assert.Equal(value, pdfValue.ToString());
        Assert.Equal(!isName, pdfValue.IsString);
        Assert.Equal(isName, pdfValue.IsName);

    }

    [Fact]
    public async Task GetEmbeddedIndirectAsync()
    {
        var indirect = (PdfIndirectValue)1;
        Assert.Equal("1", indirect.ToString());
        Assert.Equal("1", (await indirect.LoadValueAsync()).ToString());
    }

    [Fact]
    public async Task GetReferredIndirectAsync()
    {
        var registry = new Mock<IIndirectValueSource>();
        registry.Setup(i => i.Lookup(MementoUnion.CreateFrom(2L,5L))).Returns(
            new ValueTask<PdfDirectValue>(25));  

        var refValue = new PdfIndirectValue(registry.Object, 2, 5);

        Assert.Equal("25", (await refValue.LoadValueAsync()).ToString());
    }

    [Fact]
    public async Task ArrayaAccessAsync()
    {
        var value = PdfDirectValue.FromArray(1, 2.3, "Hello World"u8);
        Assert.Equal($"[1 2.3 Hello World]", value.ToString());
        var asArray = value.Get<PdfValueArray>();
        Assert.Equal(1, (await asArray[0]).Get<int>());
        Assert.Equal(2.3, (await asArray.RawItems[1].LoadValueAsync()).Get<double>(),4);
        Assert.Equal(3, asArray.Count);
    }

    [Fact]
    public void EmptyDictionaryExists() => Assert.Empty(PdfValueDictionary.Empty);

    [Fact]
    public void LongDictionaryTest()
    {
        var builder = new ValueDictionaryBuilder();
        for (int i = 0; i < 30; i++)
        {
            builder.WithItem($"/a{i}", i);
        }
        var dict = builder.AsDictionary();
        Assert.True(dict.RawItems["/a15"u8].TryGetEmbeddedDirectValue(out var value));
        Assert.Equal(15, value.Get<int>());
    }

    [Fact]
    public async Task StreamTest()
    {
        var str = new ValueDictionaryBuilder()
            .WithItem("/Length"u8, 11)
            .AsStream("Hello World");

        Assert.Equal(11, await str.DeclaredLengthAsync());

        Assert.Equal("Hello World",
            
            await new StreamReader(await str.StreamContentAsync()).ReadToEndAsync());
    }
}