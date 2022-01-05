﻿using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Writers.ContentStreams;
using Melville.Pdf.ReferenceDocuments.Graphics;

namespace Melville.Pdf.ReferenceDocuments.Text;

public abstract class TextAttributeTest : Card3x5
{
    protected TextAttributeTest(string helpText) : base (helpText)
    {
    }

    private static readonly PdfName fontName = NameDirectory.Get("F1"); 
    protected override void SetPageProperties(PageCreator page)
    {
        page.AddStandardFont(fontName, BuiltInFontName.Courier, FontEncodingName.StandardEncoding);
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        using (var tr = csw.StartTextBlock())
        {
            await csw.SetStrokeRGB(1.0, 0.0, 0.0);
            await csw.SetFont(fontName, 70);
            tr.SetTextMatrix(1,0,0,1,30,25);
            tr.ShowString("Is Text");
            SetTestedParameter(csw);
            tr.SetTextMatrix(1,0,0,1,30,125);
            tr.ShowString("Is Text");
        }
    }

    protected abstract void SetTestedParameter(ContentStreamWriter csw);
}

public abstract class ClippingTextAttributeTest : TextAttributeTest
{
    protected ClippingTextAttributeTest(string helpText) : base(helpText)
    {
    }

    protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
    {
        await base.DoPaintingAsync(csw);
        await csw.SetNonstrokingRGB(0, 1, 0);
        csw.Rectangle(10,130, 300, 20);
        csw.FillPath();
    }
}

public class CharacterSpacing: TextAttributeTest
{
    public CharacterSpacing() : base("Set the Character Spacing")
    {
    }

    protected override void SetTestedParameter(ContentStreamWriter csw)
    {
        csw.SetCharSpace(30);
    }
}
public class WordSpacing: TextAttributeTest
{
    public WordSpacing() : base("Set the Word Spacing")
    {
    }

    protected override void SetTestedParameter(ContentStreamWriter csw)
    {
        csw.SetWordSpace(50);
        csw.SetCharSpace(-10);
    }
}

public class HorizontalScaling : TextAttributeTest
{
    public HorizontalScaling() : base("Demonstrate horizontal scaling.")
    {
    }

    protected override void SetTestedParameter(ContentStreamWriter csw) => csw.SetHorizontalTextScaling(50);
}

public class StrokeText : TextAttributeTest
{
    public StrokeText() : base("Show outline of text")
    {
    }

    protected override void SetTestedParameter(ContentStreamWriter csw) => csw.SetTextRender(TextRendering.Stroke);
}

public class StrokeAndFillText : TextAttributeTest
{
    public StrokeAndFillText() : base("Show Filled outline of text")
    {
    }

    protected override void SetTestedParameter(ContentStreamWriter csw) => 
        csw.SetTextRender(TextRendering.FillAndStroke);
}

public class InvisibleText : TextAttributeTest
{
    public InvisibleText() : base("\"Show\" invisible text")
    {
    }

    protected override void SetTestedParameter(ContentStreamWriter csw) => 
        csw.SetTextRender(TextRendering.Invisible);
}

public class StrokeAndClip : ClippingTextAttributeTest
{
    public StrokeAndClip() : base("Stroked text as a clipping region.")
    {
    }

    protected override void SetTestedParameter(ContentStreamWriter csw) => 
        csw.SetTextRender(TextRendering.StrokeAndClip);
}