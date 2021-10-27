﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Melville.Pdf.LowLevel.Writers.Builder
{
    public static class ObjectStreamCreation
    {
        public static  ValueTask<PdfStream> NewObjectStream(params PdfIndirectReference[] objectRefs) => 
            NewObjectStream(objectRefs, KnownNames.FlateDecode, PdfTokenValues.Null);
        
        public static async ValueTask<PdfStream> NewObjectStream(
            IEnumerable<PdfIndirectReference> objectRefs, FilterName? encoding, PdfObject? parameters = null)
        {
            var contentStreamInfo = await CreateContentStream(objectRefs);

            return new DictionaryBuilder()
                .WithFilter(encoding)
                .WithFilterParam(parameters)
                .WithItem(KnownNames.Type, KnownNames.ObjStm)
                .WithItem(KnownNames.N, new PdfInteger(contentStreamInfo.N))
                .WithItem(KnownNames.First, new PdfInteger(contentStreamInfo.First))
                .AsStream(contentStreamInfo.Data);
        }

        private static async ValueTask<ObjectStringInfo> CreateContentStream(
            IEnumerable<PdfIndirectReference> objectRefs)
        {
            int n = 0;
            var refs = new MultiBufferStream();
                                              var objects = new MultiBufferStream();
            var refWriter = new CountingPipeWriter(PipeWriter.Create(refs));
            var objectsWriter = new CountingPipeWriter(PipeWriter.Create(objects));
            var writingVisitor = new PdfObjectWriter(objectsWriter);
            foreach (var item in objectRefs)
            {
                n++;
                WriteObjectPosition(refWriter, item, objectsWriter.BytesWritten);
                await WriteObject(item, writingVisitor, objectsWriter);
            }
            await refWriter.FlushAsync();
            await objectsWriter.FlushAsync();
            return new ObjectStringInfo(n, refWriter.BytesWritten, 
                new ConcatStream(refs.CreateReader(), objects.CreateReader()));
        }

        private static void VeriftyLegalWrite(PdfIndirectReference item, PdfObject directValue)
        {
            if (directValue is PdfStream)
                throw new InvalidOperationException("Cannot add a stream to an object stream.");
            if (item.Target.GenerationNumber != 0)
                throw new InvalidOperationException("Only objects with generation 0 may be added to an object stream,");
        }

        private static async Task WriteObject(PdfIndirectReference item, PdfObjectWriter writingVisitor,
            CountingPipeWriter objectsWriter)
        {
            var directValue = await item.DirectValueAsync();
            VeriftyLegalWrite(item, directValue);
            await directValue.Visit(writingVisitor);
            objectsWriter.WriteLineFeed();
        }

        private static void WriteObjectPosition(PipeWriter refWriter, PdfIndirectReference item, long objectPosition)
        {
            IntegerWriter.Write(refWriter, item.Target.ObjectNumber);
            refWriter.WriteSpace();
            IntegerWriter.Write(refWriter, objectPosition);
            refWriter.WriteSpace();
        }
    }

    public readonly struct ObjectStringInfo
    {
        public int N { get; }
        public long First { get; }
        public Stream Data { get; }

        public ObjectStringInfo(int n, long first, Stream data)
        {
            N = n;
            First = first;
            Data = data;
        }
    }
}