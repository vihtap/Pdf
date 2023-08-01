﻿using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.Builder.Functions;
using Melville.Pdf.LowLevel.Writers.ContentStreams;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;
using PdfIndirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfIndirectValue;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors;

public class DefaultCmyk: ColorBars
{
    public DefaultCmyk() : base("Default CMYK colorspace")
    {
    }

    protected override void SetPageProperties(PageCreator page)
    {
        base.SetPageProperties(page);
        page.AddResourceObject(ResourceTypeName.ColorSpace, KnownNames.DefaultCMYKTName,
            CreateColorSpace);
    }

    private PdfIndirectValue CreateColorSpace(IPdfObjectCreatorRegistry i)
    {
        var builder = new PostscriptFunctionBuilder();
        builder.AddArgument((0, 1));
        builder.AddArgument((0, 1));
        builder.AddArgument((0, 1));
        builder.AddArgument((0, 1));
        builder.AddOutput((0, 1));
        builder.AddOutput((0, 1));
        builder.AddOutput((0, 1));
        var func = i.Add(builder.Create("{pop 0.2 mul exch 0.4 mul add exch 0.2 mul add dup dup}"));
        return new PdfValueArray(
            KnownNames.DeviceNTName, ColorantNames(), KnownNames.DeviceRGBTName, func);
    }

    protected virtual PdfValueArray ColorantNames()
    {
        return new PdfValueArray(
            PdfDirectValue.CreateName("khed"),
            PdfDirectValue.CreateName("QGR"),
            PdfDirectValue.CreateName("DFS"),
            PdfDirectValue.CreateName("DFS")
        );
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.SetLineWidth(15);

        //setting the colorspace should reset to black
        csw.SetStrokeColor(0.7);
        
        await csw.SetStrokingColorSpaceAsync(KnownNames.DeviceCMYKTName);
        DrawLine(csw);
        csw.SetStrokeColor(1, 0, 0, .25);
        DrawLine(csw);
        csw.SetStrokeColor(0,1,0, .25);
        DrawLine(csw);
        csw.SetStrokeColor(0, 0, 1, .5);
        DrawLine(csw);
    }
}