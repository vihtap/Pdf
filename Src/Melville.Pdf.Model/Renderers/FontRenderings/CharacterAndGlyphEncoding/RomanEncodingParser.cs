﻿using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CharacterAndGlyphEncoding;

public static class RomanEncodingParser
{
    public static ValueTask<IByteToUnicodeMapping> InterpretEncodingValue(PdfObject? encoding) =>
        (encoding, encoding?.GetHashCode()) switch
        {
            (null,_)  => new (CharacterEncodings.Standard),
            (PdfName, KnownNameKeys.WinAnsiEncoding) => new(CharacterEncodings.WinAnsi),
            (PdfName, KnownNameKeys.StandardEncoding) => new(CharacterEncodings.Standard),
            (PdfName, KnownNameKeys.MacRomanEncoding) => new(CharacterEncodings.MacRoman),
            (PdfName, KnownNameKeys.PdfDocEncoding) => new(CharacterEncodings.Pdf),
            (PdfName, KnownNameKeys.MacExpertEncoding) => new(CharacterEncodings.MacExpert),
            (PdfDictionary dict, _) => ReadEncodingDictionary(dict),
            _ => throw new PdfParseException("Invalid encoding member on font.")
        };

    private static async ValueTask<IByteToUnicodeMapping> ReadEncodingDictionary(PdfDictionary dict)
    {
        var baseEncoding = dict.TryGetValue(KnownNames.BaseEncoding, out var baseTask)
            ? await InterpretEncodingValue(await baseTask.CA()).CA()
            : CharacterEncodings.Standard;
        return dict.TryGetValue(KnownNames.Differences, out var arrTask) &&
               (await arrTask.CA()) is PdfArray arr?
            await CustomFontEncodingFactory.Create(baseEncoding, arr).CA(): baseEncoding;
    }
}