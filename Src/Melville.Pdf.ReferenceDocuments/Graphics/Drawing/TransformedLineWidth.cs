﻿using System.Numerics;
using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Drawing;

public class MoveToIsRequired : Card3x5
{
    public MoveToIsRequired(): base("Line to points prior to a first MoveTo do not render."){}

    protected override void DoPainting(ContentStreamWriter csw)
    {
        csw.SetLineWidth(10);
        csw.LineTo(100,100);
        csw.StrokePath();
        
        csw.MoveTo(100,0);
        csw.LineTo(200,100);
        csw.StrokePath();
    }
}
public class TransformedLineWidth : Card3x5
{
    public TransformedLineWidth() : base("LineWidth varies with angle in transformed rectangle.")
    {
        
    }

    protected override void DoPainting(ContentStreamWriter csw)
    {
        csw.SetLineWidth(10);
        csw.SetNonstrokingRGB(1,0,0);

        csw.ModifyTransformMatrix(Matrix3x2.CreateTranslation(25,25));
        csw.ModifyTransformMatrix(Matrix3x2.CreateScale(4,1));
        csw.MoveTo(0,0);
        csw.LineTo(60,0);
        csw.LineTo(60, 150);
        csw.CloseFillAndStrokePath();
    }
}