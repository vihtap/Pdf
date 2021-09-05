﻿using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.Decryptors;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption
{
    public partial class DecryptionInterceptTests
    {
        [Fact]
        public async Task LoadDecodedString()
        {
            var source = new MemoryStream("(AbCd)".AsExtendedAsciiBytes());
            var reader = new ConstDecryptor(new ParsingReader(null!, PipeReader.Create(source), 0));
            Assert.Equal("abcd", (await new PdfCompositeObjectParser().ParseAsync(reader)).ToString());
            
        }
        [Fact]
        public async Task LoadDecodedStream()
        {
            var source = new MemoryStream("<</Length 4>> stream\r\nABCD endstream".AsExtendedAsciiBytes());
            var reader = new ConstDecryptor(
                new ParsingReader(new ParsingFileOwner(new MemoryStream()), PipeReader.Create(source), 0));
            var str = (PdfStream)await new PdfCompositeObjectParser().ParseAsync(reader);
            var output = await new StreamReader(await str.GetEncodedStreamAsync()).ReadToEndAsync();
            Assert.Equal("Decoded", output);
        }
        
        private class DecryptorFake: IDecryptor
        {
            public void DecryptStringInPlace(PdfString input)
            {
                for (int i = 0; i < input.Bytes.Length; i++)
                {
                    input.Bytes[i] |= 0x20;
                } 
            }

            public Stream WrapRawStream(Stream input) => new MemoryStream("Decoded".AsExtendedAsciiBytes());
        }
        
        private partial class ConstDecryptor: IParsingReader
        {
            [DelegateTo] private readonly IParsingReader reader;

            public ConstDecryptor(IParsingReader reader)
            {
                this.reader = reader;
            }

            public IDecryptor Decryptor() => new DecryptorFake();
        }

    }
}