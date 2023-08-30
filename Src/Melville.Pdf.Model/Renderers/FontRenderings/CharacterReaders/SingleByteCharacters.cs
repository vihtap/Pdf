﻿using System;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;

[StaticSingleton()]
internal sealed partial class SingleByteCharacters : IReadCharacter
{
    public Memory<uint> GetCharacters(
        in ReadOnlyMemory<byte> input, in Memory<uint> scratchBuffer, out int bytesConsumed)
    {
        bytesConsumed = 1;
        scratchBuffer.Span[0] = input.Span[0];
        return scratchBuffer[..1];
    }
}