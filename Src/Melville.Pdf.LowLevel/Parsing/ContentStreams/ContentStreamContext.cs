﻿using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams;

public readonly struct ContentStreamContext
{
    private readonly IContentStreamOperations target;
    private readonly List<double> doubles;

    public ContentStreamContext(IContentStreamOperations target)
    {
        this.target = target;
        doubles = new List<double>();
    }

    public void HandleNumber(double doubleValue)
    {
        doubles.Add(doubleValue);
    }


    public void HandleOpCode(ContentStreamOperatorValue opCode)
    {
        switch (opCode)
        {
            case ContentStreamOperatorValue.b:
                break;
            case ContentStreamOperatorValue.B:
                break;
            case ContentStreamOperatorValue.bStar:
                break;
            case ContentStreamOperatorValue.BStar:
                break;
            case ContentStreamOperatorValue.BDC:
                break;
            case ContentStreamOperatorValue.BI:
                break;
            case ContentStreamOperatorValue.BMC:
                break;
            case ContentStreamOperatorValue.BT:
                break;
            case ContentStreamOperatorValue.BX:
                break;
            case ContentStreamOperatorValue.c:
                break;
            case ContentStreamOperatorValue.cm:
                target.ModifyTransformMatrix(
                    doubles[0], doubles[1], doubles[2], doubles[3], doubles[4], doubles[5]);
                break;
            case ContentStreamOperatorValue.CS:
                break;
            case ContentStreamOperatorValue.cs:
                break;
            case ContentStreamOperatorValue.d:
                break;
            case ContentStreamOperatorValue.d0:
                break;
            case ContentStreamOperatorValue.d1:
                break;
            case ContentStreamOperatorValue.Do:
                break;
            case ContentStreamOperatorValue.DP:
                break;
            case ContentStreamOperatorValue.EI:
                break;
            case ContentStreamOperatorValue.EMC:
                break;
            case ContentStreamOperatorValue.ET:
                break;
            case ContentStreamOperatorValue.f:
                break;
            case ContentStreamOperatorValue.F:
                break;
            case ContentStreamOperatorValue.fStar:
                break;
            case ContentStreamOperatorValue.G:
                break;
            case ContentStreamOperatorValue.g:
                break;
            case ContentStreamOperatorValue.gs:
                break;
            case ContentStreamOperatorValue.h:
                break;
            case ContentStreamOperatorValue.i:
                break;
            case ContentStreamOperatorValue.ID:
                break;
            case ContentStreamOperatorValue.j:
                break;
            case ContentStreamOperatorValue.K:
                break;
            case ContentStreamOperatorValue.k:
                break;
            case ContentStreamOperatorValue.l:
                break;
            case ContentStreamOperatorValue.m:
                break;
            case ContentStreamOperatorValue.M:
                break;
            case ContentStreamOperatorValue.MP:
                break;
            case ContentStreamOperatorValue.n:
                break;
            case ContentStreamOperatorValue.q:
                target.SaveGraphicsState();
                break;
            case ContentStreamOperatorValue.Q:
                target.RestoreGraphicsState();
                break;
            case ContentStreamOperatorValue.re:
                break;
            case ContentStreamOperatorValue.RG:
                break;
            case ContentStreamOperatorValue.rg:
                break;
            case ContentStreamOperatorValue.ri:
                break;
            case ContentStreamOperatorValue.s:
                break;
            case ContentStreamOperatorValue.S:
                break;
            case ContentStreamOperatorValue.SC:
                break;
            case ContentStreamOperatorValue.SCN:
                break;
            case ContentStreamOperatorValue.sh:
                break;
            case ContentStreamOperatorValue.TStar:
                break;
            case ContentStreamOperatorValue.Tc:
                break;
            case ContentStreamOperatorValue.Td:
                break;
            case ContentStreamOperatorValue.TD:
                break;
            case ContentStreamOperatorValue.Tj:
                break;
            case ContentStreamOperatorValue.TJ:
                break;
            case ContentStreamOperatorValue.TL:
                break;
            case ContentStreamOperatorValue.Tm:
                break;
            case ContentStreamOperatorValue.Tr:
                break;
            case ContentStreamOperatorValue.Ts:
                break;
            case ContentStreamOperatorValue.Tw:
                break;
            case ContentStreamOperatorValue.Tz:
                break;
            case ContentStreamOperatorValue.v:
                break;
            case ContentStreamOperatorValue.w:
                break;
            case ContentStreamOperatorValue.W:
                break;
            case ContentStreamOperatorValue.WStar:
                break;
            case ContentStreamOperatorValue.y:
                break;
            case ContentStreamOperatorValue.SingleQuote:
                break;
            case ContentStreamOperatorValue.DoubleQuote:
                break;
            default:
                throw new PdfParseException("Unknown content stream operator");
        }

        ClearStacks();
    }

    private void ClearStacks()
    {
        doubles.Clear();
    }
}