﻿using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;

internal static class PostscriptFunctionParser
{
    public static async Task<PdfFunction> ParseAsync(PdfValueStream source)
    {
        var domain = await source.ReadIntervalsAsync(KnownNames.DomainTName).CA();
        var range = await source.ReadIntervalsAsync(KnownNames.RangeTName).CA();

        var interp = SharedPostscriptParser.BasicPostscriptEngine();
        await interp.ExecuteAsync(await source.StreamContentAsync().CA()).CA();

        var ops = interp.OperandStack.Pop().Get<IPostscriptArray>();

        return new PostscriptFunction(domain, range, ops);
    }
}