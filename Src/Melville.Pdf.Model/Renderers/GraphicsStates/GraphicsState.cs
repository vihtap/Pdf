﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
// needed in rendering code.
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

public interface IGraphiscState : IStateChangingOperations
{
    ValueTask LoadGraphicStateDictionary(PdfDictionary dictionary);
    void SetStrokeColorSpace(IColorSpace colorSpace);
    void SetNonstrokeColorSpace(IColorSpace colorSpace);
    GraphicsState CurrentState();
    void SetTextMatrix(in Matrix3x2 value);
    void SetTextLineMatrix(in Matrix3x2 value);
    void SetBothTextMatrices(in Matrix3x2 value);
    
}

public enum WritingMode
{
    LeftToRight = 0,
    TopToBottom = 1
}

[MacroItem("T", "StrokeBrush", "default")]
[MacroItem("T", "NonstrokeBrush", "default")]
// code
[MacroCode("public ~0~ ~1~ {get; private set;} = ~2~;")]
[MacroCode("    ~1~ = ((GraphicsState<T>)other).~1~;", Prefix = "public override void CopyFrom(GraphicsState other){ base.CopyFrom(other);", Postfix = "}")]
public abstract partial class GraphicsState<T> : GraphicsState
{
    protected override void StrokeColorChanged() => StrokeBrush = CreateSolidBrush(StrokeColor);
    protected override void NonstrokeColorChanged() => NonstrokeBrush = CreateSolidBrush(NonstrokeColor);

    protected abstract T CreateSolidBrush(DeviceColor color);
}

public abstract partial  class GraphicsState: IGraphiscState
{
    [MacroItem("Matrix3x2", "TransformMatrix", "Matrix3x2.Identity")]
    [MacroItem("Matrix3x2", "TextMatrix", "Matrix3x2.Identity")]
    [MacroItem("Matrix3x2", "TextLineMatrix", "Matrix3x2.Identity")]
    [MacroItem("double", "LineWidth", "1.0")]
    [MacroItem("double", "MiterLimit", "10.0")]
    [MacroItem("LineJoinStyle", "LineJoinStyle", "LineJoinStyle.Miter")]
    [MacroItem("LineCap", "LineCap", "LineCap.Butt")]
    [MacroItem("double", "DashPhase", "0.0")]
    [MacroItem("double", "FlatnessTolerance", "0.0")]
    [MacroItem("double[]", "DashArray", "Array.Empty<double>()")]
    [MacroItem("RenderIntentName", "RenderIntent", "RenderIntentName.RelativeColoriMetric")]
    [MacroItem("IColorSpace", "StrokeColorSpace", "DeviceGray.Instance")]
    [MacroItem("IColorSpace", "NonstrokeColorSpace", "DeviceGray.Instance")]
    [MacroItem("DeviceColor", "StrokeColor", "DeviceColor.Black")]
    [MacroItem("DeviceColor", "NonstrokeColor", "DeviceColor.Black")]

    // Text Properties
    [MacroItem("double", "CharacterSpacing", "0.0")]
    [MacroItem("double", "WordSpacing", "0.0")]
    [MacroItem("double", "TextLeading", "0.0")]
    [MacroItem("double", "TextRise", "0.0")]
    [MacroItem("double", "HorizontalTextScale", "100.0")]
    [MacroItem("TextRendering", "TextRender", "TextRendering.Fill")]
    [MacroItem("IRealizedFont", "Typeface", "NullRealizedFont.Instance")]
    [MacroItem("double", "FontSize", "0.0")]
    [MacroItem("IByteToUnicodeMapping", "ByteMapper", "CharacterEncodings.Standard")]

    // code
    [MacroCode("public ~0~ ~1~ {get; private set;} = ~2~;")]
    [MacroCode("    ~1~ = other.~1~;", Prefix = "public virtual void CopyFrom(GraphicsState other){", Postfix = "}")]
    public void SaveGraphicsState() { }
    public void RestoreGraphicsState() { }

    public void ModifyTransformMatrix(in Matrix3x2 newTransform)
    {
        TransformMatrix = newTransform * TransformMatrix;
    }

    public Vector2 ApplyCurrentTransform(in Vector2 point) => Vector2.Transform(point, TransformMatrix);

    #region Drawing

    public void SetLineWidth(double width) => LineWidth = width;
    public void SetLineCap(LineCap cap) => LineCap = cap;
    public void SetLineJoinStyle(LineJoinStyle lineJoinStyle) => LineJoinStyle = lineJoinStyle;
    public void SetMiterLimit(double miter) => MiterLimit = miter;

    public void SetLineDashPattern(double dashPhase, in ReadOnlySpan<double> dashArray)
    {
        DashPhase = dashPhase;
        // this copies the dasharray on write, which is unavoidable, but then it reuses the array object when
        // we duplicate the graphicsstate, which we expect to happen frequently.
        DashArray = dashArray.ToArray(); 
    }
    
    private async ValueTask SetLineDashPattern(PdfArray entryValue)
    {
        DashPhase = (await entryValue.GetAsync<PdfNumber>(1).CA()).DoubleValue;
        var pattern = await entryValue.GetAsync<PdfArray>(0).CA();
        var patternNums = new double[pattern.Count];
        for (int i = 0; i < pattern.Count; i++)
        {
            patternNums[i] = (await pattern.GetAsync<PdfNumber>(i).CA()).DoubleValue;
        }
        DashArray = patternNums;
    }
    
