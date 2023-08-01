﻿using System.IO;
using System.Threading.Tasks;
using Melville.CSJ2K;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;

internal class JpxToPdfAdapter: ICodecDefinition
{
    public ValueTask<Stream> EncodeOnReadStreamAsync(Stream data, PdfDirectValue parameters)
    {
        throw new System.NotSupportedException();
    }

    public ValueTask<Stream> DecodeOnReadStreamAsync(Stream input, PdfDirectValue parameters)
    {
        var independentImage = J2kReader.FromStream(input);
        return new(independentImage.NumberOfComponents == 1 ?
            new JPeg200GrayStream(independentImage):
            new JPeg200RgbStream(independentImage));
    }
}