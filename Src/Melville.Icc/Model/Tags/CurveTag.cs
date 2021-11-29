﻿using System.Buffers;
using Melville.Icc.Parser;

namespace Melville.Icc.Model.Tags;

public class CurveTag: ProfileData
{
     public IReadOnlyList<ushort> Values { get; }

     public CurveTag(ref SequenceReader<byte> reader)
     {
          reader.Skip32BitPad();
          Values = reader.ReadUshortArray((int)reader.ReadBigEndianUint32());
     }
}