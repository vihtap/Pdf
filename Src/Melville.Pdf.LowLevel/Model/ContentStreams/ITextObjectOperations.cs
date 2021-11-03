﻿using System;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public interface ITextObjectOperations
{
    /// <summary>
    /// Content stream operator Td
    /// </summary>
    void MovePositionBy(double x, double y);
    
    /// <summary>
    /// Content stream operator TD
    /// </summary>
    void MovePositionByWithLeading(double x, double y);

    /// <summary>
    /// Content Stream operator Tm
    /// </summary>
    void SetTextMatrix(double a, double b, double c, double d, double e, double f);

    /// <summary>
    /// Content stream operator T*
    /// </summary>
    void MoveToNextTextLine();

    /// <summary>
    /// Content stream operator Tj
    /// </summary>
    void ShowString(in ReadOnlyMemory<byte> decodedString);

    /// <summary>
    /// Content stream operator '
    /// </summary>
    /// <param name="decodedString"></param>
    void MoveToNextLineAndShowString(in ReadOnlyMemory<byte> decodedString);

    /// <summary>
    /// Content stream operator "
    /// </summary>
    /// <param name="decodedString"></param>
    void MoveToNextLineAndShowString(
        double wordSpace, double charSpace, in ReadOnlyMemory<byte> decodedString);

}

public interface ITextBlockOperations
{
    /// <summary>
    /// Content stream operator BT
    /// </summary>
    void BeginTextObject();
    /// <summary>
    /// Content stream operator ET
    /// </summary>
    void EndTextObject();
    
}