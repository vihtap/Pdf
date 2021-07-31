﻿using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext
{
    public partial class ParsingFileOwner
    {
        private readonly Stream source;
        public long StreamLength => source.Length;
        public IPdfObjectParser RootObjectParser { get; }
        public IIndirectObjectResolver IndirectResolver { get; }

        public ParsingFileOwner(
            Stream source, IPdfObjectParser? rootObjectParser = null, IIndirectObjectResolver? indirectResolver = null)
        {
            this.source = source;
            RootObjectParser = rootObjectParser ?? new PdfCompositeObjectParser();
            IndirectResolver = indirectResolver ?? new IndirectObjectResolver();
            if (!source.CanSeek) throw new PdfParseException("PDF Parsing requires a seekable stream");
        }

        private object? currentReader = null;

        private void SeekToRentedOrigin(long offset)
        {
            // we may eventually want a multithreaed version of this so we can load multiple pages on different
            // threads
            if (currentReader != null) throw new InvalidOperationException("May only create one reader at a time");
            source.Seek(offset, SeekOrigin.Begin);
        }

        public ValueTask<IParsingReader> RentReader(long offset)
        {
            SeekToRentedOrigin(offset);
            var reader = new ParsingReader(this, PipeReader.Create(source, pipeOptions), offset);
            currentReader = reader;
            return new ValueTask<IParsingReader>(reader);
        }

        public ValueTask<Stream> RentStream(long position, long length)
        {
            SeekToRentedOrigin(position);
            var ret = new RentedStream(source, length, this);
            currentReader = ret;
            return new ValueTask<Stream>(ret);
        }

        private static readonly StreamPipeReaderOptions pipeOptions = new(leaveOpen: true);

        private void ReturnReader(object item)
        {
            if (item != currentReader) throw new InvalidOperationException("Returned reader is not current");
            currentReader = null;
        }

    }
}