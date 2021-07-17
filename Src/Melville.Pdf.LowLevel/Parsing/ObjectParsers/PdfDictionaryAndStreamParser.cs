﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using PdfDictionary = Melville.Pdf.LowLevel.Model.Objects.PdfDictionary;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers
{
    public class InlineStreamSource: IStreamDataSource
    {
        private long sourceFilePosition;

        public InlineStreamSource(long sourceFilePosition)
        {
            this.sourceFilePosition = sourceFilePosition;
        }

        public ValueTask<Stream> OpenRawStream(long streamLength)
        {
            throw new System.NotImplementedException();
        }
    }

    public class PdfDictionaryAndStreamParser : IPdfObjectParser
    {
        public async Task<PdfObject> ParseAsync(ParsingSource source)
        {
            var reader = await source.ReadAsync();
            //This has to succeed because the prior parser looked at the prefix to get here.
            source.AdvanceTo(reader.Buffer.GetPosition(2));
            var dictionary = new Dictionary<PdfName, PdfObject>();
            while (true)
            {
                var key = await source.RootObjectParser.ParseAsync(source);
                if (key == PdfTokenValues.DictionaryTerminator)
                {
                    bool isStream;
                    do {}while (source.ShouldContinue(
                        CheckForStreamTrailer(await source.ReadAsync(), out isStream)));
                    return CreateFinalObject(source, dictionary, isStream);
                }

                var item = await source.RootObjectParser.ParseAsync(source);
                if (item == PdfTokenValues.Null) continue;
                AddItemToDictionary(dictionary, key, item);
            }
        }

        private static PdfObject CreateFinalObject(
            ParsingSource source, Dictionary<PdfName, PdfObject> dictionary, bool isStream)
        {
            //TODO: See how much the trim helps in memory and costs in speed.
            dictionary.TrimExcess();
            return isStream ? 
                new PdfStream(dictionary, new InlineStreamSource(source.Position)) : 
                new PdfDictionary(dictionary);
        }

        private static void AddItemToDictionary(Dictionary<PdfName, PdfObject> dictionary, PdfObject key,
            PdfObject item)
        {
            if (item == PdfTokenValues.DictionaryTerminator)
                throw new PdfParseException("Dictionary must have an even number of children.");
            dictionary[CheckIfKeyIsName(key)] = item;
        }

        private static PdfName CheckIfKeyIsName(PdfObject? name) =>
            name is PdfName typedAsName ? typedAsName:
                throw new PdfParseException("Dictionary keys must be names");

        private static byte[] streamSuffix = {115, 116, 114, 101, 97, 109}; // stream
        private (bool success, SequencePosition pos) CheckForStreamTrailer(ReadResult source, out bool isStream)
        {
            isStream = false;
            var reader = new SequenceReader<Byte>(source.Buffer);
            if (!FindStreamSuffix(ref reader)) return (false, reader.Position);
            if (!reader.TryCheckToken(streamSuffix, out isStream)) return (false, reader.Position);
            if (!isStream) return (true, source.Buffer.Start);
            if (!SkipOverStreamSuffix(ref reader)) return (false, reader.Position);
            isStream = true;
            return (true, reader.Position);
        }

        private static bool FindStreamSuffix(ref SequenceReader<byte> reader) => 
            NextTokenFinder.SkipToNextToken(ref reader);

        private static bool SkipOverStreamSuffix(ref SequenceReader<byte> reader) => 
            reader.TryAdvanceTo((byte)'\n');

    }
}