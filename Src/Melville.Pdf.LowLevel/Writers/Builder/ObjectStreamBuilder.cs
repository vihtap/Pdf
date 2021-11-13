﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public class ObjectStreamBuilder
{
    private readonly List<PdfIndirectObject> members = new();
    
    public bool TryAddRef(PdfIndirectObject obj)
    {
        if (!IsLegalWrite(obj, obj.DirectValueAsync().GetAwaiter().GetResult())) return false;
        members.Add(obj);
        return true;
    }
    private bool IsLegalWrite(PdfIndirectObject pdfIndirectObject, PdfObject direcetValue) => 
        pdfIndirectObject.GenerationNumber == 0 && direcetValue is not PdfStream;

    public async ValueTask<PdfObject> CreateStream(DictionaryBuilder builder)
    {
        var writer = new ObjectStreamWriter();
        foreach (var member in members)
        {
            await  writer.TryAddRefAsync(member);
        }
        return await writer.Build(builder, members.Count);
    }
}