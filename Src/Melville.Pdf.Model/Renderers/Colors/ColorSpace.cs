﻿using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Icc.ColorTransforms;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.Colors.Profiles;

namespace Melville.Pdf.Model.Renderers.Colors;

[StaticSingleton]
internal partial class NoPageContext : IHasPageAttributes
{
    public PdfDictionary LowLevel => PdfDictionary.Empty;
    public ValueTask<Stream> GetContentBytesAsync() => new(new MemoryStream(Array.Empty<byte>()));

    public ValueTask<IHasPageAttributes?> GetParentAsync() => new((IHasPageAttributes?)null);
}

internal readonly struct ColorSpaceFactory
{
    private readonly IHasPageAttributes page;
    public ColorSpaceFactory(IHasPageAttributes page)
    {
        this.page = page;
    }

    public ValueTask<IColorSpace> ParseColorSpaceAsync(PdfDirectObject colorSpaceName)
    {
        var code = colorSpaceName;
        return code.Equals(KnownNames.DeviceGray) ||
               code.Equals(KnownNames.DeviceRGB) ||
               code.Equals(KnownNames.DeviceCMYK)
            ? SpacesWithoutParametersAsync(code)
            : LookupInResourceDictionaryAsync(colorSpaceName);
    }

    private async ValueTask<IColorSpace> SearchForDefaultAsync(PdfDirectObject name, Func<ValueTask<IColorSpace>> space) =>
        await ((await page.GetResourceAsync(ResourceTypeName.ColorSpace, name).CA()).TryGet(out PdfArray? array) 
            ?  ExcludeIllegalDefaultAsync(array): space()).CA();

    private async ValueTask<IColorSpace> ExcludeIllegalDefaultAsync(PdfArray array) => 
        (await FromArrayAsync(array).CA()).AsValidDefaultColorSpace();

    public ValueTask<IColorSpace> FromNameOrArrayAsync(PdfDirectObject datum) => datum switch
    {
        {IsName:true} name => ParseColorSpaceAsync(name),
        var x when x.TryGet(out PdfArray? array) => FromArrayAsync(array),
        _ => throw new PdfParseException("Invalid Color space definition")
    };

    private static IColorSpace? cmykColorSpacel;

    public static async ValueTask<IColorSpace> CreateCmykColorSpaceAsync() => cmykColorSpacel ??= 
        new IccColorspaceWithBlackDefault( (await CmykIccProfile.ReadCmykProfileAsync().CA()).DeviceToSrgb());

    private async ValueTask<IColorSpace> LookupInResourceDictionaryAsync(PdfDirectObject colorSpaceName)
    {
        var obj = await page.GetResourceAsync(ResourceTypeName.ColorSpace, colorSpaceName).CA();
        return obj.TryGet(out PdfArray? array)? await FromArrayAsync(array).CA(): DeviceGray.Instance;
    }

    private async ValueTask<IColorSpace> FromArrayAsync(PdfArray array) =>
        await FromMemoryAsync((await array.AsDirectValues().CA()).AsMemory()).CA();

    private ValueTask<IColorSpace> FromMemoryAsync(Memory<PdfDirectObject> memory)
    {
        var array = memory.Span;
        if (array.Length == 0) return new(DeviceRgb.Instance);
        if (array.Length == 1 && array[0].TryGet(out PdfArray innerArray)) 
            return FromArrayAsync(innerArray);
        if (array[0] is not {IsName:true} name) 
            throw new PdfParseException("'Name expected in colorspace array");
        return name switch
        {
            var x when x.Equals(KnownNames.CalGray) => 
                CalGray.ParseAsync(ColorSpaceParameterAs<PdfDictionary>(array)),
            var x when x.Equals(KnownNames.Lab) => 
                LabColorSpace.ParseAsync(ColorSpaceParameterAs<PdfDictionary>(array)),
            var x when x.Equals(KnownNames.ICCBased) => 
                IccProfileColorSpaceParser.ParseAsync(ColorSpaceParameterAs<PdfStream>(array)),
            var x when x.Equals(KnownNames.Indexed) => 
                IndexedColorSpace.ParseAsync(memory, page),
            var x when x.Equals(KnownNames.Separation) => 
                SeparationParser.ParseSeparationAsync(memory, page),
            var x when x.Equals(KnownNames.DeviceN) => 
                SeparationParser.ParseDeviceNAsync(memory, page),
            var x when x.Equals(KnownNames.Pattern) => FromMemoryAsync(memory.Slice(1)),
            var other => SpacesWithoutParametersAsync(other)
        };
    }

    private ValueTask<IColorSpace> SpacesWithoutParametersAsync(PdfDirectObject nameHashCode) => 
        nameHashCode switch
    {
        // for monitors ignore CalRGB see standard section 8.6.5.7
        var x when x.Equals(KnownNames.CalRGB) => new(DeviceRgb.Instance),
        var x when x.Equals(KnownNames.CalCMYK) => 
            CreateCmykColorSpaceAsync(), // standard section 8.6.5.1
        var x when x.Equals(KnownNames.DeviceGray) => 
            SearchForDefaultAsync(KnownNames.DefaultGray, static ()=>new(DeviceGray.Instance)),
        var x when x.Equals(KnownNames.DeviceRGB) => 
            SearchForDefaultAsync(KnownNames.DefaultRGB, static ()=>new(DeviceRgb.Instance)),
        var x when x.Equals(KnownNames.DeviceCMYK) =>  
            SearchForDefaultAsync(KnownNames.DefaultCMYK, CreateCmykColorSpaceAsync),
        _ => throw new PdfParseException("Unrecognized Colorspace")
    };
        

    private static T ColorSpaceParameterAs<T>(in Span<PdfDirectObject> array) =>
        array[1].TryGet(out T ret)? ret: throw new PdfParseException("Dictionary Expected");
}