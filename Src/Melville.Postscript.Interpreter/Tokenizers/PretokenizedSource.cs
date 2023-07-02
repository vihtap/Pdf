﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// A class that feeds pre-tokenized code into the postscript engine
/// </summary>
public partial class PretokenizedSource : ITokenSource
{
    /// <summary>
    /// The source of the tokens
    /// </summary>
    [FromConstructor] private IEnumerable<PostscriptValue> source;

    /// <inheritdoc/>
    public ICodeSource CodeSource => EmptyCodeSource.Instance;

    /// <inheritdoc/>
    public IEnumerable<PostscriptValue> Tokens() => source;

    /// <inheritdoc/>
    public IAsyncEnumerable<PostscriptValue> TokensAsync() =>
        source.ToAsyncEnumerable();
}