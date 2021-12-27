﻿using System;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.ContentValueStreamUnions;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Model.Renderers;

public partial class RenderEngine<TTypeface>: IContentStreamOperations
{
    private readonly IHasPageAttributes page;
    private readonly IRenderTarget<TTypeface> target;
    public RenderEngine(IHasPageAttributes page, IRenderTarget<TTypeface> target)
    {
        this.page = page;
        this.target = target;
    }
    
    [DelegateTo]
    private IMarkedContentCSOperations Marked => throw new NotImplementedException("Marked Operations not implemented");
    [DelegateTo]
    private ICompatibilityOperations Compat => 
        throw new NotImplementedException("Compatibility Operations not implemented");
    [DelegateTo]
    private IFontMetricsOperations FontMetrics => 
        throw new NotImplementedException("Compatibility Operations not implemented");

    #region Graphics State
    [DelegateTo] private IGraphiscState<TTypeface> StateOps => target.GrapicsStateChange;
    
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
            await page.GetResourceAsync(ResourceTypeName.ExtGState, dictionaryName) as 
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

    public async ValueTask DoAsync(PdfName name) =>
        await DoAsync((await page.GetResourceAsync(ResourceTypeName.XObject, name)) as PdfStream ??
                      throw new PdfParseException("Co command can only be called on Streams"));

    public async ValueTask DoAsync(PdfStream inlineImage)
    {
        switch ((await inlineImage.GetAsync<PdfName>(KnownNames.Subtype)).GetHashCode())
        {
            case KnownNameKeys.Image:
                await target.RenderBitmap(
                    await inlineImage.WrapForRenderingAsync(page, StateOps.CurrentState().NonstrokeColor));
                break;
            case KnownNameKeys.Form:
                await RunTargetGroup(inlineImage);
                break;
            default: throw new PdfParseException("Cannot do the provided object");
        }
    }

    private async ValueTask RunTargetGroup(PdfStream formXObject)
    {
        SaveGraphicsState();
        if(await formXObject.GetOrDefaultAsync<PdfObject>(KnownNames.Matrix, PdfTokenValues.Null) is PdfArray arr &&
           (await arr.AsDoublesAsync()) is {} matrix)
            ModifyTransformMatrix(CreateMatrix(matrix));
        
        if ((await formXObject.GetOrDefaultAsync<PdfObject>(KnownNames.BBox, PdfTokenValues.Null)) is PdfArray arr2 &&
            (await arr2.AsDoublesAsync()) is { } bbArray)
        {
            Rectangle(bbArray[0], bbArray[1], bbArray[2], bbArray[3]);
            ClipToPath();
            EndPathWithNoOp();
        }
        await new PdfFormXObject(formXObject, page).RenderTo(target);
        RestoreGraphicsState();
    }

    private static Matrix3x2 CreateMatrix(double[] matrix) =>
        new(
            (float)matrix[0],
            (float)matrix[1],
            (float)matrix[2],
            (float)matrix[3],
            (float)matrix[4],
            (float)matrix[5]
        );

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

    public async ValueTask SetStrokeGray(double grayLevel)
    {
        await SetStrokingColorSpace(KnownNames.DeviceGray);
        SetStrokeColor(stackalloc double[] { grayLevel });
    }

    public async ValueTask SetStrokeRGB(double red, double green, double blue)
    {
        await SetStrokingColorSpace(KnownNames.DeviceRGB);
        SetStrokeColor(stackalloc double[] { red, green, blue });
    }

    public async ValueTask SetStrokeCMYK(double cyan, double magenta, double yellow, double black)
    {
        await SetStrokingColorSpace(KnownNames.DeviceCMYK);
        SetStrokeColor(stackalloc double[] { cyan, magenta, yellow, black });
    }

    public async ValueTask SetNonstrokingGray(double grayLevel)
    {
        await SetNonstrokingColorSpace(KnownNames.DeviceGray);
        SetNonstrokingColor(stackalloc double[] { grayLevel });
    }

    public async ValueTask SetNonstrokingRGB(double red, double green, double blue)
    {
        await SetNonstrokingColorSpace(KnownNames.DeviceRGB);
        SetNonstrokingColor(stackalloc double[] { red, green, blue });
    }

    public async ValueTask SetNonstrokingCMYK(double cyan, double magenta, double yellow, double black)
    {
        await SetNonstrokingColorSpace(KnownNames.DeviceCMYK);
        SetNonstrokingColor(stackalloc double[] { cyan, magenta, yellow, black });
    }
    #endregion

    #region Text Operations

    public void BeginTextObject()
    {
        StateOps.SetBothTextMatrices(Matrix3x2.Identity);
    }

    public void EndTextObject()
    {
    }

    public void MovePositionBy(double x, double y) =>
        StateOps.SetBothTextMatrices(
            Matrix3x2.CreateTranslation((float)x,(float)y) 
            * StateOps.CurrentState().TextLineMatrix);

    public void MovePositionByWithLeading(double x, double y)
    {
        StateOps.SetTextLeading(-y);
        MovePositionBy(x,y);
    }

    public void SetTextMatrix(double a, double b, double c, double d, double e, double f) =>
        StateOps.SetBothTextMatrices(new Matrix3x2(
            (float)a,(float)b,(float)c,(float)d,(float)e,(float)f));

    public void MoveToNextTextLine() => 
        MovePositionBy(0, - StateOps.CurrentState().TextLeading);

    public void ShowString(in ReadOnlyMemory<byte> decodedString)
    {
        foreach (var character in decodedString.Span)
        {
            var (w, h) = target.RenderGlyph(MapToUnicodwCodePoint(character));
            UpdateTextPosition(w, h);
        }
    }

    private static char MapToUnicodwCodePoint(byte character)
    {
        return (Char)character;
    }

    private void UpdateTextPosition(double width, double height)
    { 
        StateOps.CurrentState().SetTextMatrix(
            NextCharPositionAfterWrite(width, height)*
            StateOps.CurrentState().TextMatrix
        );
    }

    private Matrix3x2 NextCharPositionAfterWrite(double width, double height) =>
        StateOps.CurrentState().WritingMode == WritingMode.TopToBottom
            ? Matrix3x2.CreateTranslation(0f, (float)-height)
            : Matrix3x2.CreateTranslation((float)width, 0.0f);

    public void MoveToNextLineAndShowString(in ReadOnlyMemory<byte> decodedString)
    {
        MoveToNextTextLine();
        ShowString(decodedString);
    }

    public void MoveToNextLineAndShowString(double wordSpace, double charSpace, in ReadOnlyMemory<byte> decodedString)
    {
        SetWordSpace(wordSpace);
        SetCharSpace(charSpace);
        MoveToNextLineAndShowString(decodedString);
    }

    public void ShowSpacedString(in Span<ContentStreamValueUnion> values)
    {
        foreach (var value in values)
        {
            switch (value.Type)
            {
                case ContentStreamValueType.Number:
                    var delta = value.Floating / 1000;
                    UpdateTextPosition(-delta, delta);
                    break;
                case ContentStreamValueType.Memory:
                    ShowString(value.Bytes);
                    break;
                default:
                    throw new PdfParseException("Invalid ShowSpacedString argument");
            }
        }
    }

    #endregion
}