﻿using System;
using Melville.CCITT;
using Melville.Parsing.VariableBitEncoding;
using Xunit;

namespace Melville.Pdf.DataModelTests.Writer.CcittParts;

public class CcittHorizontalEncoderTest
{
    [Theory]
    [InlineData("001 00110101 0000110111", 0)]
    [InlineData("001 000111 010", 1)]
    [InlineData("001 0111 11", 2)]
    [InlineData("001 1000 10", 3)]
    [InlineData("001 1011 011", 4)]
    [InlineData("001 1100 0011", 5)]
    [InlineData("001 1110 0010", 6)]
    [InlineData("001 1111 00011", 7)]
    [InlineData("001 10011 000101", 8)]
    [InlineData("001 10100 000100", 9)]
    [InlineData("001 00111 0000100", 10)]
    [InlineData("001 01000 0000101", 11)]
    [InlineData("001 001000 0000111", 12)]
    [InlineData("001 000011 00000100", 13)]
    [InlineData("001 110100 00000111", 14)]
    [InlineData("001 110101 000011000", 15)]
    [InlineData("001 101010 0000010111", 16)]
    [InlineData("001 101011 0000011000", 17)]
    [InlineData("001 0100111 0000001000", 18)]
    [InlineData("001 0001100 00001100111", 19)]
    [InlineData("001 0001000 00001101000", 20)]
    [InlineData("001 0010111 00001101100", 21)]
    [InlineData("001 0000011 00000110111", 22)]
    [InlineData("001 0000100 00000101000", 23)]
    [InlineData("001 0101000 00000010111", 24)]
    [InlineData("001 0101011 00000011000", 25)]
    [InlineData("001 0010011 000011001010", 26)]
    [InlineData("001 0100100 000011001011", 27)]
    [InlineData("001 0011000 000011001100", 28)]
    [InlineData("001 00000010 000011001101", 29)]
    [InlineData("001 00000011 000001101000", 30)]
    [InlineData("001 00011010 000001101001", 31)]
    [InlineData("001 00011011 000001101010", 32)]
    [InlineData("001 00010010 000001101011", 33)]
    [InlineData("001 00010011 000011010010", 34)]
    [InlineData("001 00010100 000011010011", 35)]
    [InlineData("001 00010101 000011010100", 36)]
    [InlineData("001 00010110 000011010101", 37)]
    [InlineData("001 00010111 000011010110", 38)]
    [InlineData("001 00101000 000011010111", 39)]
    [InlineData("001 00101001 000001101100", 40)]
    [InlineData("001 00101010 000001101101", 41)]
    [InlineData("001 00101011 000011011010", 42)]
    [InlineData("001 00101100 000011011011", 43)]
    [InlineData("001 00101101 000001010100", 44)]
    [InlineData("001 00000100 000001010101", 45)]
    [InlineData("001 00000101 000001010110", 46)]
    [InlineData("001 00001010 000001010111", 47)]
    [InlineData("001 00001011 000001100100", 48)]
    [InlineData("001 01010010 000001100101", 49)]
    [InlineData("001 01010011 000001010010", 50)]
    [InlineData("001 01010100 000001010011", 51)]
    [InlineData("001 01010101 000000100100", 52)]
    [InlineData("001 00100100 000000110111", 53)]
    [InlineData("001 00100101 000000111000", 54)]
    [InlineData("001 01011000 000000100111", 55)]
    [InlineData("001 01011001 000000101000", 56)]
    [InlineData("001 01011010 000001011000", 57)]
    [InlineData("001 01011011 000001011001", 58)]
    [InlineData("001 01001010 000000101011", 59)]
    [InlineData("001 01001011 000000101100", 60)]
    [InlineData("001 00110010 000001011010", 61)]
    [InlineData("001 00110011 000001100110", 62)]
    [InlineData("001 00110100 000001100111", 63)]
    [InlineData("001 11011 00110101 0000001111 0000110111", 64)]
    [InlineData("001 10010 00110101 000011001000 0000110111", 128)]
    [InlineData("001 010111 00110101 000011001001 0000110111", 192)]
    [InlineData("001 0110111 00110101 000001011011 0000110111", 256)]
    [InlineData("001 00110110 00110101 000000110011 0000110111", 320)]
    [InlineData("001 00110111 00110101 000000110100 0000110111", 384)]
    [InlineData("001 01100100 00110101 000000110101 0000110111", 448)]
    [InlineData("001 01100101 00110101 0000001101100 0000110111", 512)]
    [InlineData("001 01101000 00110101 0000001101101 0000110111", 576)]
    [InlineData("001 01100111 00110101 0000001001010 0000110111", 640)]
    [InlineData("001 011001100 00110101 0000001001011 0000110111", 704)]
    [InlineData("001 011001101 00110101 0000001001100 0000110111", 768)]
    [InlineData("001 011010010 00110101 0000001001101 0000110111", 832)]
    [InlineData("001 011010011 00110101 0000001110010 0000110111", 896)]
    [InlineData("001 011010100 00110101 0000001110011 0000110111", 960)]
    [InlineData("001 011010101 00110101 0000001110100 0000110111", 1024)]
    [InlineData("001 011010110 00110101 0000001110101 0000110111", 1088)]
    [InlineData("001 011010111 00110101 0000001110110 0000110111", 1152)]
    [InlineData("001 011011000 00110101 0000001110111 0000110111", 1216)]
    [InlineData("001 011011001 00110101 0000001010010 0000110111", 1280)]
    [InlineData("001 011011010 00110101 0000001010011 0000110111", 1344)]
    [InlineData("001 011011011 00110101 0000001010100 0000110111", 1408)]
    [InlineData("001 010011000 00110101 0000001010101 0000110111", 1472)]
    [InlineData("001 010011001 00110101 0000001011010 0000110111", 1536)]
    [InlineData("001 010011010 00110101 0000001011011 0000110111", 1600)]
    [InlineData("001 011000 00110101 0000001100100 0000110111", 1664)]
    [InlineData("001 010011011 00110101 0000001100101 0000110111", 1728)]
    [InlineData("001 11011 000111 0000001111 010", 64 + 1)]
    [InlineData("001 10010 000111 000011001000 010", 128 + 1)]
    [InlineData("001 010111 000111 000011001001 010", 192 + 1)]
    [InlineData("001 0110111 000111 000001011011 010", 256 + 1)]
    [InlineData("001 00110110 000111 000000110011 010", 320 + 1)]
    [InlineData("001 00110111 000111 000000110100 010", 384 + 1)]
    [InlineData("001 01100100 000111 000000110101 010", 448 + 1)]
    [InlineData("001 01100101 000111 0000001101100 010", 512 + 1)]
    [InlineData("001 01101000 000111 0000001101101 010", 576 + 1)]
    [InlineData("001 01100111 000111 0000001001010 010", 640 + 1)]
    [InlineData("001 011001100 000111 0000001001011 010", 704 + 1)]
    [InlineData("001 011001101 000111 0000001001100 010", 768 + 1)]
    [InlineData("001 011010010 000111 0000001001101 010", 832 + 1)]
    [InlineData("001 011010011 000111 0000001110010 010", 896 + 1)]
    [InlineData("001 011010100 000111 0000001110011 010", 960 + 1)]
    [InlineData("001 011010101 000111 0000001110100 010", 1024 + 1)]
    [InlineData("001 011010110 000111 0000001110101 010", 1088 + 1)]
    [InlineData("001 011010111 000111 0000001110110 010", 1152 + 1)]
    [InlineData("001 011011000 000111 0000001110111 010", 1216 + 1)]
    [InlineData("001 011011001 000111 0000001010010 010", 1280 + 1)]
    [InlineData("001 011011010 000111 0000001010011 010", 1344 + 1)]
    [InlineData("001 011011011 000111 0000001010100 010", 1408 + 1)]
    [InlineData("001 010011000 000111 0000001010101 010", 1472 + 1)]
    [InlineData("001 010011001 000111 0000001011010 010", 1536 + 1)]
    [InlineData("001 010011010 000111 0000001011011 010", 1600 + 1)]
    [InlineData("001 011000 000111 0000001100100 010", 1664 + 1)]
    [InlineData("001 010011011 000111 0000001100101 010", 1728 + 1)]
    [InlineData("001 00000001000 000111 00000001000 010", 1792 + 1)]
    [InlineData("001 00000001100 000111 00000001100 010", 1856 + 1)]
    [InlineData("001 00000001101 000111 00000001101 010", 1920 + 1)]
    [InlineData("001 000000010010 000111 000000010010 010", 1984 + 1)]
    [InlineData("001 000000010011 000111 000000010011 010", 2048 + 1)]
    [InlineData("001 000000010100 000111 000000010100 010", 2112 + 1)]
    [InlineData("001 000000010101 000111 000000010101 010", 2176 + 1)]
    [InlineData("001 000000010110 000111 000000010110 010", 2240 + 1)]
    [InlineData("001 000000010111 000111 000000010111 010", 2304 + 1)]
    [InlineData("001 000000011100 000111 000000011100 010", 2368 + 1)]
    [InlineData("001 000000011101 000111 000000011101 010", 2432 + 1)]
    [InlineData("001 000000011110 000111 000000011110 010", 2496 + 1)]
    [InlineData("001 000000011111 000111 000000011111 010", 2560 + 1)]
    public void Encode(string result, int length)
    {
        var buffer = new byte[10];
        var bw = new BitWriter();
        var writer = new BitTarget(buffer.AsSpan(), bw);
        HorizontalSpanEncoder.Write(ref writer, true, length, length);
        var wl = writer.BytesWritten + bw.FinishWrite(buffer.AsSpan(writer.BytesWritten));
        Assert.Equal(0, StringToByteArray.CreateArray(result).AsSpan()
            .SequenceCompareTo(buffer.AsSpan(0,wl)));
        
    }
}