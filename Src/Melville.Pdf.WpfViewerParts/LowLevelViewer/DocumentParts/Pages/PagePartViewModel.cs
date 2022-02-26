﻿using System.Windows.Media;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.DocumentRenderers;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.Wpf;

namespace Melville.Pdf.WpfViewerParts.LowLevelViewer.DocumentParts.Pages;

public partial class PagePartViewModel: DocumentPart
{
    [AutoNotify]private ImageSource? renderedPage;
    private readonly PdfPage page;
    
    private ImageSource? RenderedPageGetFilter(ImageSource? value)
    {
        if (value == null) RenderPage();
        return value;
    }

    private async void RenderPage()
    {
        try
        {
            var drawingGroup = await new RenderToDrawingGroup().Render(
                DocumentRendererFactory.CreateRenderer(page, new WindowsDefaultFonts()), 0);
            drawingGroup.Freeze();
            var image = new DrawingImage(drawingGroup);
            image.Freeze();
            RenderedPage =image;
        }
        catch (Exception )
        {
        }
    }


    public override object? DetailView => this;

    public PagePartViewModel(string title, IReadOnlyList<DocumentPart> children, PdfPage page) : base(title, children)
    {
        this.page = page;
    }
}