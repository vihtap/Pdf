﻿using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Visitors;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers;
using Melville.Pdf.Model.Renderers.FontRenderings.DefaultFonts;
using Melville.Pdf.SkiaSharp;

namespace Melville.Pdf.FuzzTest;

public static class ParseFile
{
    public static async ValueTask Do(string fileName)
    {
        await using var stream = File.OpenRead(fileName);
        await Do(stream, fileName);
    }

    private static async ValueTask Do(FileStream source, string name)
    {
        try
        {
            using var doc = await DocumentRendererFactory.CreateRendererAsync(
                await PdfDocument.ReadAsync(source), WindowsDefaultFonts.Instance);
            for (int i = 0; i < doc.TotalPages; i++)
            {
                Console.Write($"{name} {i+1}/{doc.TotalPages}");
                await RenderPage(doc, i);
                ClearLine();
            }
        }
        catch (Exception e)
        {
            OutputException(e);
        }
    }

    private static async Task RenderPage(DocumentRenderer doc, int page)
    {
        try
        {
            await RenderWithSkia.ToSurfaceAsync(doc, page);
        }
        catch (Exception e)
        {
            OutputException(e);
        }
    }
    static void ClearLine(){
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth)); 
        Console.SetCursorPosition(0, Console.CursorTop - 1);
    }


    private static void OutputException(Exception e)
    {
        Console.WriteLine();
        Console.WriteLine("    " +e.Message);
    }
}