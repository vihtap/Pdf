﻿using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing;

public class DctToMonochromeFilter : IApplySingleFilter
{
    private readonly IApplySingleFilter innerFilter;
    private DctToMonochromeFilter(IApplySingleFilter innerFilter)
    {
        this.innerFilter = innerFilter;
    }

    public static async ValueTask<IApplySingleFilter> TryApply(
        IApplySingleFilter innerFilter, PdfStream stream)
    {
        if (stream.TryGetValue(KnownNames.ColorSpace, out var csTask) && await csTask.CA() is { } cs &&
            cs.GetHashCode() is KnownNameKeys.CalGray or KnownNameKeys.DefaultGray or KnownNameKeys.DeviceGray)
            return new DctToMonochromeFilter(innerFilter);
        return innerFilter;
    }

    public ValueTask<Stream> Encode(Stream source, PdfObject filter, PdfObject parameter)
    {
        throw new NotSupportedException("JPEG encoding is not supported");
    }

    public async ValueTask<Stream> Decode(Stream source, PdfObject filter, PdfObject parameter)
    {
        return filter == KnownNames.DCTDecode?
            (ReadingFilterStream.Wrap( await innerFilter.Decode(source, filter, parameter).CA(),
                EveryThirdByteFilter.Instance)):
            (source);
    }
}