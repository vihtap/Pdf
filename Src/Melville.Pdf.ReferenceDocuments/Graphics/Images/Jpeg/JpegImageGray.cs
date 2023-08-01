﻿using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images.Jpeg;

public class JpegImageGray: DisplayImageTest
{
    public JpegImageGray() : base("Draw a simple, grayscale JPEG image")
    {
    }
    
    protected override PdfValueStream CreateImage()
    {
        using var img = GetType().Assembly
            .GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Graphics.Images.JPEGGray.jpg");
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.ImageTName)
            .WithItem(KnownNames.ColorSpaceTName, KnownNames.DeviceGrayTName)
            .WithItem(KnownNames.WidthTName, 256)
            .WithItem(KnownNames.HeightTName, 256)
            .WithItem(KnownNames.BitsPerComponentTName, 8)
            .WithFilter(FilterName.DCTDecode)
            .AsStream(img!, StreamFormat.DiskRepresentation);
    }
}