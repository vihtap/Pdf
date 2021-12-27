﻿using System;
using System.Runtime.InteropServices;
using Melville.Parsing.Streams.Bases;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Melville.Pdf.LowLevel.Filters.ExternalFilters;

public class ImageReadStream : DefaultBaseStream
{
    private Image<Rgb24> source;
    private int currentRow;
    private int currentByte;
    public ImageReadStream(Image<Rgb24> source) : base(true, false, false)
    {
        this.source = source;
    }

    public override int Read(Span<byte> buffer)
    {
        var totalLen = 0;
        while (buffer.Length > 0 && currentRow < source.Height)
        {
            var localLen = TryReadFromRow(buffer);
            totalLen += localLen;
            buffer = buffer.Slice(localLen);
        }
        
        return totalLen;
    }

    private int TryReadFromRow(Span<byte> buffer)
    {
        var byteSpan = RemainingBytesInCurrentRow();
        var bytesToCopy = ClaimCopyableBytes(byteSpan.Length, buffer.Length);
        byteSpan.Slice(0,bytesToCopy).CopyTo(buffer);
        return bytesToCopy;
    }

    private Span<byte> RemainingBytesInCurrentRow() => 
        MemoryMarshal.AsBytes(source.GetPixelRowSpan(currentRow)).Slice(currentByte);

    private int ClaimCopyableBytes(int sourceLength, int destLength)
    {
        if (sourceLength <= destLength)
        {
            currentByte = 0;
            currentRow++;
            return sourceLength;
        }
        else
        {
            currentByte += destLength;
            return destLength;
        }
    }
}