﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Pdf.KnownNamesGenerator.CharacterEncodings;

public static class EncodingGeneratorFactory
{
    public static EncodingGenerator Create(ImmutableArray<AdditionalText> texts)
    {
        return new EncodingGenerator(GlyphNameParser.Parse(Inputfile(texts, "glyphlist.cedsl")),
            new MultiEncodingMaps(Inputfile(texts, "stdEncodings.cedsl")),
            SimpleMapParser.Parse(Inputfile(texts, "Symbol.cedsl")));
    }

    private static string Inputfile(ImmutableArray<AdditionalText> texts, string name) => 
        texts.First(i => i.Path.EndsWith(name)).GetText()!.ToString();
}

public class EncodingGenerator
{
    private IDictionary<string, string> glyphNameToUnicode;
    private MultiEncodingMaps maps;
    private IReadOnlyDictionary<byte, string> symbolMap;
    public EncodingGenerator(IDictionary<string, string> glyphNameToUnicode, 
        MultiEncodingMaps maps, IReadOnlyDictionary<byte, string> symbolMap)
    {
        this.glyphNameToUnicode = glyphNameToUnicode;
        this.maps = maps;
        this.symbolMap = symbolMap;
    }

    public string Generate()
    {
        var sb = new StringBuilder();
        GenerateAllEncodings(sb);
        var text = sb.ToString();
        UdpConsole.WriteLine(text.Substring(0,1000));
        return text;
    }

    private void GenerateAllEncodings(StringBuilder sb)
    {
        GeneratePreamble(sb);
        GenerateEncoding(sb,"Symbol", symbolMap);
        GenerateEncoding(sb,"Standard",maps.Standard);
        GenerateEncoding(sb,"WinAnsi",maps.Win);
        CloseClassBlock(sb);
    }

    private void GeneratePreamble(StringBuilder sb)
    {
        sb.AppendLine("#nullable enable");
        sb.AppendLine("namespace Melville.Pdf.LowLevel.Model.CharacterEncoding;");
        sb.AppendLine();
        sb.AppendLine("public static partial class CharacterEncodings {");
    }

    private void CloseClassBlock(StringBuilder sb)
    {
        sb.AppendLine("}");
    }

    private void GenerateEncoding(StringBuilder sb, string name, IReadOnlyDictionary<byte, string> map)
    {
        sb.AppendLine($"    public static IByteToUnicodeMapping {name} = new TableMapping(new char[] {{");
        sb.Append("        ");
        for (int i = 0; i < 256; i++)
        {
            var value = map.TryGetValue((byte)i, out var mappedName) &&
                        glyphNameToUnicode.TryGetValue(mappedName, out var uni)
                ? uni
                : "25CF";
            sb.Append($" (char)(0x{value}),");
        }
        sb.AppendLine("    });");
    }
}

public static class UdpConsole
{
    private static UdpClient? client = null;
    private static UdpClient Client
    {
        get
        {
            client ??= new UdpClient();
            return client ;
        }
    }

    public static string WriteLine(string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        Client.Send(bytes, bytes.Length, "127.0.0.1", 15321);
        return str;
    }
}