﻿using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils;

public class TestWriter
{
    private readonly MultiBufferStream target = new();
    public PipeWriter Writer { get; }

    public TestWriter()
    {
        Writer = PipeWriter.Create(target);
    }

    public string Result() => ExtendedAsciiEncoding.ExtendedAsciiString(
        target.CreateReader().ReadToArray());
}

public static class TestWriterOperations
{
    public static async ValueTask<string> WriteToStringAsync(this PdfIndirectValue obj)
    {
        var writer = new TestWriter();
        var objWriter = new PdfObjectWriter(writer.Writer);
        objWriter.Write(obj);
        return writer.Result();
    }
}