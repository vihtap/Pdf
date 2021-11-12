﻿using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.ReferenceDocuments.LowLevel;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure;

public class S_7_5_8CrossReferenceStreams
{
    [Fact]
    public async Task GenerateAndParseFileWithReferenceStream()
    {
        var document = MinimalPdfParser.MinimalPdf(1, 7);
        var ms = new MultiBufferStream();
        var writer = new LowLevelDocumentWriter(PipeWriter.Create(ms), document.CreateDocument());
        await writer.WriteWithReferenceStream();
        var fileAsString = ms.CreateReader().ReadToArray().ExtendedAsciiString();
        Assert.DoesNotContain(fileAsString, "trailer");
        var doc = await (fileAsString).ParseDocumentAsync();
        Assert.NotNull(doc.TrailerDictionary);
        Assert.IsType<PdfStream>(doc.TrailerDictionary);
        var root = await doc.TrailerDictionary.GetAsync<PdfDictionary>(KnownNames.Root);
        var pages = await root.GetAsync<PdfDictionary>(KnownNames.Pages);
        var kids = await pages.GetAsync<PdfArray>(KnownNames.Kids);
        await Assert.Single(kids);
    }

}