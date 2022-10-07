﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings.FreeType.GlyphMappings;

public readonly partial struct CharacterToGlyphMapFactory
{
    [FromConstructor] private readonly Face face;
    [FromConstructor] private readonly PdfFont font;
    [FromConstructor] private readonly PdfEncoding encoding;

    public async ValueTask<IMapCharacterToGlyph> Parse() =>
        (await font.SubTypeAsync().CA()).GetHashCode() switch
        {
            KnownNameKeys.Type0 => await Type0CharMapping().CA(),
            KnownNameKeys.MMType1 => throw new NotImplementedException("MultiMaster fonts not implemented."),
            KnownNameKeys.Type1 => await SingleByteNamedMapping().CA(),
            KnownNameKeys.TrueType => await ParseTrueTypeMapping((await font.FontFlagsAsync().CA())).CA(),
            _ => throw new PdfParseException("Unknown Font Type"),

        };

    // this is a variance from the spec.  If a symbolic true type font lacks a valid CMAP for mapping
    // we fall back and attempt to map the font as a roman font.
    private ValueTask<IMapCharacterToGlyph> ParseTrueTypeMapping(FontFlags fontFlags) => 
        TryMapAsSymbolicFont(fontFlags) is {} ret? new(ret): SingleByteNamedMapping();

    private IMapCharacterToGlyph? TryMapAsSymbolicFont(FontFlags fontFlags) => 
        fontFlags.HasFlag(FontFlags.Symbolic) ? TrueTypeSymbolicMapping(): null;

    private IMapCharacterToGlyph? TrueTypeSymbolicMapping()
    {
        var charmap = ValidCharMap(face.CharMapByInts(1, 0)) ?? ValidCharMap(face.CharMapByInts(3, 0));
        if (charmap is null) return null;
        var ret = new uint[256];
        foreach (var (character, glyph) in charmap.AllMappings())
        {
            ret[character & 0xFF] = glyph;
        }

        return new FontRenderings.GlyphMappings.CharacterToGlyphArray(ret);
    }

    private CharMap? ValidCharMap(CharMap? input) => 
        input?.AllMappings().Any()??false?input:null;

    private async ValueTask<IMapCharacterToGlyph> SingleByteNamedMapping()
    {
        var array = new uint[256];
        var nameToGlyphMapper = new NameToGlyphMappingFactory(face).Create();
        await new FontRenderings.GlyphMappings.SingleByteEncodingParser(nameToGlyphMapper, array, await BuiltInFontCharMappings().CA())
            .WriteEncodingToArray(encoding.LowLevel).CA();
        WriteBackupMappings(array);
        return new FontRenderings.GlyphMappings.CharacterToGlyphArray(array);
    }

    private void WriteBackupMappings(uint[] array)
    {
        // this is nonstandard behavior to handle a small population of PDF files that appear to be malformed.
        // the idea is to scan all of the CMaps and if there are any mappings from single bytes to a character that
        // is currently unmapped (ie mapped to glyph 0) then we use the backup mapping.  Since undefined mappings cause
        // undefined behavior, this should not affect any valid files, but it will allow some malformed files to
        // be presented correctly.

        if (face.CharMaps is not { } charMaps) return;
        foreach (var map in charMaps)
        {
            foreach (var (character, glyph) in map.AllMappings())
            {
                if (character < 256 && array[character] == 0)
                {
                    array[character] = glyph;
                }
            }
        }
    }

    private async Task<byte[][]?> BuiltInFontCharMappings()
    {
        if ((await font.SubTypeAsync().CA()) != KnownNames.Type1) return null;
        return (await font.BaseFontNameAsync().CA()).GetHashCode() switch
        {
            KnownNameKeys.Symbol => CharacterEncodings.Symbol,
            KnownNameKeys.ZapfDingbats => CharacterEncodings.ZapfDingbats,
            _=> null
        };
    }
    
    private async ValueTask<IMapCharacterToGlyph> Type0CharMapping()
    {
        var subFont = await font.Type0SubFont().CA();
        return  (await subFont.CidToGidMapStream().CA() is {} mapStream)?
            await new CMapStreamParser(
                PipeReader.Create(await mapStream.StreamContentAsync().CA())).Parse().CA():
             IdentityCharacterToGlyph.Instance;
    }
}