﻿using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Microsoft.CodeAnalysis.CSharp;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Patterns;

public class ColoredTilePattern: Card3x5
{
    public ColoredTilePattern() : base("Spec example of a colored tile pattern")
    {
    }

    protected override ValueTask SetPagePropertiesAsync(PageCreator page)
    {
        return base.SetPagePropertiesAsync(page);
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        csw.SetNonstrokingRGB(1.0, 1.0, 0.0);
        csw.Rectangle(25, 175, 175, -150);
        csw.FillPath();
        
        csw.MoveTo(99.92, 49.92);
        csw.CurveTo(99.92, 77.52, 77.52, 99.91, 49.92, 99.92);
        csw.CurveTo(22.32, 99.92, -0.08, 77.52, -0.08, 49.92);
        csw.CurveTo(-0.08, 22.32, 22.32, -0.08, 49.92, -0.08);
        csw.CurveTo(77.52, -0.08, 99.92, 22.32, 99.92, 49.92);
        csw.FillAndStrokePath();
        
        csw.MoveTo(224.96, 49.92);
        csw.CurveTo(224.96, 77.52, 202.56, 99.92, 174.96, 99.92);
        csw.CurveTo(147.26, 99.92, 124.96, 77.52, 124.96, 49.92);
        csw.CurveTo(124.96, 22.32, 147.36, -0.08, 174.96, -0.08);
        csw.CurveTo(202.56, -0.08, 224.96, 22.32, 224.96, 49.92);
        csw.CloseFillAndStrokePath();
        
        csw.MoveTo(87.56, 201.70);
        csw.CurveTo(63.66, 187.90, 55.46, 157.32, 69.26, 133.40);
        csw.CurveTo(83.06, 109.50, 113.66, 101.30, 137.56, 115.10);
        csw.CurveTo(161.46, 128.90, 169.66, 159.50, 155.86, 183.40);
        csw.CurveTo(142.06, 207.30, 111.46, 215.50, 87.56, 201.70);
        csw.FillAndStrokePath();
        
        csw.MoveTo(50,50);
        csw.LineTo(175, 50);
        csw.LineTo(112.5, 158.253);
        csw.CloseFillAndStrokePath();
    }
}