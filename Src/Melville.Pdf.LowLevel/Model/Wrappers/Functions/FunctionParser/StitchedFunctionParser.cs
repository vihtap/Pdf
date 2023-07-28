﻿using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;

internal static class StitchedFunctionParser
{
    public static async ValueTask<PdfFunction> ParseAsync(PdfValueDictionary source)
    {
        var domain = await source.ReadIntervalsAsync(KnownNames.DomainTName).CA();
        var encode = await source.ReadIntervalsAsync(KnownNames.EncodeTName).CA();
        var bounds = await (await source.GetAsync<PdfValueArray>(KnownNames.BoundsTName).CA()).CastAsync<double>().CA();
        var functionDecls =
            await (await source.GetAsync<PdfValueArray>(KnownNames.FunctionsTName).CA()).CastAsync<PdfValueDictionary>().CA();
        var functions = await CreateFunctionSegmentsAsync(functionDecls, domain[0], bounds, encode).CA();

        var range = await source.ReadOptionalRangesAsync(functions[0].NumberOfOutputs).CA();
        return new StitchedFunction(domain, range, functions);
    }
        
    private static async Task<StitchedFunctionSegment[]> CreateFunctionSegmentsAsync(
        PdfValueDictionary[] functionDecls, ClosedInterval domain, double[] bounds, 
        ClosedInterval[] encode)
    {
        var functions = new StitchedFunctionSegment[functionDecls.Length];
        for (int i = 0; i < functionDecls.Length; i++)
        {
            functions[i] = new StitchedFunctionSegment(
                SegmentDomain(bounds, i, domain),
                encode[i],
                await functionDecls[i].CreateFunctionAsync().CA());
        }

        return functions;
    }

    private static ClosedInterval SegmentDomain(double[] bounds, int i, ClosedInterval domain) =>
        new(
            i == 0 ? domain.MinValue : bounds[i-1],
            i >= bounds.Length ? domain.MaxValue : bounds[i]);
}