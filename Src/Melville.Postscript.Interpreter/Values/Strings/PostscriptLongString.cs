﻿using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Melville.INPC;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Postscript.Interpreter.Values;

internal sealed partial class 
    PostscriptLongString: PostscriptString, IPostscriptArray,IPostscriptTokenSource
{
    [FromConstructor]private readonly Memory<byte> value;

    internal override Span<byte> GetBytes(
        scoped in Int128 memento, scoped in Span<byte> scratch) =>
        value.Span;

    public override int GetHashCode()
    {
        var hc = new HashCode();
        hc.AddBytes(value.Span);
        return hc.ToHashCode();
    }

    protected override Memory<byte> ValueAsMemory(in Int128 memento) => value;

    public bool TryGet(in PostscriptValue indexOrKey, out PostscriptValue result) =>
        (indexOrKey.TryGet(out int index) && index >= 0 &&
         index < value.Length)
            ? PostscriptValueFactory.Create(value.Span[index]).AsTrueValue(out result)
            : default(PostscriptValue).AsFalseValue(out result);

    public void Put(in PostscriptValue indexOrKey, in PostscriptValue setValue)
    {
        value.Span[indexOrKey.Get<int>()] = (byte)setValue.Get<int>();
    }

    public int Length => value.Length;

    public PostscriptValue CopyFrom(PostscriptValue source, PostscriptValue target)
    {
        var sourceSpan = source.Get<StringSpanSource>().GetSpan(
            stackalloc byte[ShortStringLimit]);
        sourceSpan.CopyTo(value.Span);
        return (sourceSpan.Length == value.Length)
            ? target
            : PostscriptValueFactory.CreateLongString(value[..sourceSpan.Length],
                StringKind);
    }

    public ForAllCursor CreateForAllCursor()
    {
        var ret = new PostscriptValue[value.Length];
        for (int i = 0; i < value.Length; i++)
        {
            ret[i] = PostscriptValueFactory.Create(value.Span[i]);
        }

        return new ForAllCursor(ret, 1);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<PostscriptValue> GetEnumerator()
    {
        for (int i = 0; i < value.Length; i++)
        {
            yield return PostscriptValueFactory.Create(value.Span[i]);
        }
    }

    public IPostscriptValueStrategy<string> IntervalFrom(
        int beginningPosition, int length) =>
        new PostscriptLongString(StringKind, value.Slice(beginningPosition, length));

    public void InsertAt(int index, IPostscriptArray values) => 
        ((PostscriptLongString)values).value.Span.CopyTo(value.Span[index..]);

    public void DoAnchorSearch(OperandStack stack, PostscriptValue seek)
    {
        var seekSpan = seek.Get<StringSpanSource>().GetSpan(
            stackalloc byte[ShortStringLimit]);
        if (value.Length >= seekSpan.Length &&
            value.Span.Slice(0, seekSpan.Length).SequenceEqual(seekSpan))
        {
            stack.Pop();
            stack.Push(PostscriptValueFactory.CreateLongString(
                value[seekSpan.Length..], StringKind));
            stack.Push(PostscriptValueFactory.CreateLongString(
                value[..seekSpan.Length], StringKind));
            stack.Push(true);
        }
        else
        {
            stack.Push(false);
        }
    }

    public void DoSearch(OperandStack stack, PostscriptValue seek)
    {
        var seekSpan = seek.Get<StringSpanSource>().GetSpan(
            stackalloc byte[ShortStringLimit]);
        var index = value.Span.IndexOf(seekSpan);
        if (index >= 0)
        {
            stack.Pop();
            stack.Push(PostscriptValueFactory.CreateLongString(
                value[(index + seekSpan.Length)..], StringKind));
            stack.Push(PostscriptValueFactory.CreateLongString(
                value[index..(index + seekSpan.Length)], StringKind));
            stack.Push(PostscriptValueFactory.CreateLongString(
                value[..index], StringKind));
            stack.Push(true);

        }
        else
        {
            stack.Push(false);
        }
    }

    public void GetToken(OperandStack stack)
    {
        var wrapper = new MemoryWrapper(value);
        var tokenizer = new Tokenizer(wrapper);
        using var enumerator = tokenizer.Tokens().GetEnumerator();
        if (enumerator.MoveNext()) 
        {
            stack.Push(PostscriptValueFactory.CreateLongString(
                value[wrapper.BytesConsumed..], StringKind));
            stack.Push(enumerator.Current);
            stack.Push(true);
        }
        else
            stack.Push(false);
    }

}