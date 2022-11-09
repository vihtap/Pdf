﻿using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Text;

public class Type3FontWithOwnResource: FontDefinitionTest
{
    public Type3FontWithOwnResource() : base("Render a type 3 font with its own resources")
    {
        TextToRender = "abaabb";
    }
    
    private static PdfDictionary LineStyleDict()
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.LW, 15)
            .WithItem(KnownNames.D,
                new PdfArray(new PdfArray(30), new PdfInteger(0)))
            .AsDictionary();
    }

    protected override PdfObject CreateFont(ILowLevelDocumentCreator arg)
    {
        var triangle = arg.Add(new DictionaryBuilder().AsStream(@"
/GS1 gs
1000 0 0 0 750 750 d1
0 0 m
375 750 l
750 0 l
s
"));
        var square = arg.Add(new DictionaryBuilder().AsStream(@"
1000 0 0 0 750 750 d1
0 0 750 750 re
s"));
        var triName = NameDirectory.Get("triangle");
        var sqName = NameDirectory.Get("square");
        var chanProcs = arg.Add(new DictionaryBuilder()
            .WithItem(sqName, square)
            .WithItem(triName, triangle)
            .AsDictionary()
        );

        var encoding = arg.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Encoding)
            .WithItem(KnownNames.Differences, new PdfArray(new PdfInteger(97), sqName, triName))
            .AsDictionary()
        );
        
        return new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Font)
            .WithItem(KnownNames.Subtype, KnownNames.Type3)
            .WithItem(KnownNames.FontBBox, new PdfArray(
                new PdfInteger(0),
                new PdfInteger(0),
                new PdfInteger(750),
                new PdfInteger(750)
            ))
            .WithItem(KnownNames.FontMatrix, new PdfArray(
                new PdfDouble(0.001),
                new PdfDouble(0),
                new PdfDouble(0),
                new PdfDouble(0.001),
                new PdfDouble(0),
                new PdfDouble(0)
            ))
            .WithItem(KnownNames.CharProcs, chanProcs)
            .WithItem(KnownNames.Encoding, encoding)
            .WithItem(KnownNames.FirstChar, new PdfInteger(97))
            .WithItem(KnownNames.LastChar, new PdfInteger(98))
            .WithItem(KnownNames.Widths, new PdfArray(new PdfInteger(1000), new PdfInteger(1000)))
            .WithItem(KnownNames.Resources,
                new DictionaryBuilder()
                    .WithItem(KnownNames.ExtGState, new DictionaryBuilder()
                        .WithItem(NameDirectory.Get("GS1"), LineStyleDict()).AsDictionary()).AsDictionary())
            .AsDictionary();
    }
}