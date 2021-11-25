﻿using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using SkiaSharp;

public class SkiaRenderTarget:RenderTargetBase<SKCanvas>, IRenderTarget
{
    public SkiaRenderTarget(SKCanvas target, GraphicsStateStack state, PdfPage page) : 
        base(target, state, page)
    {
    }

    public void SetBackgroundRect(PdfRect rect, int width, int height)
    {
        Target.Clear(SKColors.White);
        MapUserSpaceToBitmapSpace(rect, width, height);
    }

    
    #region Path Building

    private SKPath? currentPath;
    private SKPath GetOrCreatePath => currentPath ??= new SKPath();

    void IRenderTarget.MoveTo(double x, double y) => GetOrCreatePath.MoveTo((float)x,(float)y);

    void IRenderTarget.LineTo(double x, double y) => currentPath?.LineTo((float)x, (float)y);

    void IRenderTarget.ClosePath()
    {
        currentPath?.Close();
    }

    void IRenderTarget.CurveTo(double control1X, double control1Y, double control2X, double control2Y,
        double finalX, double finalY) =>
        currentPath?.CubicTo(
            (float)control1X, (float)control1Y, (float)control2X, (float)control2Y, (float)finalX, (float)finalY);

    #endregion

    #region PathDrawing
    void IRenderTarget.PaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        Target.SetMatrix(State.Current().Transform());
        if (fill)
        {
            SetCurrentFillRule(evenOddFillRule); 
            Target.DrawPath(GetOrCreatePath, State.Current().Brush());
        }
        if (stroke)
        {
            Target.DrawPath(GetOrCreatePath, State.Current().Pen());
        }
    }

    private void SetCurrentFillRule(bool evenOddFillRule) => 
        GetOrCreatePath.FillType = evenOddFillRule ? SKPathFillType.EvenOdd : SKPathFillType.Winding;

    void IRenderTarget.EndPath() => currentPath = null;

    #endregion
}