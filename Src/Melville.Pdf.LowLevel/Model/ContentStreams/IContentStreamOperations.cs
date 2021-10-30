﻿namespace Melville.Pdf.LowLevel.Model.ContentStreams;

public interface IStateChangingCSOperations
{
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
    void ModifyTransformMatrix(params double[] xFromParameters);

    /// <summary>
    /// Content stream operator lineWidth w
    /// </summary>
    void SetLineWidth(double width);
}

public interface IStatePreservingCSOperations
{
}

public interface IContentStreamOperations: IStateChangingCSOperations, IStatePreservingCSOperations
{
    
}