﻿using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Melville.Pdf.Model.Renderers.Colors.Profiles;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors;

public class IccCMYK: ColorBars
{
    public IccCMYK() : base("Four different Colors from CMYK with an explicit icc profile")
    {
    }

    protected override void SetPageProperties(PageCreator page)
    {
        base.SetPageProperties(page);
        page.AddResourceObject(ResourceTypeName.ColorSpace, PdfDirectValue.CreateName("CS1"),
            cr =>
            {

                var strRef = cr.Add(new ValueDictionaryBuilder()
                    .WithItem(KnownNames.NTName, 4)
                    .AsStream(CmykIccProfile.GetCmykProfileStream()));
                return new PdfValueArray(KnownNames.ICCBasedTName, strRef);
            }
        );
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.SetLineWidth(15);

        //setting the colorspace should reset to black
        csw.SetStrokeColor(0.7);
        
        await csw.SetStrokingColorSpaceAsync(PdfDirectValue.CreateName("CS1"));
        DrawLine(csw);
        csw.SetStrokeColor(1,0,0,0);
        DrawLine(csw);
        csw.SetStrokeColor(0,1,0,0);
        DrawLine(csw);
        csw.SetStrokeColor(0,0,1,0);
        DrawLine(csw);
    }
}