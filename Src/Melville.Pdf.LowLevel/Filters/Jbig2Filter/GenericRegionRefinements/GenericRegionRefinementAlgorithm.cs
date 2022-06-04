﻿using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.GenericRegionRefinements;

public readonly struct GenericRegionRefinementAlgorithm
{
    private readonly BinaryBitmap target;
    private readonly IBinaryBitmap reference;
    private readonly int deltaX;
    private readonly int deltaY;
    private readonly bool useTypicalPredicition;
    private readonly RefinementTemplateSet template;
    private readonly MQDecoder decoder;

    public GenericRegionRefinementAlgorithm(
        BinaryBitmap target, IBinaryBitmap reference, int deltaX, int deltaY, 
        bool useTypicalPredicition, in RefinementTemplateSet template, MQDecoder decoder)
    {
        this.target = target;
        this.reference = reference;
        this.deltaX = deltaX;
        this.deltaY = deltaY;
        this.useTypicalPredicition = useTypicalPredicition;
        this.template = template;
        this.decoder = decoder;
    }

    public void Read(ref SequenceReader<byte> source)
    {
        if (useTypicalPredicition)
            throw new NotImplementedException("Typical Prediction is not implemented");
        for (int i = 0; i < target.Height; i++)
        {
            for (int j = 0; j < target.Width; j++)
            {
                ref var context = ref 
                    template.ContextFor(reference, i + deltaY, j + deltaX, target, i, j);
                var bit = decoder.GetBit(ref source, ref context);
                target[i, j] = bit == 1;
            }
        }
    }
}