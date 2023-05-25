﻿using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption;

public class S7_6_5CryptFilters
{
    private async Task VerifyStringAndStreamEncodingAsync(bool hideStream, bool hideString,
        ILowLevelDocumentCreator creator, PdfName? cryptFilterTypeForStream = null)
    {
        creator.Add(PdfString.CreateAscii("plaintext string"));
        creator.Add(InsertedStream(creator, cryptFilterTypeForStream));
        var str = await creator.AsStringAsync();
        Assert.Equal(!hideString, str.Contains("plaintext string"));
        Assert.Equal(!hideStream, str.Contains("plaintext stream"));
        var doc = await str.ParseDocumentAsync();
        Assert.Equal(3, doc.Objects.Count);
        Assert.Equal("plaintext string", (await doc.Objects[(2, 0)].DirectValueAsync()).ToString());
        Assert.Equal("plaintext stream", await (
                await ((PdfStream)(
                    await doc.Objects[(3, 0)].DirectValueAsync().CA())).StreamContentAsync())
            .ReadAsStringAsync());
    }

    private PdfStream InsertedStream(
        IPdfObjectRegistry creator, PdfName? cryptFilterTypeForStream)
    {
        var builder = cryptFilterTypeForStream == null ?
            new DictionaryBuilder():
            EncryptedStreamBuilder(cryptFilterTypeForStream);

        return builder.AsStream("plaintext stream");
    }

    private static DictionaryBuilder EncryptedStreamBuilder(PdfName cryptFilterTypeForStream)
    {
        return new DictionaryBuilder()
            .WithFilter(FilterName.Crypt)
            .WithFilterParam(new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.CryptFilterDecodeParms)
                .WithItem(KnownNames.Name, cryptFilterTypeForStream).AsDictionary());
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task CanUseDefaultStringsInAsync(bool hideStream, bool hideString)
    {
        var creator = LowLevelDocumentBuilderFactory.New();
        creator.AddEncryption(DocumentEncryptorFactory.V4("","", PdfPermission.None,
            Encoder(hideStream), Encoder(hideString), Encoder(hideStream), new V4CfDictionary(KnownNames.V2, 16)));
        await VerifyStringAndStreamEncodingAsync(hideStream, hideString, creator);
    }

    [Fact]
    public Task UseIdentityCryptFilterToGEtOutOfStreamEncryptionAsync()
    {
        var creator = LowLevelDocumentBuilderFactory.New();
        creator.AddEncryption(DocumentEncryptorFactory.V4("","", PdfPermission.None,
            KnownNames.StdCF, KnownNames.StdCF, KnownNames.StmF, new V4CfDictionary(KnownNames.V2, 16)));
        return VerifyStringAndStreamEncodingAsync(false, true, creator, KnownNames.Identity);
    }

    [Fact]
    public Task UseCryptFilterToOptIntoEncryptionAsync()
    {
        var creator = LowLevelDocumentBuilderFactory.New();
        creator.AddEncryption(DocumentEncryptorFactory.V4("","", PdfPermission.None,
            KnownNames.Identity, KnownNames.StdCF, KnownNames.StmF, new V4CfDictionary(KnownNames.V2, 16)));
        return VerifyStringAndStreamEncodingAsync(true, true, creator, KnownNames.StdCF);
    }

    [Fact]
    public  Task StreamsWithoutCryptFilterGetDefaultEncryptionAsync()
    {
        var creator = LowLevelDocumentBuilderFactory.New();
        creator.AddEncryption(DocumentEncryptorFactory.V4("","", PdfPermission.None,
            Encoder(true), Encoder(true),Encoder(true), new V4CfDictionary(KnownNames.None, 16)));
        return VerifyStringAndStreamEncodingAsync(false, false, creator);
            
    }
        
    private static PdfName Encoder(bool hideString) => 
        hideString?KnownNames.StdCF:KnownNames.Identity;
}