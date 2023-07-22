﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Model.Objects2;


public readonly partial struct PdfIndirectValue
{
    /// <summary>
    /// Strategy object that defines the type of the PdfObject
    /// </summary>
    [FromConstructor] private readonly object? valueStrategy;
    
    /// <summary>
    /// A memento that allows most PdfObjects to be represented without allocations
    /// </summary>
    [FromConstructor] public readonly MementoUnion Memento { get; }

    private object NonNullValueStrategy() => (valueStrategy ?? PostscriptNull.Instance);

    internal PdfIndirectValue(IIndirectValueSource src, long objNum, long generation) :
        this(src, MementoUnion.CreateFrom(objNum, generation)){}

    public bool IsNull => TryGetEmbeddedDirectValue(out var dirVal) && dirVal.IsNull;

    #region Getter

    public ValueTask<PdfDirectValue> LoadValueAsync() =>
        valueStrategy is IIndirectValueSource source?
            source.Lookup(Memento):
        new(CreateDirectValueUnsafe());

    private PdfDirectValue CreateDirectValueUnsafe()
    {
        Debug.Assert(IsEmbeddedDirectValue());
        return new PdfDirectValue(valueStrategy, Memento);
    }

    public bool IsEmbeddedDirectValue() => valueStrategy is not IIndirectValueSource;

    public bool TryGetEmbeddedDirectValue(out PdfDirectValue value) =>
        valueStrategy is IIndirectValueSource
            ? ((PdfDirectValue)default).AsFalseValue(out value)
            : CreateDirectValueUnsafe().AsTrueValue(out value);
    public bool TryGetEmbeddedDirectValue<T>(out T value)
    {
        value = default;
        return TryGetEmbeddedDirectValue(out PdfDirectValue dv) &&
               dv.TryGet(out value);
    }

    #endregion

    #region Implicit Operators

    public static implicit operator PdfIndirectValue(bool value) => (PdfDirectValue)value;
    public static implicit operator PdfIndirectValue(int value) => (PdfDirectValue)value;
    public static implicit operator PdfIndirectValue(long value) => (PdfDirectValue)value;
    public static implicit operator PdfIndirectValue(double value) => (PdfDirectValue)value;
    public static implicit operator PdfIndirectValue(string value) => (PdfDirectValue)value;
    public static implicit operator PdfIndirectValue(PdfValueArray value) => (PdfDirectValue)value;
    public static implicit operator PdfIndirectValue(PdfValueDictionary value) => (PdfDirectValue)value;
    public static implicit operator PdfIndirectValue(in ReadOnlySpan<byte> value) => 
        (PdfDirectValue)value;

    public override string ToString() => NonNullValueStrategy() switch
    {
        IPostscriptValueStrategy<string> vs => vs.GetValue(Memento),
        _=> valueStrategy.ToString()
    };

    #endregion
}

public static class IndirectValueOperations
{
    public static async ValueTask<T> LoadValueAsync<T>(this PdfIndirectValue value) =>
        (await value.LoadValueAsync().CA()).Get<T>();
}