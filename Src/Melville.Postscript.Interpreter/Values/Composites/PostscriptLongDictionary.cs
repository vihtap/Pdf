﻿using System;
using System.Collections.Generic;
using System.Text;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values;

internal partial class PostscriptLongDictionary :
    IPostscriptValueStrategy<string>,
    IPostscriptValueStrategy<IPostscriptComposite>,
    IPostscriptComposite
{
    [FromConstructor] private readonly Dictionary<PostscriptValue, PostscriptValue> items;

    public PostscriptLongDictionary() : this(new Dictionary<PostscriptValue, PostscriptValue>())
    {
    }

    string IPostscriptValueStrategy<string>.GetValue(in Int128 memento)
    {
        var ret = new StringBuilder();
        ret.AppendLine("<<");
        foreach (var pair in items)
        {
            ret.AppendLine($"    {pair.Key.Get<string>()}: {pair.Value.Get<string>()}");
        }
        ret.Append(">>");

        return ret.ToString();
    }

    IPostscriptComposite
        IPostscriptValueStrategy<IPostscriptComposite>.GetValue(in Int128 memento) => this;

    public bool TryGet(in PostscriptValue indexOrKey, out PostscriptValue result) =>
        items.TryGetValue(indexOrKey, out result);

    public void Add(in PostscriptValue indexOrKey, in PostscriptValue value) =>
        items[indexOrKey] = value;
}