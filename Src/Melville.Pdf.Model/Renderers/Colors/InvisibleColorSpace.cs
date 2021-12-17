﻿using System;

namespace Melville.Pdf.Model.Renderers.Colors;

public class InvisibleColorSpace: IColorSpace
{
    public DeviceColor SetColor(in ReadOnlySpan<double> newColor) => DeviceColor.Invisible;

    public DeviceColor DefaultColor()  => DeviceColor.Invisible;
    public DeviceColor SetColorFromBytes(in ReadOnlySpan<byte> newColor) => DeviceColor.Invisible;
}