﻿using System.IO;
using Melville.INPC;
using Melville.MVVM.WaitingServices;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;
using Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.References;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer;

public partial class LowLevelViewModel
{
    private readonly IPasswordSource passwordSource;
    [AutoNotify] private ParsedLowLevelDocument? parsedDoc;
    [AutoNotify] public DocumentPart[]? Root => ParsedDoc?.Root;
    [AutoNotify] private DocumentPart? selected;
    private IWaitingService? waiter;
 
    public LowLevelViewModel(IPasswordSource passwordSource)
    {
        this.passwordSource = passwordSource;
    }

    public void SetVisualTreeRunner(IWaitingService waiter)
    {
        this.waiter = waiter;
    }

    public async void SetStream(Stream source)
    {
        ParsedDoc = await TryParse(source);
    }

    private async Task<ParsedLowLevelDocument> TryParse(Stream source)
    {
        try
        {
            return await (await CreateParser()).ParseAsync(source, waiter ?? new FakeWaitingService());
        }
        catch (Exception e)
        {
            return new ParsedLowLevelDocument(
                new[] { new DocumentPart($"Exception: {e.Message}") },
                NoPageLookup.Instance);
        }
    }

    private async Task<PartParser> CreateParser()
    {
        var (password, passwordType) = await passwordSource.GetPasswordAsync();
        return new PartParser(new ConstantPasswordSource(passwordType, password));
    }

    public ValueTask JumpToReference(ReferencePartViewModel target, IWaitingService waiting)
    {
        return JumpToReference2(target.RefersTo, waiting);
    }

    private async ValueTask JumpToReference2(CrossReference reference, IWaitingService waiting)
    {
        if (Root is null) return;
        if (Selected is not null &&
            (targetHistory.Count == 0 || targetHistory.Peek() != Selected)) targetHistory.Push(Selected);
        Selected = (await new DocumentPartSearcher(reference, waiting)
            .FindAsync(Root)) ?? Selected;
    }

    public async void JumpTOPage(int page)
    {
        if (ParsedDoc is null) return;
        var reference = await ParsedDoc.Pages.PageForNumber(page);
        if (reference.Object == 0) return;
        await JumpToReference2(reference, new FakeWaitingService());
    }

    private readonly Stack<DocumentPart> targetHistory = new();

    public void NavigateBack()
    {
        if (targetHistory.TryPop(out var sel))
        {
            sel.Selected = true;
            Selected = sel;
        }
    }
}