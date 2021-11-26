﻿using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Model.Renderers;

public partial class RenderEngine: IContentStreamOperations
{
    private readonly PdfPage page;
    private readonly IRenderTarget target;
    public RenderEngine(PdfPage page, IRenderTarget target)
    {
        this.page = page;
        this.target = target;
    }
    
    [DelegateTo]
    private ITextObjectOperations TextObject => throw new NotImplementedException("Text Object not implemented");
    [DelegateTo]
    private ITextBlockOperations TextBlock => throw new NotImplementedException("Text Block not implemented");
    [DelegateTo]
    private IMarkedContentCSOperations Marked => throw new NotImplementedException("Marked Operations not implemented");
    [DelegateTo]
    private ICompatibilityOperations Compat => 
        throw new NotImplementedException("Compatibility Operations not implemented");
    [DelegateTo]
    private IFontMetricsOperations FontMetrics => 
        throw new NotImplementedException("Compatibility Operations not implemented");

    #region Graphics State
    [DelegateTo] private IGraphiscState StateOps => target.GrapicsStateChange;
    
    public void SaveGraphicsState()
    {
        StateOps.SaveGraphicsState();
        target.SaveTransformAndClip();
    }

    public void RestoreGraphicsState()
    {
        StateOps.RestoreGraphicsState();
        target.RestoreTransformAndClip();
    }

    public void ModifyTransformMatrix(in System.Numerics.Matrix3x2 newTransform)
    {
        StateOps.ModifyTransformMatrix(in newTransform);
        target.Transform(newTransform);
    }


    public async ValueTask LoadGraphicStateDictionary(PdfName dictionaryName) =>
        await StateOps.LoadGraphicStateDictionary(
            await page.GetResourceObject(ResourceTypeName.ExtGState, dictionaryName) as 
                PdfDictionary ?? throw new PdfParseException($"Cannot find GraphicsState {dictionaryName}"));
    #endregion
    
    #region Drawing Operations

    private double firstX, firstY;
    private double lastX, lasty;

    private void SetLast(double x, double y) => (lastX, lasty) = (x, y);
    private void SetFirst(double x, double y) => (firstX, firstY) = (x, y);

    public void MoveTo(double x, double y)
    {
        target.MoveTo(x, y);
        SetLast(x,y);
        SetFirst(x,y);
    }

    public void LineTo(double x, double y)
    {
        target.LineTo(x, y);
        SetLast(x,y);
    }

    public void CurveTo(double control1X, double control1Y, double control2X, double control2Y, double finalX, double finalY)
    {
        target.CurveTo(control1X, control1Y, control2X, control2Y, finalX, finalY);
        SetLast(finalX, finalY);
    }

    public void CurveToWithoutInitialControl(double control2X, double control2Y, double finalX, double finalY)
    {
        target.CurveTo(lastX, lasty, control2X, control2Y, finalX, finalY);
        SetLast(finalX, finalY);
    }

    public void CurveToWithoutFinalControl(double control1X, double control1Y, double finalX, double finalY)
    {
        target.CurveTo(control1X, control1Y, finalX, finalY, finalX, finalY);
        SetLast(finalX, finalY);
    }

    public void ClosePath()
    {
        target.ClosePath();
        SetLast(firstX, firstY);
    }

    public void Rectangle(double x, double y, double width, double height)
    {
        target.MoveTo(x,y);
        target.LineTo(x+width,y);
        target.LineTo(x+width,y+height);
        target.LineTo(x,y+height);
        target.ClosePath();
    }

    public void EndPathWithNoOp() => target.EndPath();
    public void StrokePath() => PaintPath(true, false, false);
    public void CloseAndStrokePath() => CloseAndPaintPath(true, false, false);
    public void FillPath() => PaintPath(false, true, false);
    public void FillPathEvenOdd() => PaintPath(false, true, true);
    public void FillAndStrokePath() => PaintPath(true, true, false);
    public void FillAndStrokePathEvenOdd() => PaintPath(true, true, true);
    public void CloseFillAndStrokePath() => CloseAndPaintPath(true, true, false);
    public void CloseFillAndStrokePathEvenOdd() => CloseAndPaintPath(true, true, true);

    
    private void CloseAndPaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        ClosePath();
        PaintPath(stroke, fill, evenOddFillRule);
    }
    private void PaintPath(bool stroke, bool fill, bool evenOddFillRule)
    {
        target.PaintPath(stroke, fill, evenOddFillRule);
        EndPathWithNoOp();
    }

    public void ClipToPath() => target.CombineClip(false);

    public void ClipToPathEvenOdd() => target.CombineClip(true);

    public ValueTask DoAsync(PdfName name)
    {
        throw new NotImplementedException();
    }

    public ValueTask DoAsync(PdfStream inlineImage)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Color Implementation
    
    public async ValueTask SetStrokingColorSpace(PdfName colorSpace)
    {
        target.GrapicsStateChange.SetStrokeColorSpace(
            await ColorSpaceFactory.ParseColorSpace(colorSpace, page));
    }

    public async ValueTask SetNonstrokingColorSpace(PdfName colorSpace) =>
        target.GrapicsStateChange.SetNonstrokeColorSpace(
            await ColorSpaceFactory.ParseColorSpace(colorSpace, page));
    
    public ValueTask SetStrokeColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors)
    {
        if (patternName != null) throw new NotImplementedException("Patterns not implemented yet");
        SetStrokeColor(colors);
        return ValueTask.CompletedTask;
    }
    
    public ValueTask SetNonstrokingColorExtended(PdfName? patternName, in ReadOnlySpan<double> colors)
    {
        if (patternName != null) throw new NotImplementedException("Patterns not implemented yet");
        SetNonstrokingColor(colors);
        return ValueTask.CompletedTask;
    }

    public void SetStrokeGray(double grayLevel)
    {
        target.GrapicsStateChange.SetStrokeColorSpace(DeviceGray.Instance);
        Span<double> color = stackalloc double[] { grayLevel };
            SetStrokeColor(color);
    }

    public void SetStrokeRGB(double red, double green, double blue)
    {
        target.GrapicsStateChange.SetStrokeColorSpace(DeviceRgb.Instance);
        Span<double> color = stackalloc double[] { red, green, blue };
        SetStrokeColor(color);
    }

    public void SetStrokeCMYK(double cyan, double magenta, double yellow, double black)
    {
        target.GrapicsStateChange.SetStrokeColorSpace(DeviceCmyk.Instance);
        Span<double> color = stackalloc double[] { cyan, magenta, yellow, black };
        SetStrokeColor(color);
    }

    public void SetNonstrokingGray(double grayLevel)
    {
        target.GrapicsStateChange.SetNonstrokeColorSpace(DeviceGray.Instance);
        Span<double> color = stackalloc double[] { grayLevel };
        SetNonstrokingColor(color);
    }

    public void SetNonstrokingRGB(double red, double green, double blue)
    {
        target.GrapicsStateChange.SetNonstrokeColorSpace(DeviceRgb.Instance);
        Span<double> color = stackalloc double[] { red, green, blue };
        SetNonstrokingColor(color);
    }

    public void SetNonstrokingCMYK(double cyan, double magenta, double yellow, double black)
    {
        target.GrapicsStateChange.SetNonstrokeColorSpace(DeviceCmyk.Instance);
        Span<double> color = stackalloc double[] { cyan, magenta, yellow, black };
        SetNonstrokingColor(color);
    }
    #endregion
}