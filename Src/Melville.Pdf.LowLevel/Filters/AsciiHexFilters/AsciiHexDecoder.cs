﻿using System;
using System.Buffers;
using Melville.Parsing.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Pdf.LowLevel.Filters.AsciiHexFilters;

internal class AsciiHexDecoder : StringDecoderStreamFilterDefinition<HexStringDecoder, byte>
{
    public override int MinWriteSize => 1;
    protected override ReadOnlySpan<byte> TerminatorSequence() => ">"u8;
}