﻿using System;
using System.Buffers;
using System.Diagnostics;
using Melville.INPC;
using Melville.Parsing.SequenceReaders;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Filters.Jpeg;

public partial class JpegStreamFactory
{
    [StaticSingleton]
    public sealed partial class StartOfFrameParser : IJpegBlockParser
    {
        public void ParseBlock(SequenceReader<byte> data, JpegStreamFactory factory)
        {
            var precision = data.ReadBigEndianUint8();
            if (precision != 8) throw new NotImplementedException("Only 8 bit JPEGS are supported");
            var height = data.ReadBigEndianUint16();
            var width = data.ReadBigEndianUint16();
            var componentData = ReadAllComponents(ref data);
            factory.frameData = new JpegFrameData(width, height, precision, componentData);
            Debug.WriteLine($"    ImageSize: ({width}, {height})");
            Debug.WriteLine($"    Precision: {precision}");
            foreach (var component in componentData)
            {
                Debug.WriteLine($"    {component.Id}: xSamp: {component.HorizontalSamplingFactor} ysamp: {component.HorizontalSamplingFactor} QuantTable: {component.QuantTableNumber}");
            }
        }

        private CompnentData[] ReadAllComponents(ref SequenceReader<byte> data)
        {
            var components = data.ReadBigEndianUint8();
            if (components is < 1 or > 4) throw new PdfParseException("Unknown number of JPEG components");
            var componentData = new CompnentData[components];
            ParseAllComponentData(ref data, componentData);
            return componentData;
        }

        private void ParseAllComponentData(ref SequenceReader<byte> data, CompnentData[] componentData)
        {
            for (int i = 0; i < componentData.Length; i++) 
                componentData[i] = ParseComponentData(ref data);
        }

        private CompnentData ParseComponentData(ref SequenceReader<byte> data)
        {
            Span<byte> componentData = stackalloc byte[3];
            data.TryCopyTo(componentData);
            data.Advance(3);
            return new CompnentData((ComponentId)componentData[0], componentData[1], componentData[2]);
        }
    }
}