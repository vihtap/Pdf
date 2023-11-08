﻿using System.Xml.Linq;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.FormReader.Interface;
using Melville.Pdf.FormReader.XfaForms;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.FormReader.AcroForms;

internal partial class AcroPdfForm : IPdfForm
{
    [FromConstructor] public readonly PdfLowLevelDocument document;
    [FromConstructor] public IReadOnlyList<IPdfFormField> Fields { get; }
    [FromConstructor] private readonly PdfDirectObject formAppearanceString;
    [FromConstructor] private readonly PdfIndirectObject xfaDataSet;
    [FromConstructor] private readonly XfaSubForm xfaForm;

    public async ValueTask<PdfLowLevelDocument> CreateModifiedDocumentAsync()
    {
        var ret = new ModifyableLowLevelDocument(document );
        await WriteChangedFieldsAsync(ret).CA();
        if (!xfaDataSet.IsNull)
        {
            await WriteXfaDataset(ret).CA();
        }
        return ret;
    }

    private async ValueTask WriteChangedFieldsAsync(ICanReplaceObjects target)
    {
        foreach (var field in Fields.OfType<AcroFormField>())
        {
            await field.WriteChangeToAsync(target, formAppearanceString).CA();
        }
    }

    private static readonly XNamespace DataNs = "http://www.xfa.org/schema/xfa-data/1.0/";

    private async ValueTask WriteXfaDataset(ModifyableLowLevelDocument doc)
    {
        var targetXml = new XElement(DataNs + "datasets", 
            new XAttribute(XNamespace.Xmlns + "xfa", DataNs),
            new XElement(DataNs + "data", xfaForm.DataElements())
            );

        doc.ReplaceReferenceObject(xfaDataSet, 
            new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.EmbeddedFile)
                .WithFilter(FilterName.FlateDecode)
                .AsStream(targetXml.ToString().AsExtendedAsciiBytes())
            );
        
    }
}