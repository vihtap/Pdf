﻿using System.Collections.Generic;
using System.Numerics;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;

namespace Melville.Pdf.Model.Renderers.GraphicsStates;

public partial class GraphicsStateStack: IStateChangingOperations
{
    private readonly Stack<GraphicsState> states;
    public GraphicsState Current() => states.Peek();
   
    public GraphicsStateStack()
    {
        states = new Stack<GraphicsState>();
        states.Push(new GraphicsState());
    }

    public void SaveGraphicsState()
    {
        var newTop = new GraphicsState();
        newTop.CopyFrom(Current());
        states.Push(newTop);
    }

    public void RestoreGraphicsState() => states.Pop();

    [DelegateTo]
    private IStateChangingOperations topState => states.Peek();
}