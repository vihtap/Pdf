﻿using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Melville.Pdf.LowLevel.Model;

namespace Melville.Pdf.LowLevel.Parsing
{
    public ref struct NumberParser
    {
        public static bool TryParse(
            ref SequenceReader<byte> source, [NotNullWhen(true)] out PdfNumber? output) =>
            new NumberParser().InnerTryParse(ref source, out output);

        private int value;
        private int sign;
        private double fractionalPart;
        private double placeValue;
        
        private bool InnerTryParse(
            ref SequenceReader<byte> source, [NotNullWhen(true)] out PdfNumber? output)
        {
            output = null;
            return TryReadSign(ref source) && TryParseWholeDigits(ref source, ref output);
        }

        private bool TryReadSign(ref SequenceReader<byte> source)
        {
            sign = 1;
            if (!source.TryPeek(out var character)) return false;
            switch (character)
            {
                case (int)'+':
                    source.Advance(1);
                    break;
                case (int)'-':
                    source.Advance(1);
                    sign = -1;
                    break;
            }
            return true;
        }

        private bool TryParseWholeDigits(
            ref SequenceReader<byte> source, [NotNullWhen(true)] ref PdfNumber? output)
        {
            while (true)
            {
                if (!source.TryRead(out var character)) return false;
                switch (character)
                {
                    case >= (byte) '0' and <= (byte) '9':
                        ConsumeWholeNumberPart(character);
                        break;
                    case (int)'.':
                        return TryParseFractionalDigits(ref source, ref output);
                    default:
                        source.Rewind(1);
                        return TryCompleteNumberParse(ref source, out output);
                }
            }
        }
        
        private void ConsumeWholeNumberPart(byte character)
        {
            value *= 10;
            value += character - (byte) '0';
        }
        
        private bool TryParseFractionalDigits(ref SequenceReader<byte> source, ref PdfNumber? output)
        {
            placeValue = 1.0;
            while (true)
            {
                if (!source.TryRead(out var character)) return false;
                switch (character)
                {
                    case >= (byte) '0' and <= (byte) '9':
                        ConsumeDecimalNumberPart(character);
                        break;
                    default:
                        source.Rewind(1);
                        return TryCompleteNumberParse(ref source, out output);
                }
            }
        }

        private void ConsumeDecimalNumberPart(byte character)
        {
            placeValue /= 10.0;
            fractionalPart += placeValue * (character - (byte) '0');
        }

        private bool TryCompleteNumberParse(ref SequenceReader<byte> source, out PdfNumber? output)
        {
            CreateParsedNumber(out output);
            return NextTokenFinder.SkipToNextToken(ref source);
        }

        private void CreateParsedNumber(out PdfNumber? output) => 
            output = fractionalPart == 0.0 ? 
                new PdfInteger(sign * value) : 
                new PdfDouble(sign * (value + fractionalPart));
    }
}