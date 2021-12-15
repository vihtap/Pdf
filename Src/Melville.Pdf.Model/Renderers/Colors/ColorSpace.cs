﻿using System;
using System.Text.Json;
using System.Threading.Tasks;
using Melville.Icc.Model;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors.Profiles;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Pdf.Model.Renderers.Colors;

public record struct DeviceColor(double Red, double Green, double Blue)
{
    public static readonly DeviceColor Black = new(0, 0, 0);

    public byte RedByte => (byte)(255 * Red);
    public byte GreenByte => (byte)(255 * Green);
    public byte BlueByte => (byte)(255 * Blue);
}

public interface IColorSpace
{
    public DeviceColor SetColor(in ReadOnlySpan<double> newColor);
    public DeviceColor DefaultColor();
}

public static class ColorSpaceFactory
{
    public static ValueTask<IColorSpace> ParseColorSpace(PdfName colorSpaceName, in PdfPage page)
    {
        #warning both cmyk and CalGray need to honor the current rendering intent
        return colorSpaceName.GetHashCode() switch
        {
            KnownNameKeys.DeviceGray => new(DeviceGray.Instance),
            KnownNameKeys.DeviceRGB => new(DeviceRgb.Instance),
            KnownNameKeys.DeviceCMYK => CreateCmykColorSpace(),
            _ => FromArray(page, colorSpaceName)
        };
    }
    private static IColorSpace? cmykColorSpacel;
    public static async ValueTask<IColorSpace> CreateCmykColorSpace() => cmykColorSpacel ??= 
        new IccColorSpace((await IccProfileLibrary.ReadCmyk()).TransformTo(
            await IccProfileLibrary.ReadSrgb()));

    private static async ValueTask<IColorSpace> FromArray(PdfPage page, PdfName colorSpaceName)
    {
        var obj = await page.GetResourceObject(ResourceTypeName.ColorSpace, colorSpaceName);
        return obj is PdfArray array? await FromArray(array): DeviceGray.Instance;
    }

    private static async ValueTask<IColorSpace> FromArray(PdfArray array) =>
        (await array.GetAsync<PdfName>(0)).GetHashCode() switch
        {
            KnownNameKeys.CalGray => await CalGray.Parse(await array.GetAsync<PdfDictionary>(1)),
            // for monitors ignore CalRGB see standard section 8.6.5.7
            KnownNameKeys.CalRGB => DeviceRgb.Instance, 
            KnownNameKeys.CalCMYK => await CreateCmykColorSpace(), // standard section 8.6.5.1
            KnownNameKeys.Lab => await LabColorSpace.Parse(await array.GetAsync<PdfDictionary>(1)),
            KnownNameKeys.ICCBased => await IccProfileColorSpace.Parse(await array.GetAsync<PdfStream>(1)),
            _=> throw new PdfParseException("Unrecognized Colorspace")
        };

}