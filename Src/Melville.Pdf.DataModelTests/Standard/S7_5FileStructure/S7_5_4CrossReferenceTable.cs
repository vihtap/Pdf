﻿using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Postscript.Interpreter.Tokenizers;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_5FileStructure;

public class S7_5_4CrossReferenceTable
{

    private readonly Mock<IIndirectObjectRegistry> resolver = new();

    private async ValueTask<long> ReadTable(string data)
    {
        var reader = new MemoryWrapper(data.AsExtendedAsciiBytes());
        var parser = new CrossReferenceTableParser(reader, resolver.Object);
        await parser.ParseAsync();
        return reader.GlobalPosition;
    }

    [Fact]
    public async Task ParseSimpleTableAsync()
    {
        await ReadTable("""
            0 6
            0000000003 65535 f
            0000000017 00000 n
            0000000081 00000 n
            0000000000 00007 f
            0000000331 00122 n
            0000000409 00000 n            
            trailer
            """);
        resolver.Verify(i=>i.RegisterDeletedBlock(0, 3, 65535));
        resolver.Verify(i => i.RegisterIndirectBlock(1, 0, 17));
        resolver.Verify(i => i.RegisterIndirectBlock(2, 0, 81));
        resolver.Verify(i=>i.RegisterDeletedBlock(3, 0, 7));
        resolver.Verify(i => i.RegisterIndirectBlock(4, 122, 331));
        resolver.Verify(i => i.RegisterIndirectBlock(5, 000, 409));
        resolver.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ParseCompoundTableAsync()
    {
        await ReadTable("""
            0 4
            0000000003 65535 f
            0000000017 00000 n
            0000000081 00000 n
            0000000000 00007 f
            23 2
            0000000331 00122 n
            0000000409 00000 n
            trailer
            """);
        resolver.Verify(i=>i.RegisterDeletedBlock(0, 3, 65535));
        resolver.Verify(i => i.RegisterIndirectBlock(1, 0, 17));
        resolver.Verify(i => i.RegisterIndirectBlock(2, 0, 81));
        resolver.Verify(i=>i.RegisterDeletedBlock(3, 0, 7));
        resolver.Verify(i => i.RegisterIndirectBlock(23, 122, 331));
        resolver.Verify(i => i.RegisterIndirectBlock(24, 000, 409));
        resolver.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ParseZeroTableAsync()
    {
        await ReadTable("""
            0 0
            trailer
            """);
        resolver.VerifyNoOtherCalls();
    }
}