    // as of 11/18/2021 this parameter is ignored by both the Pdf and Skia renderers, but
    // we will preserve the property.
    public void SetFlatnessTolerance(double flatness) => FlatnessTolerance = flatness;
    

    #endregion    
    
    public async ValueTask LoadGraphicStateDictionary(PdfDictionary dictionary)
    {
        foreach (var entry in dictionary.RawItems)
        {
            var hashCode = entry.Key.GetHashCode();
            switch (hashCode)
            {
                case KnownNameKeys.LW:
                    SetLineWidth((await EntryValue<PdfNumber>(entry).CA()).DoubleValue);
                    break;
                case KnownNameKeys.FL:
                    SetFlatnessTolerance((await EntryValue<PdfNumber>(entry).CA()).DoubleValue);
                    break;
                case KnownNameKeys.LC:
                    SetLineCap((LineCap)(await EntryValue<PdfNumber>(entry).CA()).IntValue);
                    break;
                case KnownNameKeys.LJ:
                    SetLineJoinStyle((LineJoinStyle)(await EntryValue<PdfNumber>(entry).CA()).IntValue);
                    break;
                case KnownNameKeys.ML:
                    SetMiterLimit((await EntryValue<PdfNumber>(entry).CA()).DoubleValue);
                    break;
                case KnownNameKeys.D:
                    await SetLineDashPattern(await EntryValue<PdfArray>(entry).CA()).CA();
                    break;
                case KnownNameKeys.RI:
                    SetRenderIntent(new RenderIntentName(await EntryValue<PdfName>(entry).CA()));
                    break;
            }
        }
    }
    
    private static async ValueTask<T> EntryValue<T>(KeyValuePair<PdfName, PdfObject> entry)
        where T:PdfObject
    {
        return (T) await entry.Value.DirectValueAsync().CA();
    }

    #region Text State

    public WritingMode WritingMode => WritingMode.LeftToRight;
    
    public void SetCharSpace(double value) => CharacterSpacing = value;
    public void SetWordSpace(double value) => WordSpacing = value;
    public void SetHorizontalTextScaling(double value) => HorizontalTextScale = value;
    public void SetTextLeading(double value) => TextLeading = value;
    public ValueTask SetFont(PdfName font, double size)
    {
        FontSize = size;
        return ValueTask.CompletedTask;
    }

    public void SetTextRender(TextRendering rendering) => TextRender = rendering;
    public void SetTextRise(double value) => TextRise = value;

    #endregion

    #region Text Position

    public void SetTextMatrix(in Matrix3x2 value) => TextMatrix = value;
    public void SetTextLineMatrix(in Matrix3x2 value) => TextLineMatrix = value;
    public void SetBothTextMatrices(in Matrix3x2 value)
    {
        SetTextMatrix(value);
        SetTextLineMatrix(value);
    }

    #endregion

    #region Color

    protected abstract void StrokeColorChanged();
    protected abstract void NonstrokeColorChanged();
    public void SetRenderIntent(RenderIntentName intent) => RenderIntent = intent;
    public void SetStrokeColorSpace(IColorSpace colorSpace)
    {
        SetStrokeColor(colorSpace.DefaultColor());
        StrokeColorSpace = colorSpace;
    }
    
    public void SetNonstrokeColorSpace(IColorSpace colorSpace)
    {
        SetNonstrokeColor(colorSpace.DefaultColor());
        NonstrokeColorSpace = colorSpace;
    }

    public GraphicsState CurrentState() => this;

    public void SetStrokeColor(in ReadOnlySpan<double> color) => 
        SetStrokeColor(StrokeColorSpace.SetColor(color));
    private void SetStrokeColor(DeviceColor color)
    {
        StrokeColor = color;
        StrokeColorChanged();
    }
    public void SetNonstrokingColor(in ReadOnlySpan<double> color) => 
        NonstrokeColor = NonstrokeColorSpace.SetColor(color);
    private void SetNonstrokeColor(DeviceColor color)
    {
        NonstrokeColor = color;
        NonstrokeColorChanged();
    }
    #endregion

    public void SetTypeface(IRealizedFont realizedFont)
    {
        (Typeface as IDisposable)?.Dispose();
        Typeface = realizedFont;
    }

    //Only the lowest state on the stack should dispose of a typeface, because we might pop the stack and come
    // back to a prior font.  If the typeface is disposable, wrap it in a wrapper that does not implement IDisposable
    // and this instance will never dispose of the typeface
    public void MakeFontNotDisposable() =>
        Typeface = BlockFontDispose.AsNonDisposableTypeface(Typeface);
}

public static class GraphicsStateHelpers
{
    public static bool IsDashedStroke(this GraphicsState gs) =>
        gs.DashArray.Length > 0;


    public static double EffectiveLineWidth(this GraphicsState state)
    {
        if (state.LineWidth > double.Epsilon) return state.LineWidth;
        if (!Matrix3x2.Invert(state.TransformMatrix, out var invmat)) return 1;
        return invmat.M11;
    }
}