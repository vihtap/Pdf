﻿using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Melville.Pdf.ReferenceDocuments.Graphics.Images;

namespace Melville.Pdf.ReferenceDocuments.PageProperties.OptionalContent;

public class HideOptionalImageOn : HideOptionalImageOff
{
    public HideOptionalImageOn():base("Show Optional Image and content")
    {
    }

    protected override PdfDirectObject OnOrOff() => KnownNames.ONTName;
}
public class HideOptionalImageOff: DisplayImageTest
{
    public HideOptionalImageOff() : this("Hide optional image and content")
    {
    }

    public HideOptionalImageOff(string helpText) : base(helpText)
    { }

    private PdfIndirectObject ocg;

    protected override ValueTask AddContentToDocumentAsync(PdfDocumentCreator docCreator)
    {
        var usageDictionary = new DictionaryBuilder()
            .WithItem(KnownNames.CreatorInfoTName, new DictionaryBuilder().WithItem(KnownNames.CreatorTName,"JDM").AsDictionary())
            .AsDictionary();

        ocg = docCreator.LowLevelCreator.Add(new DictionaryBuilder().WithItem(KnownNames.NameTName, "OptionalGroup")
            .WithItem(KnownNames.TypeTName, KnownNames.OCGTName)
            .WithItem(KnownNames.IntentTName, new PdfArray(KnownNames.ViewTName, KnownNames.DesignTName))
            .WithItem(KnownNames.UsageTName, usageDictionary)
            .AsDictionary());

        docCreator.AddToRootDictionary(KnownNames.OCPropertiesTName, new DictionaryBuilder()
            .WithItem(KnownNames.OCGsTName, new PdfArray(ocg))
            .WithItem(KnownNames.DTName, new DictionaryBuilder()
                .WithItem(OnOrOff(), new PdfArray(ocg))
                .AsDictionary())
            .AsDictionary()
        );
        return base.AddContentToDocumentAsync(docCreator);
    }

    protected virtual PdfDirectObject OnOrOff() => KnownNames.OFFTName;

    protected override void SetPageProperties(PageCreator page)
    {

        page.AddResourceObject(ResourceTypeName.Properties, PdfDirectObject.CreateName("OCLayer"),
            ocg!
        );
        base.SetPageProperties(page);
    }
    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        using (await csw.BeginMarkedRangeAsync(KnownNames.OCTName, PdfDirectObject.CreateName("OCLayer")))
        {
            csw.MoveTo(0,0);
            csw.LineTo(300,300);
            csw.StrokePath();
        }
        await base.DoPaintingAsync(csw);
    }

    protected override PdfStream CreateImage()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.ImageTName)
            .WithItem(KnownNames.ColorSpaceTName, KnownNames.DeviceRGBTName)
            .WithItem(KnownNames.WidthTName, 256)
            .WithItem(KnownNames.HeightTName, 256)
            .WithItem(KnownNames.BitsPerComponentTName, 8)
            .WithItem(KnownNames.OCTName, ocg)
            .AsStream(GenerateImage());
    }

    private byte[] GenerateImage()
    {
        var ret = new byte[256 * 256 * 3];
        var pos = 0;
        for (int i = 0; i < 256; i++)
        for (int j = 0; j < 256; j++)
        {
            ret[pos++] = (byte)i;
            ret[pos++] = (byte)j;
            ret[pos++] = 0;
        }
        return ret;
    }
}