﻿using System;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterAndGlyphEncoding;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Model.Renderers.FontRenderings.FreeType;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using SharpFont;

namespace Melville.Pdf.Model.Renderers.FontRenderings;

public readonly struct FontReader
{
    private readonly IDefaultFontMapper defaultMapper;

    public FontReader(IDefaultFontMapper defaultMapper)
    {
        this.defaultMapper = defaultMapper;
    }
    
    public ValueTask<IRealizedFont> NameToRealizedFont(PdfName name, FreeTypeFontFactory factory) =>
        defaultMapper.MapDefaultFont(name, factory);

    public  ValueTask<IRealizedFont> DictionaryToRealizedFont(PdfDictionary fontDict, double size) => 
         PdfFontToRealizedFont(size, new PdfFont(fontDict));

    private async ValueTask<IRealizedFont> PdfFontToRealizedFont(
        double size, PdfFont font, IGlyphMapping? externalMapping = null)
    {
        var fontTypeKey = (await font.SubTypeAsync().CA()).GetHashCode();

        return await (fontTypeKey switch
        {
            KnownNameKeys.Type3 => new Type3FontFactory(font.LowLevel, size).ParseAsync().CA(),
            KnownNameKeys.Type0 => CreateType0Font(font, size).CA(),
            _ => CreateRealizedFont(font, 
                new FreeTypeFontFactory(size, font) { GlyphMapping = externalMapping }).CA()
        });
    }


    private async ValueTask<IRealizedFont> CreateRealizedFont(PdfFont font, FreeTypeFontFactory factory) =>
        await (
                await font.EmbeddedStreamAsync().CA() is { } fontAsStream ?
                    factory.FromStream(fontAsStream) :
                    SystemFontByName(font, factory)
              ).CA();

    private async ValueTask<IRealizedFont> SystemFontByName(PdfFont font, FreeTypeFontFactory factory) =>
        await defaultMapper.MapDefaultFont(await font.OsFontNameAsync().CA(), factory)
            .CA();

    private async ValueTask<IRealizedFont> CreateType0Font(PdfFont font, double size)
    {
        var sub = await font.Type0SubFont().CA();
        var mapper = await ParseSubFontSytemInfo(
            await ParseType0Encoding(await font.EncodingAsync().CA()).CA(), sub).CA();
        return await PdfFontToRealizedFont(size, sub, mapper).CA();
    }

    private async ValueTask<IGlyphMapping> ParseSubFontSytemInfo(
        IGlyphMapping externalMapping, PdfFont font)
    {
        var info = await font.CidSystemInfo().CA();
        if (info == null ||
            await info.GetOrDefaultAsync(KnownNames.Supplement, 0) != 0 ||
            !(await info.GetOrDefaultAsync(KnownNames.Registry, PdfString.Empty).CA()).IsSameAS("Adobe") ||
            !(await info.GetOrDefaultAsync(KnownNames.Ordering, PdfString.Empty).CA()).IsSameAS("Identity"))
            throw new NotImplementedException("Only default CID Font Orderings are implemented. ");
        return externalMapping;
    }

    private async ValueTask<IGlyphMapping> ParseType0Encoding(PdfObject? encodingEntry)
    {
        if (encodingEntry != KnownNames.IdentityH && encodingEntry != KnownNames.IdentityV)
            throw new NotImplementedException("CMAP parsing is not implemented.");
        return IdentityCmapMapping.Instance;
    }
}