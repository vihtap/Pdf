﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;
internal class PdfArrayParser : IPdfObjectParser
{
    public async Task<PdfObject> ParseAsync(IParsingReader source)
    {
        var reader = await source.Reader.ReadAsync().CA();
        //This has to succeed because the prior parser looked at the prefix to get here.
        source.Reader.AdvanceTo(reader.Buffer.GetPosition(1));
        return new PdfArray(await RecursiveReadArrayAsync(source, 0).CA());
    }

    private const int RecursiveLimit = 500;

    private static async ValueTask<PdfObject[]> RecursiveReadArrayAsync(
        IParsingReader reader, int currentLocation)
    {
        // reading the array recursively allows us to use the stack, rather than the heap, to store intermediate values
        // until we discover the exact size of array that we need.
        //
        // Very big arrays can blow the stack of course, after 1000 iterations, which is bigger than 99% of pdf arrays
        // we bail out to a heap based solution
        
        var element = await reader.RootObjectParser.ParseAsync(reader).CA();
        
        if (element == PdfTokenValues.ArrayTerminator) return new PdfObject[currentLocation];
        var ret = await ReadRestOfArrayAsync(reader, currentLocation).CA();
        
        ret[currentLocation] = element;
        return ret;
    }

    private static ValueTask<PdfObject[]> ReadRestOfArrayAsync(IParsingReader reader, int currentLocation) =>
        IsBigArray(currentLocation) ? 
            ReadEndOfBigArrayUsingHeapAsync(reader) 
            : RecursiveReadArrayAsync(reader, currentLocation + 1);

    private static bool IsBigArray(int currentLocation) => currentLocation >= RecursiveLimit;

    private static async ValueTask<PdfObject[]> ReadEndOfBigArrayUsingHeapAsync(IParsingReader reader)
    {
        var items = new List<PdfObject>();
        while (true)
        {
            var item = await reader.RootObjectParser.ParseAsync(reader).CA();
            if (item == PdfTokenValues.ArrayTerminator) return CopyToHighPositions(items);
            items.Add(item);
        }
    }
    private static PdfObject[] CopyToHighPositions(List<PdfObject> items)
    {
        var ret = new PdfObject[items.Count + RecursiveLimit + 1];
        CollectionsMarshal.AsSpan(items).CopyTo(ret.AsSpan(RecursiveLimit + 1));
        return ret;
    }
}