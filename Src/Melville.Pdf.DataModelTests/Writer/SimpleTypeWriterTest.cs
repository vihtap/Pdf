﻿using System.IO;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;
using Xunit;

namespace Melville.Pdf.DataModelTests.Writer
{
    public class SimpleTypeWriterTest
    {
        public Task<string> DoWrite(PdfObject source)
        {
            var target = new MemoryStream();
            var writer = new PdfObjectWriter(PipeWriter.Create(target));
            source.Visit(writer);
            return Task.FromResult(Encoding.UTF8.GetString(target.ToArray()));
        }
        
        [Fact]
        public async Task WriteTokens()
        {
            Assert.Equal("true",await DoWrite(PdfBoolean.True));
            Assert.Equal("false",await DoWrite(PdfBoolean.False));
            Assert.Equal("null",await DoWrite(PdfTokenValues.Null));
            Assert.Equal("]",await DoWrite(PdfTokenValues.ArrayTerminator));
            Assert.Equal(">>",await DoWrite(PdfTokenValues.DictionaryTerminator));
            
        }
        
        [Theory]
        [InlineData("Hello", "(Hello)")]
        [InlineData("", "()")]
        [InlineData(@"this is a \Test", @"(this is a \\Test)")]
        [InlineData(@"this is a (Test", @"(this is a \(Test)")]
        [InlineData(@"this is a )Test", @"(this is a \)Test)")]
        public async Task WriteStrings(string source, string dest)
        {
            Assert.Equal(dest, await DoWrite(new PdfString(source)));
        }
        [Theory]
        [InlineData("Hello", "/Hello")]
        [InlineData("Hel#lo", "/Hel#23lo")]
        [InlineData("Hel lo", "/Hel#20lo")]
        public async Task WriteName(string source, string dest)
        {
            Assert.Equal(dest, await DoWrite(new PdfName(source)));
        }
        [Theory]
        [InlineData(0, "0")]
        [InlineData(1234, "1234")]
        [InlineData(-1234, "-1234")]
        public async Task WriteIntegers(int source, string dest)
        {
            Assert.Equal(dest, await DoWrite(new PdfInteger(source)));
        }
        [Theory]
        [InlineData(0, "0")]
        [InlineData(1234, "1234")]
        [InlineData(-1234, "-1234")]
        [InlineData(-1234.54, "-1234.54")]
        public async Task WriteDoubles(double source, string dest)
        {
            Assert.Equal(dest, await DoWrite(new PdfDouble(source)));
        }

        [Fact]
        public async Task WriteIndirectObjectReference()
        {
            var reference = new PdfIndirectReference(new PdfIndirectObject(34, 555, PdfBoolean.False));
            Assert.Equal("34 555 R", await DoWrite(reference));

        }
        [Fact]
        public async Task WriteIndirectObject()
        {
            var reference = new PdfIndirectObject(34, 555, PdfBoolean.False);
            Assert.Equal("34 555 obj false endobj", await DoWrite(reference));
        }
    }
}