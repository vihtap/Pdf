﻿using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.LowLevel.Writers.Builder.Functions;

/// <summary>
/// This struct is a builder for Pdf PostScript style fuctions
/// </summary>
public readonly struct PostscriptFunctionBuilder
{
    private readonly List<ClosedInterval> domains = new ();
    private readonly List<ClosedInterval> ranges = new ();

    /// <summary>
    /// Create a PostscriptFunctionBuilder
    /// </summary>
    public PostscriptFunctionBuilder()
    {
    }

    /// <summary>
    /// Add an input argument to the function.
    /// </summary>
    /// <param name="domain">Domain for the given input.</param>
    public void AddArgument(ClosedInterval domain) => domains.Add(domain);
    
    /// <summary>
    /// Add an output to the function
    /// </summary>
    /// <param name="range">Range of possible values for this output.</param>
    public void AddOutput(ClosedInterval range) => ranges.Add(range);

    /// <summary>
    /// Create a PdfStream that defines this function.
    /// </summary>
    /// <param name="code">The PostScript code for the function.</param>
    /// <returns>A PdfDictionary that defines this function</returns>
    public PdfValueStream Create(string code) => Create(code, new ValueDictionaryBuilder());

    /// <summary>
    /// Create a PdfStream that defines this function.
    /// </summary>
    /// <param name="code">The PostScript code for the function.</param>
    /// <param name="members">A DictionaryBuilder to which the function declaration should be added/</param>
    /// <returns>A PdfDictionary that defines this function</returns>
    public PdfValueStream Create(string code, ValueDictionaryBuilder members) =>
        AddFunctionItems(members).AsStream(code);

    private ValueDictionaryBuilder AddFunctionItems(in ValueDictionaryBuilder builder) =>
        builder
            .WithItem(KnownNames.FunctionTypeTName, 4)
            .WithItem(KnownNames.DomainTName, domains.AsPdfArray())
            .WithItem(KnownNames.RangeTName, ranges.AsPdfArray());
}