﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.CryptFilters;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

public interface IStreamDataSource
{
    ValueTask<Stream> OpenRawStream(long streamLength);
    Stream WrapStreamWithDecryptor(Stream encryptedStream, PdfName cryptFilterName);
    Stream WrapStreamWithDecryptor(Stream encryptedStream);
    StreamFormat SourceFormat { get; }
}
    
public class PdfStream : PdfDictionary, IHasInternalIndirectObjects
{
    private IStreamDataSource source;
        
    public PdfStream(IStreamDataSource source, IReadOnlyDictionary<PdfName, PdfObject> rawItems) :
        base(rawItems)
    {
        this.source = source;
    }

    public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    private async ValueTask<Stream> SourceStreamAsync() => 
        await source.OpenRawStream(await DeclaredLengthAsync().ConfigureAwait(false)).ConfigureAwait(false);

    public async ValueTask<long> DeclaredLengthAsync() => 
        TryGetValue(KnownNames.Length, out var len) && await len.ConfigureAwait(false) is PdfNumber num ? num.IntValue : -1;

    public async ValueTask<Stream> StreamContentAsync(StreamFormat desiredFormat = StreamFormat.PlainText,
        IObjectCryptContext? encryptor = null)
    {
        var innerEncryptor = encryptor ?? ErrorObjectEncryptor.Instance;
            
        var decoder = new CryptSingleFilter(new SinglePredictionFilter(new StaticSingleFilter()),
            source, innerEncryptor);
        IFilterProcessor processor = 
            new FilterProcessor(await FilterList().ConfigureAwait(false), await FilterParamList().ConfigureAwait(false), decoder);
        if (await ShouldApplyDefaultEncryption().ConfigureAwait(false))
        {
            processor = new DefaultEncryptionFilterProcessor(
                processor, source, innerEncryptor);
        }
        return await processor.StreamInDesiredEncoding(await SourceStreamAsync().ConfigureAwait(false),
            source.SourceFormat, desiredFormat).ConfigureAwait(false);
    }

    private async ValueTask<IReadOnlyList<PdfObject>> FilterList() => 
        (await this.GetOrNullAsync(KnownNames.Filter).ConfigureAwait(false)).AsList();
    private async ValueTask<IReadOnlyList<PdfObject>> FilterParamList() => 
        (await this.GetOrNullAsync(KnownNames.DecodeParms).ConfigureAwait(false)).AsList();

    public async ValueTask<bool> HasFilterOfType(PdfName filterType)
    {
        foreach (var item in await FilterList().ConfigureAwait(false))
        {
            if (await item.DirectValueAsync().ConfigureAwait(false) == filterType) return true;
        }
        return false;
    }

    private async Task<bool> ShouldApplyDefaultEncryption() =>
        !(await this.GetOrNullAsync(KnownNames.Type).ConfigureAwait(false) == KnownNames.XRef ||
          await HasFilterOfType(KnownNames.Crypt).ConfigureAwait(false));

    public async ValueTask<IEnumerable<ObjectLocation>> GetInternalObjectNumbersAsync()
    {
        if (await this.GetOrNullAsync(KnownNames.Type).ConfigureAwait(false) != KnownNames.ObjStm)
            return Array.Empty<ObjectLocation>();
        return await this.GetIncludedObjectNumbersAsync().ConfigureAwait(false);
    }
}