﻿using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Wpf;

public partial class WpfRenderTarget: RenderTargetBase<DrawingContext>, IRenderTarget
{
    public WpfRenderTarget(DrawingContext target, GraphicsStateStack state, PdfPage page):
        base(target, state, page)
    {
        SaveTransformAndClip();
    }

    #region Path and transform state

    private Stack<int> savePoints = new();
    public void SaveTransformAndClip()
    {
        savePoints.Push(0);
    }

    public void RestoreTransformAndClip()
    {
        var pops = savePoints.Pop();
        for (int i = 0; i < pops; i++)
        {
            Target.Pop();
        }
    }

    public override void Transform(in Matrix3x2 newTransform)
    {
        IncrementSavePoints();
        Target.PushTransform(newTransform.WpfTransform());
   }

    private void IncrementSavePoints()
    {
        savePoints.Push(1+savePoints.Pop());
    }

    public override void ClipToPath(bool evenOddRule)
    {
        if(currentShape is null) return;
        currentShape.ClipToPath(evenOddRule);
        IncrementSavePoints();
    }

    #endregion

    public void SetBackgroundRect(PdfRect rect)
    {
        var clipRectangle = new Rect(0,0, rect.Width, rect.Height);
        Target.DrawRectangle(Brushes.White, null, clipRectangle);
        Target.PushClip(new RectangleGeometry(clipRectangle));
        // setup the userSpace to device space transform
        MapUserSpaceToBitmapSpace(rect, rect.Width, rect.Height);
    }
    
    public override IDrawTarget CreateDrawTarget() => new WpfDrawTarget(Target, State);
    
    public async ValueTask RenderBitmap(IPdfBitmap bitmap) => Target.DrawImage(await bitmap.ToWbfBitmap(), new Rect(0, 0, 1, 1));
}