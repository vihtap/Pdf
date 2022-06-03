﻿using System;
using System.Buffers;
using System.Diagnostics;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.EncodedReaders;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.SegmentParsers.SymbolDictonaries;

public ref struct SymbolDictionaryParser
{
    private SequenceReader<byte> reader;
    private ReadOnlySpan<Segment> referencedSegments;
    private readonly SymbolDictionaryFlags flags;

    public SymbolDictionaryParser(in SequenceReader<byte> reader, in ReadOnlySpan<Segment> referencedSegments)
    {
        this.reader = reader;
        this.referencedSegments = referencedSegments;
        flags = new SymbolDictionaryFlags(this.reader.ReadBigEndianUint16());
    }

    public SymbolDictionarySegment Parse()
    {
        CheckTemporaryAssumptions(flags);
        var heightClassReader = flags.UseHuffmanEncoding?
            CompositeHeightClassReaderStrategy.Instance:
            ParseAtDecoder();
        var symbols = CreateSymbolArray();

        var intReader = flags.UseHuffmanEncoding
            ? HuffmanIntReader()
            : new ArithmeticIntegerDecoder();
        
        new SymbolParser(flags, intReader, symbols, heightClassReader).Parse(ref reader);
        
        return new SymbolDictionarySegment(symbols, ReadExportedSymbols(symbols, intReader));
    }

    private IEncodedReader HuffmanIntReader()
    {
        return new HuffmanIntegerDecoder()
        {
            ExportFlagsContext = StandardHuffmanTables.B1,
            DeltaHeightContext = GetHuffmanTable(flags.HuffmanSelectionForHeight).VerifyNoOutOfBand(),
            DeltaWidthContext = GetHuffmanTable(flags.HuffmanSelectionForWidth).VerifyHasOutOfBand(),
            BitmapSizeContext = GetHuffmanTable(flags.HuffmanSelectionBitmapSize).VerifyNoOutOfBand(),
            AggregationSymbolInstancesContext = GetHuffmanTable(flags.HuffmanTableSelectionAggInst).VerifyNoOutOfBand()
        };
    }

    private IHeightClassReaderStrategy ParseAtDecoder() => 
        new ArithmeticHeightClassReader(
            BitmapTemplateFactory.ReadContext(ref reader, flags.SymbolDictionaryTemplate));

    private Memory<IBinaryBitmap> ReadExportedSymbols(IBinaryBitmap[] symbols, IEncodedReader intReader)
    {
        var offset = intReader.ExportFlags(ref reader);
        var length = intReader.ExportFlags(ref reader);
        #warning right now this code assumes that the dictionary imports no other dictionaries, and exports all its symbols
        Debug.Assert(offset == 0);
        Debug.Assert(length == symbols.Length);
        return symbols.AsMemory(offset, length);
    }

    private IBinaryBitmap[] CreateSymbolArray()
    {
        var exportedSymbols = reader.ReadBigEndianUint32();
        var newSymbols = reader.ReadBigEndianUint32();
        if (exportedSymbols != newSymbols)
            throw new NotImplementedException("must export all symbols");
        var symbols = new IBinaryBitmap[newSymbols];
        return symbols;
    }

    private static void CheckTemporaryAssumptions(SymbolDictionaryFlags flags)
    {
        if (flags.AggregateRefinement)
            throw new NotImplementedException("Aggregate refinement is not implemented yet");
        if (flags.AggregateRefinement && !flags.RefAggTeqmplate)
            throw new NotImplementedException("Parsing refinement ATflags is not supported");
    }

    private HuffmanLine[] GetHuffmanTable(HuffmanTableSelection selection) =>
        selection.GetTableLines(ref referencedSegments);
}