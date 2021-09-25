﻿using System;
using Melville.Pdf.LowLevel.Encryption.Cryptography;
using Melville.Pdf.LowLevel.Encryption.Readers;

namespace Melville.Pdf.LowLevel.Encryption.PasswordHashes
{
    public sealed class ComputeUserPasswordV2 : IComputeUserPassword
    {
        public byte[] ComputeHash(in ReadOnlySpan<byte> encryptionKey, EncryptionParameters parameters)
        {
            var rc4 = new RC4(encryptionKey);
            var ret = new byte[32];
            rc4.Transform(BytePadder.PdfPasswordPaddingBytes, ret);
            return ret;
        }

        public bool CompareHashes(in ReadOnlySpan<byte> a, in ReadOnlySpan<byte> b) => 
            a.SequenceCompareTo(b) == 0;
    }
}