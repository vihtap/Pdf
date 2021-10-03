﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Encryption.New;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.Decryptors;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Encryption.Readers
{
    public interface ISecurityHandler
    {
        byte[]? TryComputeRootKey(string password, PasswordType type);
        IDocumentCryptContext CreateCryptContext(byte[] rootKey);
    }

    public static class SecurityHandlerOperations
    {
        public static async ValueTask<IDocumentCryptContext> InteractiveGetCryptContext(
            this ISecurityHandler handler, IPasswordSource source)
        {
            return handler.CreateCryptContext(await GetRootKey(handler, source));
        }

        private static ValueTask<byte[]> GetRootKey(ISecurityHandler handler, IPasswordSource source) =>
            handler.TryComputeRootKey("", PasswordType.User) is { } rootKey
                ? new(rootKey)
                : InteractiveGetRootKey(handler, source);
        
        private static async ValueTask<byte[]> InteractiveGetRootKey(
            ISecurityHandler handler, IPasswordSource source)
        {
            while (true)
            {
                var (password, type) = await source.GetPassword();
                if (password == null)
                    throw new PdfSecurityException("User cancelled pdf decryption by not providing password.");
                if (handler.TryComputeRootKey(password, type) is { } rootKey) 
                    return rootKey;
            }
        }
    }

    public class SecurityHandler : ISecurityHandler
    {
        private readonly IEncryptorAndDecryptorFactory encryptorAndDecryptorFactory;
        private readonly IKeySpecializer keySpecializer;
        private readonly ICipherFactory cipherFactory;
        private readonly RootKeyComputer rootKeyComputer;
        private byte[]? encryptionKey;
        private PdfObject? blockEncryption;
        
        public SecurityHandler(IEncryptorAndDecryptorFactory encryptorAndDecryptorFactory, 
            IKeySpecializer keySpecializer, 
            ICipherFactory cipherFactory,
            RootKeyComputer rootKeyComputer, PdfObject? blockEncryption)
        {
            this.encryptorAndDecryptorFactory =encryptorAndDecryptorFactory;
            this.keySpecializer = keySpecializer;
            this.cipherFactory = cipherFactory;
            this.rootKeyComputer = rootKeyComputer;
            this.blockEncryption = blockEncryption;
        }

        public byte[]? TryComputeRootKey(string password, PasswordType type) => 
            encryptionKey = rootKeyComputer.TryComputeRootKey(password.AsExtendedAsciiBytes(), type);

        public IDocumentCryptContext CreateCryptContext(byte[] rootKey) => 
            new DocumentCryptContext(rootKey, keySpecializer, cipherFactory, blockEncryption);
    }
}