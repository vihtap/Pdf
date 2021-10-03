﻿using System;
using System.IO;
using Melville.INPC;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.Decryptors
{
    public interface IWrapReaderForDecryption
    {
        IParsingReader Wrap(IParsingReader reader, int objectNumber, int generationNumber);
    }
}