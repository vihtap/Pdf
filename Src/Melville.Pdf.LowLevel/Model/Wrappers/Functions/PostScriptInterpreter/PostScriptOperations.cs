﻿using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter
{
    public static partial class PostScriptOperationsDict
    {
        private static readonly IReadOnlyDictionary<uint, IPostScriptOperation> operationsDict = CreateDict();

        private static void AddPdfOperation(
            Dictionary<uint, IPostScriptOperation> dict, string name, IPostScriptOperation op)
        {
              dict.Add(FnvHash.HasStringAsLowerCase(name), op);  
        }

        public static IPostScriptOperation GetOperation(uint hash)
        {
            if (operationsDict.TryGetValue(hash, out var ret)) return ret;
            throw new PdfParseException("Unknown postscript operator");
        }
    }
}