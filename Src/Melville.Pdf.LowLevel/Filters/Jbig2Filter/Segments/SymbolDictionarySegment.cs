﻿using System;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

public class SymbolDictionarySegment : DictionarySegment
{
    public SymbolDictionarySegment(IBinaryBitmap[] allSymbols) :
        this(allSymbols, allSymbols.AsMemory()){}
    public SymbolDictionarySegment(IBinaryBitmap[] allSymbols, Memory<IBinaryBitmap> exportedSymbols) : 
        base(SegmentType.SymbolDictionary, allSymbols, exportedSymbols)
    {
    }
}