﻿using System.Collections.Generic;
using System.Linq;
using Melville.INPC;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Encryption.CryptContexts;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Encryption.SecurityHandlers;

internal class DocumentCryptContextV4: IDocumentCryptContext
{
    private Dictionary<PdfName, IDocumentCryptContext> contexts;
        

    public DocumentCryptContextV4(Dictionary<PdfName, IDocumentCryptContext> contexts)
    {
        this.contexts = contexts;
    }

    public IObjectCryptContext ContextForObject(int objectNumber, int generationNumber) =>
        new ObjectContextV4(contexts, objectNumber, generationNumber);

    public bool BlockEncryption(PdfObject item) => contexts.Values.Any(i => i.BlockEncryption(item));
}
internal class ObjectContextV4: IObjectCryptContext
{
    private readonly IReadOnlyDictionary<PdfName, IDocumentCryptContext> context;
    private readonly int objectNumber;
    private readonly int genetrationNumber;

    public ObjectContextV4(IReadOnlyDictionary<PdfName, IDocumentCryptContext> context, int objectNumber, int genetrationNumber)
    {
        this.context = context;
        this.objectNumber = objectNumber;
        this.genetrationNumber = genetrationNumber;
    }

    public ICipher StringCipher() =>
        context[KnownNames.StrF]
            .ContextForObject(objectNumber, genetrationNumber)
            .StringCipher();

    public ICipher StreamCipher()=>
        context[KnownNames.StmF]
            .ContextForObject(objectNumber, genetrationNumber)
            .StringCipher();

    public ICipher NamedCipher(PdfName name) =>
        context[name]
            .ContextForObject(objectNumber, genetrationNumber)
            .NamedCipher(name);
}

internal partial class SecurityHandlerV4 : ISecurityHandler
{
    [FromConstructor]private readonly IRootKeyComputer rootKeyComputer;
    [FromConstructor]private readonly Dictionary<PdfName, ISecurityHandler> handlers;
    
    public byte[]? TryComputeRootKey(string password, PasswordType type) => 
        rootKeyComputer.TryComputeRootKey(password, type);

    public IDocumentCryptContext CreateCryptContext(byte[] rootKey) =>
        new DocumentCryptContextV4(handlers.ToDictionary(
            handlerPair => handlerPair.Key,
            handlerPair => handlerPair.Value.CreateCryptContext(rootKey)));
}