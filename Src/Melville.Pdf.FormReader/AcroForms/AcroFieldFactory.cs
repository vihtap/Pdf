﻿using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.FormReader.AcroForms;

internal static class AcroFieldFactory
{
    public static async ValueTask<IReadOnlyList<IPdfFormField>> ParseFieldsAsync(IReadOnlyList<PdfIndirectObject> fieldReferences)
    {
        var ret = new List<IPdfFormField>(fieldReferences.Count);
        foreach (var reference in fieldReferences)
        {
            var field = await reference.LoadValueAsync();
            if (field.TryGet(out PdfDictionary? dict))
            {
                await ParseSingleField(dict, reference, ret);
            }
        }
        return ret;
    }

    private static async Task ParseSingleField(PdfDictionary dict, PdfIndirectObject reference, List<IPdfFormField> ret)
    {
        var name = await dict.GetAsync<string>(KnownNames.T);
        var value = await dict.GetOrDefaultAsync(KnownNames.V);
        var type = await dict[KnownNames.FT];
        var flags = (AcroFieldFlags)(await dict.GetOrDefaultAsync(KnownNames.Ff, 0).CA());

        await new FieldBuilder(name, value, type, flags, dict, reference, ret)
            .CreateAsync().CA();
    }
}