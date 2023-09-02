﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Pdf.Model.Renderers.FontRenderings.CMaps;
using Melville.SharpFont;
using Xunit;
using Encoding = System.Text.Encoding;

namespace Melville.Pdf.DataModelTests.CmapParsers
{
    public class ParsedCMaps: IAsyncLifetime
    {
        public IReadCharacter[] Maps { get; } = new IReadCharacter[2];

        public async Task InitializeAsync()
        {
            Maps[0] = await ParseMapAsync(CMapFromCmapSpec);
            Maps[1] = await ParseMapAsync(UnicodeCMapFromSpec);
        }

        public Task DisposeAsync() => Task.CompletedTask;

        private async ValueTask<IReadCharacter> ParseMapAsync(string text)
        {
            return await CMapParser.ParseCMapAsync(
                new MemoryStream(Encoding.UTF8.GetBytes(text)),
                GlyphNameToUnicodeMap.AdobeGlyphList, TwoByteCharacters.Instance);
        }

        private const string CMapFromCmapSpec =
            """
            %!PS-Adobe-3.0 Resource-CMap
            %%DocumentNeededResources: ProcSet CIDInit
            %%IncludeResource: ProcSet CIDInit
            %%BeginResource: CMap (CJKTypeBlogTest-UTF32-H)
            %%Title: (CJKTypeBlogTest-UTF32-H Adobe Identity 0)
            %%Version: 1.000
            %%EndComments

            /CIDInit /ProcSet findresource begin

            12 dict begin

            begincmap

            /CIDSystemInfo 3 dict dup begin
              /Registry (Adobe) def
              /Ordering (Identity) def
              /Supplement 0 def
            end def

            /CMapName /CJKTypeBlogTest-UTF32-H def
            /CMapVersion 1.000 def
            /CMapType 1 def

            /WMode 0 def

            /UUIDOffset 0 def
            /XUID [1 10 25324] def


            1 begincodespacerange
              <00> <80>
              <8140> <9ffc>
              <a0> <df>
              <a040> <fbfc>
            endcodespacerange

            2 beginnotdefrange
            <00> <1f> 0
            <8148> <9FFC> 16#8149
            endnotdefrange

            3 begincidrange
            <20> <7E> 1

            endcidrange
            1 begincidchar
            <7F> 15
            endcidchar

            endcmap
            CMapName currentdict /CMap defineresource pop
            end
            end

            %%EndResource
            %%EOF
            """;

        //This comes from PDF 2.0 Spec section 9.10.2
        private const string UnicodeCMapFromSpec = """
           /CIDInit /ProcSet findresource begin
           12 dict begin
           begincmap
           /CIDSystemInfo
           <</Registry (Adobe)
           /Ordering (UCS2)
           /Supplement 0
           >> def
           /CMapName /Adobe-Identity-UCS2 def
           /CMapType 2 def
           1 begincodespacerange
           <0000> <FFFF>
           endcodespacerange
           2 beginbfrange
           <0000> <005E> <12340020>
           <005F> <0061> [<00660066> <00660069> <00660066006C>]
           endbfrange
           1 beginbfchar
           <3A51> <D840DC3E>
           %0397;Eta;GREEK CAPITAL LETTER ETA
           <3A52> /Eta
           endbfchar
           endcmap
           CMapName currentdict /CMap defineresource pop
           end
           end
           """;
    }
}