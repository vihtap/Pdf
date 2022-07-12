﻿using System.Numerics;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Patterns.Shading;

public abstract class Type3RadialShaderBase : PatternDisplayClass
{
    protected Type3RadialShaderBase(string helpText) : base(helpText)
    {
    }
    
    private PdfDictionary? function = null;
    protected override async ValueTask SetPagePropertiesAsync(PageCreator page)
    {
        function = await BuildFunction();
        await base.SetPagePropertiesAsync(page);
    }

    protected virtual async Task<PdfDictionary> BuildFunction()
    {
        var fbuilder = new SampledFunctionBuilder(4, SampledFunctionOrder.Linear);
        fbuilder.AddInput(2, new ClosedInterval(0, 1));
        fbuilder.AddOutput((double _) => 1, new ClosedInterval(0, 1));
        fbuilder.AddOutput(x => x, new ClosedInterval(0, 1));
        fbuilder.AddOutput(x => x, new ClosedInterval(0, 1));
        return await fbuilder.CreateSampledFunction();
    }

    protected override PdfObject CreatePattern(ILowLevelDocumentCreator arg) =>
        BuildPattern(arg, 
            BuildShader(arg, function ?? throw new InvalidOperationException("No func defined"), 
                new DictionaryBuilder()).AsDictionary(),
            new DictionaryBuilder()).AsDictionary();

    protected virtual DictionaryBuilder BuildPattern(
        ILowLevelDocumentCreator arg, PdfDictionary shading, DictionaryBuilder builder) => builder
        .WithItem(KnownNames.Shading, arg.Add(shading))
        .WithItem(KnownNames.Matrix, Matrix3x2.CreateScale(5 * 72, 3 * 72).AsPdfArray())
        .WithItem(KnownNames.PatternType, 2);

    protected virtual DictionaryBuilder BuildShader(
        ILowLevelDocumentCreator arg, PdfDictionary localFunc, DictionaryBuilder builder) => builder
        .WithItem(KnownNames.Function, arg.Add(localFunc))
        .WithItem(KnownNames.Coords, new PdfArray(0.25, .4, 0.1, .35, .4, .01))
        .WithItem(KnownNames.ShadingType, 3)
        .WithItem(KnownNames.ColorSpace, KnownNames.DeviceRGB);
}

public class Type3RadialShader: Type3RadialShaderBase
{
    public Type3RadialShader() : base("A simple radial shader")
    {
    }
}