﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter
{
    public class BitWriter2
    {
        private byte residue;
        private byte spotsAvailable;
        
        public BitWriter2()
        {
            residue = 0;
            spotsAvailable = 8;
        }

        public int WriteBits(int data, int bits, Span<byte> target)
        {
            var position = 0;
            if (spotsAvailable == 0)
            {
                position +=  WriteCurrentByte(target);
            }
            int leftOverBits = bits - spotsAvailable;
            if (leftOverBits <= 0)
            {
                AddBottomBitsToResidue(data, bits);
                return position;
            }

            AddBottomBitsToResidue(data >> leftOverBits, spotsAvailable);
            return position + WriteBits(data, leftOverBits, target.Slice(position));
        }

        private void AddBottomBitsToResidue(int data, int bits)
        {
            residue |= (byte) ((data & BitUtilities.Mask(bits)) << (spotsAvailable - bits));
            spotsAvailable = (byte) (spotsAvailable - bits);
        }
        
        public int FinishWrite(Span<byte> target) => 
            NoBitsWaitingToBeWritten() ? 0 : WriteCurrentByte(target);

        private bool NoBitsWaitingToBeWritten() => spotsAvailable > 7;

        private int WriteCurrentByte(Span<byte> target)
        {
            WriteByte(target);
            return  1;
        }

        private void WriteByte(Span<byte> span)
        {
            span[0] = residue;
            residue = 0;
            spotsAvailable = 8;
        }
    }

    public class BitWriter
    {
        private readonly PipeWriter target;
        private byte residue;
        private byte spotsAvailable;
        

        public BitWriter(PipeWriter target)
        {
            this.target = target;
            residue = 0;
            spotsAvailable = 8;
        }

        public async ValueTask WriteBits(int data, int bits)
        {
            if (spotsAvailable == 0) await WriteCurrentByte();
            int leftOverBits = bits - spotsAvailable;
            if (leftOverBits <= 0)
            {
                WriteBottomBits(data, bits);
                return;
            }

            WriteBottomBits(data >> leftOverBits, spotsAvailable);
            await WriteBits(data, leftOverBits);
        }

        private void WriteBottomBits(int data, int bits)
        {
            residue |= (byte) ((data & BitUtilities.Mask(bits)) << (spotsAvailable - bits));
            spotsAvailable = (byte) (spotsAvailable - bits);
        }
        
        public ValueTask<FlushResult> FinishWrite()
        {
            if (spotsAvailable > 7) return new ValueTask<FlushResult>(new FlushResult());
            return WriteCurrentByte();
        }

        private ValueTask<FlushResult> WriteCurrentByte()
        {
            WriteByte(target.GetSpan(1));
            target.Advance(1);
            return target.FlushAsync();
        }

        private void WriteByte(Span<byte> span)
        {
            span[0] = residue;
            residue = 0;
            spotsAvailable = 8;
        }
    }

    public static class BitUtilities
    {
        public static byte Mask(int bits) => (byte) ((1 << bits) - 1);
    }
}