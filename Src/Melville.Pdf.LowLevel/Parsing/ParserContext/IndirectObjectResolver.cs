﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext;

public class IndirectObjectResolver : IIndirectObjectResolver
{
    private readonly Dictionary<(int, int), PdfIndirectObject> index = new();

    public IReadOnlyDictionary<(int, int), PdfIndirectObject> GetObjects() =>
        index;

    public PdfIndirectObject FindIndirect(int objectNumber, int generation)
    {
        if (index.TryGetValue((objectNumber, generation), out var existingReference)) 
            return existingReference;
        var ret = new PromisedIndirectObject(objectNumber,
            generation);
        index.Add((objectNumber, generation), ret);
        return ret;
    }

    public void AddLocationHint(PdfIndirectObject newItem)
    {
        var key = (newItem.ObjectNumber, newItem.GenerationNumber);
        if (index.TryGetValue(key, out var prior))
        {
            if (prior is not PromisedIndirectObject mut) return;
            mut.SetIfHasNoValue(newItem);
        }
        index[key] = newItem;
    }
}