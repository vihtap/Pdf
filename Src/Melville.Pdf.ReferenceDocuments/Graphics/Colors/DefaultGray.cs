﻿using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.Builder.Functions;
using Melville.Pdf.LowLevel.Writers.ContentStreams;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;
using PdfIndirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfIndirectValue;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors;

public class DefaultGray: ColorBars
{
    public DefaultGray() : base("Default Gray Colorspace")
    {
    }

    protected override void SetPageProperties(PageCreator page)
    {
        base.SetPageProperties(page);
        page.AddResourceObject(ResourceTypeName.ColorSpace, KnownNames.DefaultGrayTName,
            CreateColorSpace);
    }

    private PdfIndirectValue CreateColorSpace(IPdfObjectCreatorRegistry i)
    {
        var builder = new PostscriptFunctionBuilder();
        builder.AddArgument((0, 1));
        builder.AddOutput((0, 1));
        builder.AddOutput((0, 1));
        builder.AddOutput((0, 1));
        var func = i.Add(builder.Create("{dup 0}"));
        return new PdfValueArray(
            KnownNames.DeviceNTName, ColorantNames(), KnownNames.DeviceRGBTName, func);
    }

    protected virtual PdfValueArray ColorantNames()
    {
        return new PdfValueArray(
            PdfDirectValue.CreateName("khed")
        );
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.SetLineWidth(15);

        //setting the colorspace should reset to black
        csw.SetStrokeColor(0.7);
        
        await csw.SetStrokingColorSpaceAsync(KnownNames.DeviceGrayTName);
        DrawLine(csw);
        csw.SetStrokeColor(.25);
        DrawLine(csw);
        csw.SetStrokeColor(.5);
        DrawLine(csw);
        csw.SetStrokeColor(.75);
        DrawLine(csw);
    }
}