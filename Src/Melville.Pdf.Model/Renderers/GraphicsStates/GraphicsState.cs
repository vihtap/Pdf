﻿using System;
using System.Numerics;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

public partial class GraphicsState: IStateChangingOperations
{
    [MacroItem("Matrix3x2", "TransformMatrix", "Matrix3x2.Identity")]
    [MacroItem("double", "LineWidth", "1.0")]
    [MacroItem("double", "MiterLimit", "10.0")]
    [MacroItem("LineJoinStyle", "LineJoinStyle", "LineJoinStyle.Miter")]
    [MacroItem("LineCap", "LineCap", "LineCap.Butt")]
    [MacroItem("double", "DashPhase", "0.0")]
    [MacroItem("double", "FlatnessTolerance", "0.0")]
    [MacroItem("double[]", "DashArray", "Array.Empty<double>()")]
    [MacroItem("RenderingIntentName", "RenderIntent", "RenderingIntentName.RelativeColoriMetric")]
    [MacroCode("public ~0~ ~1~ {get; private set;} = ~2~;")]
    [MacroCode("    ~1~ = other.~1~;", Prefix = "public void CopyFrom(GraphicsState other){", Postfix = "}")]
    public void SaveGraphicsState() { }
    public void RestoreGraphicsState() { }

    public void ModifyTransformMatrix(in Matrix3x2 newTransform)
    {
        TransformMatrix = newTransform * TransformMatrix;
    }

    public Vector2 ApplyCurrentTransform(in Vector2 point) => Vector2.Transform(point, TransformMatrix);
    
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

    public void SetRenderIntent(RenderingIntentName intent) => RenderIntent = intent;

    // as of 11/18/2021 this parameter is ignored by both the Pdf and Skia renderers, but
    // we will preserve the property.
    public void SetFlatnessTolerance(double flatness) => FlatnessTolerance = flatness;
    
    public void LoadGraphicStateDictionary(PdfName dictionaryName)
    {
        throw new NotImplementedException();
    }

    public void SetCharSpace(double value)
    {
        throw new NotImplementedException();
    }

    public void SetWordSpace(double value)
    {
        throw new NotImplementedException();
    }

    public void SetHorizontalTextScaling(double value)
    {
        throw new NotImplementedException();
    }

    public void SetTextLeading(double value)
    {
        throw new NotImplementedException();
    }

    public void SetFont(PdfName font, double size)
    {
        throw new NotImplementedException();
    }

    public void SetTextRender(TextRendering rendering)
    {
        throw new NotImplementedException();
    }

    public void SetTextRise(double value)
    {
        throw new NotImplementedException();
    }
}

public static class GraphicsStateHelpers
{
    public static bool IsDashedStroke(this GraphicsState gs) =>
        gs.DashArray.Length > 0;
}