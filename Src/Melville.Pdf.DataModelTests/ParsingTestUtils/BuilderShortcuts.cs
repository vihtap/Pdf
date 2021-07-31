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
        public static async Task<byte[]> AsBytesAsync(this ILowLevelDocumentCreator creator)
        {
            var doc = creator.CreateDocument();
            var output = new MemoryStream();
            await doc.WriteTo(output);
            return output.ToArray();
        }

        public static async Task<Stream> AsStreamAsync(this ILowLevelDocumentCreator creator) =>
            new MemoryStream(await creator.AsBytesAsync());
        public static async Task<IFile> AsFileAsync(this ILowLevelDocumentCreator creator) =>
            new MemoryFile("S:\\d.pdf", await creator.AsBytesAsync());
        public static async Task<String> AsStringAsync(this ILowLevelDocumentCreator creator) =>
            ExtendedAsciiEncoding.ExtendedAsciiString(await creator.AsBytesAsync());
    }
}