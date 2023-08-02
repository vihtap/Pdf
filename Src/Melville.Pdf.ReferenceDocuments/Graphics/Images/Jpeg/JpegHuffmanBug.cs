﻿using Melville.Pdf.LowLevel.Filters.FilterProcessing;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Images.Jpeg;

public class JpegHuffmanBug: DisplayImageTest
{
    public JpegHuffmanBug() : base("Draw a jpeg which exposes a bug in the jpeg decoder.")
    {
    }
    
    protected override PdfStream CreateImage()
    {
        using var img = GetType().Assembly
            .GetManifestResourceStream("Melville.Pdf.ReferenceDocuments.Graphics.Images.Jpeg.JpegHuffmanBug.jpg");
        return new DictionaryBuilder()
            .WithItem(KnownNames.TypeTName, KnownNames.XObjectTName)
            .WithItem(KnownNames.SubtypeTName, KnownNames.ImageTName)
            .WithItem(KnownNames.ColorSpaceTName, KnownNames.DeviceGrayTName)
            .WithItem(KnownNames.WidthTName, 632)
            .WithItem(KnownNames.HeightTName, 279)
            .WithItem(KnownNames.BitsPerComponentTName, 8)
            .WithFilter(FilterName.DCTDecode)
            .AsStream(img!, StreamFormat.DiskRepresentation);
    }
}