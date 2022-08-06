﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType;

public static class GlyphNamerFactory
{
    public static IGlyphNameMap CreateMapping(Face face)
    {
        if (face.HasGlyphNames)
        {
            return new CompositeGlyphNameMap(
                new[]
                {
                    new GlyphNameReader(face).FontNamings(),
                    GlyphNameToUnicodeMap.AdobeGlyphList
                });
        }
        return GlyphNameToUnicodeMap.AdobeGlyphList;
    }
}

public readonly partial struct GlyphNameReader
{
    [FromConstructor] private readonly Face face;
    private readonly Dictionary<int, char> names = new();
    
    public  IGlyphNameMap FontNamings()
    {
        Debug.Assert(face.HasGlyphNames);
        ReadAllGlyphNames();
        return new GlyphNameToUnicodeMap(names);
    }
    
    private void ReadAllGlyphNames()
    {
        for (uint i = 0; i < face.GlyphCount; i++)
        {
            ReadSingleGlyphName(face, i);
        }
    }

    private void ReadSingleGlyphName(Face face, uint i)
    {
        var nameKey = FnvHash.HashString(face.GetGlyphName(i, 30));
        names[(int)nameKey] = (char)i;
    }
}
/*
public  class FreeTypeGlyphNameMap : IGlyphNameMap
{
    private readonly Dictionary<PdfName, char> glyphNameKeys = new();
    public FreeTypeGlyphNameMap(Face face)
    {
        if (face.HasGlyphNames) 
            ReadAllGlyphNames(face);
    }

    private void ReadAllGlyphNames(Face face)
    {
        for (uint i = 0; i < face.GlyphCount; i++)
        {
            ReadSingleGlyphName(face, i);
        }
    }

    private void ReadSingleGlyphName(Face face, uint i)
    {
        var nameKey = NameDirectory.Get(face.GetGlyphName(i, 30));
        glyphNameKeys[nameKey] = (char)i;
    }

    public char Map(PdfName input) => 
        glyphNameKeys.TryGetValue(input, out var glyph)  ||
            GlyphNameToUnicodeMap.AdobeGlyphList.TryMap(input, out glyph)? glyph : ExtractNumber(input.Bytes);

    private static char ExtractNumber(byte[] inputBytes)
    {
        var seq = new SequenceReader<byte>(new ReadOnlySequence<byte>(inputBytes));
        if (seq.Length < 2 || !seq.TryRead(out var prefix) || prefix != 'g') return '\0';
        WholeNumberParser.TryParsePositiveWholeNumber(ref seq, out int value, out byte _);
        return (char)value;
    }
}
*/