﻿using System.IO;
using Melville.FileSystem;
using Melville.MVVM.WaitingServices;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.WpfViewerParts.LowLevelViewer.DocumentParts;

public interface IPartParser
{
    Task<DocumentPart[]> ParseAsync(IFile source, IWaitingService waiting);
    public Task<DocumentPart[]> ParseAsync(Stream source, IWaitingService waiting);
}
public class PartParser: IPartParser
{
    private ViewModelVisitor generator = new();
    private List<DocumentPart> items = new();
    private readonly IPasswordSource passwordSource;

    public PartParser(IPasswordSource passwordSource)
    {
        this.passwordSource = passwordSource;
    }

    public async Task<DocumentPart[]> ParseAsync(IFile source, IWaitingService waiting) => 
        await ParseAsync(await source.OpenRead(), waiting);

    public async Task<DocumentPart[]> ParseAsync(Stream source, IWaitingService waiting)
    {
        items.Clear();
        PdfLowLevelDocument lowlevel = await RandomAccessFileParser.Parse(
            new ParsingFileOwner(source, passwordSource));
        GenerateHeaderElement(lowlevel);
        using var waitHandle = waiting.WaitBlock("Loading File", lowlevel.Objects.Count);
        foreach (var item in lowlevel.Objects.Values
                     .OrderBy(i=>i.Target.ObjectNumber)
                     .ToList())
        {
            waiting.MakeProgress($"Loading Object ({item.Target.ObjectNumber}, {item.Target.GenerationNumber})");
            items.Add(await item.Target.Visit(generator));
        }
        items.Add(await generator.GeneratePart("Trailer: ", lowlevel.TrailerDictionary));
        return items.ToArray();
    }

    private void GenerateHeaderElement(PdfLowLevelDocument lowlevel) =>
        items.Add(new DocumentPart($"PDF-{lowlevel.MajorVersion}.{lowlevel.MinorVersion}"));
}