﻿using System;
using System.IO;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils
{
    public static class BuilderShortcuts
    {
        public static async Task<byte[]> AsBytes(this ILowLevelDocumentBuilder builder)
        {
            var doc = builder.CreateDocument();
            var output = new MemoryStream();
            await doc.WriteTo(output);
            return output.ToArray();
        }

        public static async Task<Stream> AsStream(this ILowLevelDocumentBuilder builder) =>
            new MemoryStream(await builder.AsBytes());
        public static async Task<IFile> AsFile(this ILowLevelDocumentBuilder builder) =>
            new MemoryFile("S:\\d.pdf", await builder.AsBytes());
        public static async Task<String> AsString(this ILowLevelDocumentBuilder builder) =>
            ExtendedAsciiEncoding.ExtendedAsciiString(await builder.AsBytes());
    }
}