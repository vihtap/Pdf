﻿using System;
using System.IO;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.JpegLibrary.PipeAmdStreamAdapters;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.JpegFilter;

public class DctDecoder : ICodecDefinition
{
    public ValueTask<Stream> EncodeOnReadStream(Stream data, PdfObject? parameters)
    {
        throw new NotSupportedException();
    }

    public async ValueTask<Stream> DecodeOnReadStream(Stream input, PdfObject parameters) => 
        await new JpegStreamFactory( await new DctDecodeParameters(parameters).ColorTransformAsync().CA())
            .FromStream(input).CA();

}