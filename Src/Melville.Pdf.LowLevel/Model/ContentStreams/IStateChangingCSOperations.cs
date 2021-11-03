﻿using System;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public enum TextRendering
{
    Fill = 0,
    Stroke = 1,
    FillAndStroke = 2,
    Invisible = 3,
    FillAndClip = 4,
    StrokeAndClip = 5
}

public interface IStateChangingCSOperations
{
    #region Non Text EXclusive operators

    /// <summary>
    /// Content Stream Operator q
    /// </summary>
    void SaveGraphicsState();
    
    /// <summary>
    /// Content Stream Operator Q
    /// </summary>
    void RestoreGraphicsState();

    /// <summary>
    /// Content Stream Operator a b c d e f cm
    /// </summary>
    void ModifyTransformMatrix(double a, double b, double c, double d, double e, double f);

    /// <summary>
    /// Content stream operator lineWidth w
    /// </summary>
    void SetLineWidth(double width);

    /// <summary>
    /// Content stream operator linecap J
    /// </summary>
    void SetLineCap(LineCap cap);

    /// <summary>
    /// Content stream operator lineJoinStyle j
    /// </summary>
    void SetLineJoinStyle(LineJoinStyle cap);

    /// <summary>
    /// Content stream operator miterLimit M
    /// </summary>
    void SetMiterLimit(double miter);

    /// <summary>
    /// Content stream operator dashArray dashphase
    /// Note the parameters are flipped from the PDF representation to accomodate a params extension method.
    /// </summary>
    void SetLineDashPattern(double dashPhase, in ReadOnlySpan<double> dashArray);

    /// <summary>
    /// Content stream operator renderingIntent ri
    /// </summary>
    /// <param name="intent"></param>
    void SetRenderIntent(RenderingIntentName intent);

    /// <summary>
    /// Content stream operator tolerance i
    /// </summary>
    /// <param name="flatness"></param>
    void SetFlatnessTolerance(double flatness);

    /// <summary>
    /// Content stream operator gs
    /// </summary>
    void LoadGraphicStateDictionary(PdfName dictionaryName);

    #endregion

    #region TextAttribute
    /// <summary>
    /// Content stream operator charSpace Tc
    /// </summary>
    void SetCharSpace(double value);

    /// <summary>
    /// Content stream operator charSpace Tw
    /// </summary>
    void SetWordSpace(double value);

    /// <summary>
    /// Content stream operator charSpace Tz
    /// </summary>
    void SetHorizontalTextScaling(double value);

    /// <summary>
    /// Content stream operator charSpace Tl
    /// </summary>
    void SetTextLeading(double value);

    /// <summary>
    /// Content stream operator charSpace Tf
    /// </summary>
    void SetTextSize(double value);

    /// <summary>
    /// Context stream operator Tr
    /// </summary>
    /// <param name="rendering"></param>
    void SetTextRender(TextRendering rendering);

    /// <summary>
    /// Content stream operator charSpace Ts
    /// </summary>
    void SetTextRise(double value);

    #endregion
}