﻿using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.FontMappings;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.GraphicsStates;
using Melville.Pdf.Wpf.FakeUris;

namespace Melville.Pdf.Wpf;

public partial class WpfRenderTarget: RenderTargetBase<DrawingContext>, IRenderTarget, IFontWriteTarget<GeometryGroup>
{
    private readonly TempFontDirectory fontCache;
    public WpfRenderTarget(DrawingContext target, GraphicsStateStack state, PdfPage page, TempFontDirectory fontCache):
        base(target, state, page)
    {
        this.fontCache = fontCache;
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

    public void ClipToPath(bool evenOddRule)
    {
        if(shape is null) return;
        shape.ClipToPath(evenOddRule);
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

    #region Path Building

    private IDrawTarget? shape = null;

    [DelegateTo()]
    private IDrawTarget CreateShape() => shape ??= new WpfDrawTarget(Target, State);

    public void EndPath()
    {
        shape = null;
    }

    #endregion

    #region Bitmap rendering

    public async ValueTask RenderBitmap(IPdfBitmap bitmap)
    {
        Target.DrawImage(await BitmapToWpfBitmap(bitmap), new Rect(0, 0, 1, 1));
    }

    private static async Task<BitmapSource> BitmapToWpfBitmap(IPdfBitmap bitmap)
    {
        var ret = new WriteableBitmap(bitmap.Width, bitmap.Height, 96, 96, PixelFormats.Pbgra32, null);
        ret.Lock();
        try
        {
            await FillBitmap(bitmap, ret);
        }
        finally
        {
            ret.Unlock();
        }
        return ret;
    }

    private static unsafe ValueTask FillBitmap(IPdfBitmap bitmap, WriteableBitmap wb) => 
        bitmap.RenderPbgra((byte*)wb.BackBuffer.ToPointer());

    #endregion

    #region Text Rendering

    public async ValueTask SetFont(IFontMapping font, double size)
    {
        State.CurrentState().SetTypeface(
            await new RealizedFontFactory(font, fontCache, this).CreateRealizedFont(size));
    }

    public  void RenderCurrentString(GeometryGroup currentString, bool stroke, bool fill, bool clip)
    {
        InnerPathPaint(stroke, fill, currentString);
        if (clip)
        {
            Target.PushClip(currentString);
            IncrementSavePoints();
        }
    }

    private void InnerPathPaint(bool stroke, bool fill, Geometry pathToPaint) =>
        Target.DrawGeometry(
            fill ? State.Current().Brush() : null, 
            stroke ? State.Current().Pen() : null, 
            pathToPaint);

    #endregion
}