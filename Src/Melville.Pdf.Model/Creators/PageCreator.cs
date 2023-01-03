﻿using System;
using System.Collections.Generic;
using System.Linq;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;

public abstract class ContentStreamCreator: ItemWithResourceDictionaryCreator
{
    private readonly IObjectStreamCreationStrategy objStreamStrategy;
    
    protected ContentStreamCreator(IObjectStreamCreationStrategy objStreamStrategy): base(new())
    {
        this.objStreamStrategy = objStreamStrategy;
    }

    public override (PdfIndirectObject Reference, int PageCount) ConstructPageTree(IPdfObjectRegistry creator,
        PdfIndirectObject? parent, int maxNodeSize)
    {
        using var _ = objStreamStrategy.EnterObjectStreamContext(creator);
        TryAddResources(creator);
        return (CreateFinalObject(creator), 1);
    }

    protected abstract PdfIndirectObject CreateFinalObject(IPdfObjectRegistry creator);
    public abstract void AddToContentStream(DictionaryBuilder builder, MultiBufferStreamSource data);

}

public class PageCreator: ContentStreamCreator
{
    private readonly List<PdfStream> streamSegments = new();
    private PromisedIndirectObject? promisedPageObject;
    public PageCreator(IObjectStreamCreationStrategy objStreamStrategy) : base(objStreamStrategy)
    {
        MetaData.WithItem(KnownNames.Type, KnownNames.Page);
    }

    public override void AddToContentStream(DictionaryBuilder builder, MultiBufferStreamSource data) => 
        streamSegments.Add(builder.AsStream(data));

    public override (PdfIndirectObject Reference, int PageCount) 
        ConstructPageTree(IPdfObjectRegistry creator, PdfIndirectObject? parent,
            int maxNodeSize)
    {
        if (parent is null) throw new ArgumentException("Pages must have a parent.");
        MetaData.WithItem(KnownNames.Parent, parent);
        return base.ConstructPageTree(creator, parent, maxNodeSize);
    }

    protected override PdfIndirectObject CreateFinalObject(IPdfObjectRegistry creator)
    {
        TryAddContent(creator);
        return creator.Add(TryUsePromisedObject(MetaData.AsDictionary()));
    }

    public PdfIndirectObject InitializePromiseObject(IPdfObjectRegistry builder) =>
        promisedPageObject = builder.CreatePromiseObject();

    public PdfObject TryUsePromisedObject(PdfObject value)
    {
        if (promisedPageObject == null) return value;
        promisedPageObject.SetValue(value);
        return promisedPageObject;
    }

    private void TryAddContent(IPdfObjectRegistry creator)
    {
        if (streamSegments.Count > 0)
            MetaData.WithItem(KnownNames.Contents, CreateContents(creator));
    }

    private PdfObject CreateContents(IPdfObjectRegistry creator) =>
        streamSegments.Count == 1
            ? CreateStreamSegment(creator, streamSegments[0])
            : new PdfArray(streamSegments.Select(i => CreateStreamSegment(creator, i)));

    private PdfIndirectObject CreateStreamSegment(IPdfObjectRegistry creator, PdfStream stream) => 
        creator.Add(stream);

    public void AddLastModifiedTime(PdfTime dateAndTime) => 
        MetaData.WithItem(KnownNames.LastModified, PdfString.CreateDate(dateAndTime));
}