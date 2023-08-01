﻿using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Objects;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

namespace Melville.Pdf.Model.Renderers.FontRenderings.GlyphMappings;

internal class UnicodeGlyphNameMapper : FontRenderings.GlyphMappings.DictionaryGlyphNameMapper
{
    public UnicodeGlyphNameMapper(IReadOnlyDictionary<uint, uint> mappings) : base(mappings)
    {
    }

    protected override uint HashForString(byte[] name) => 
        GlyphNameToUnicodeMap.AdobeGlyphList.TryMap(name, out var unicode) ? (uint)unicode : 0;
    protected override uint HashForString(PdfDirectValue name) => 
        GlyphNameToUnicodeMap.AdobeGlyphList.TryMap(name, out var unicode) ? (uint)unicode : 0;
}

internal class UnicodeViaMacMapper : UnicodeGlyphNameMapper
{
    public UnicodeViaMacMapper(IReadOnlyDictionary<uint, uint> mappings) : base(mappings)
    {
    }

    protected override uint HashForString(byte[] name)
    {
        var unicode = base.HashForString(name);
        return MapUnicodeToMacRoman(unicode);
    }

    private uint MapUnicodeToMacRoman(uint input) => (input switch
    {
        // Mappings from the MacRoman encoding on poge 653 + of the pdf spec
        0xC4 => 0x80,
        0xC5 => 0x81,
        0xC7 => 0x82,
        0xC9 => 0x83,
        0xD1 => 0x84,
        0xD6 => 0x85,
        0xDC => 0x86,
        0xE1 => 0x87,
        0xE0 => 0x88,
        0xE2 => 0x89,
        0xE4 => 0x8A,
        0xE3 => 0x8B,
        0xE5 => 0x8C,
        0xE7 => 0x8D,
        0xE9 => 0x8E,
        0xE8 => 0x8F,
        0xEA => 0x90,
        0xEB => 0x91,
        0xED => 0x92,
        0xEC => 0x93,
        0xEE => 0x94,
        0xEF => 0x95,
        0xF1 => 0x96,
        0xF3 => 0x97,
        0xF2 => 0x98,
        0xF4 => 0x99,
        0xF6 => 0x9A,
        0xF5 => 0x9B,
        0xFA => 0x9C,
        0xF9 => 0x9D,
        0xFB => 0x9E,
        0xFC => 0x9F,
        0x2020 => 0xA0,
        0xB0 => 0xA1,
        0xA7 => 0xA4,
        0x2022 => 0xA5,
        0xB6 => 0xA6,
        0xDF => 0xA7,
        0xAE => 0xA8,
        0x2122 => 0xAA,
        0xB4 => 0xAB,
        0xA8 => 0xAC,
        0xC6 => 0xAE,
        0xD8 => 0xAF,
        0xA5 => 0xB4,
        0xAA => 0xBB,
        0xBA => 0xBC,
        0xE6 => 0xBE,
        0xF8 => 0xBF,
        0xBF => 0xC0,
        0xA1 => 0xC1,
        0xAC => 0xC2,
        0x192 => 0xC4,
        0xAB => 0xC7,
        0xBB => 0xC8,
        0x2026 => 0xC9,
        0xC0 => 0xCB,
        0xC3 => 0xCC,
        0xD5 => 0xCD,
        0x152 => 0xCE,
        0x153 => 0xCF,
        0x2013 => 0xD0,
        0x2014 => 0xD1,
        0x201C => 0xD2,
        0x201D => 0xD3,
        0x2018 => 0xD4,
        0x2019 => 0xD5,
        0xF7 => 0xD6,
        0x25CF => 0xD7,
        0xFF => 0xD8,
        0x178 => 0xD9,
        0x2044 => 0xDA,
        0xA4 => 0xDB,
        0x2039 => 0xDC,
        0x203A => 0xDD,
        0xFB01 => 0xDE,
        0xFB02 => 0xDF,
        0x2021 => 0xE0,
        0xB7 => 0xE1,
        0x201A => 0xE2,
        0x201E => 0xE3,
        0x2030 => 0xE4,
        0xC2 => 0xE5,
        0xCA => 0xE6,
        0xC1 => 0xE7,
        0xCB => 0xE8,
        0xC8 => 0xE9,
        0xCD => 0xEA,
        0xCE => 0xEB,
        0xCF => 0xEC,
        0xCC => 0xED,
        0xD3 => 0xEE,
        0xD4 => 0xEF,
        0xD2 => 0xF1,
        0xDA => 0xF2,
        0xDB => 0xF3,
        0xD9 => 0xF4,
        0x131 => 0xF5,
        0x2C6 => 0xF6,
        0x2DC => 0xF7,
        0xAF => 0xF8,
        0x2D8 => 0xF9,
        0x2D9 => 0xFA,
        0x2DA => 0xFB,
        0xB8 => 0xFC,
        0x2DD => 0xFD,
        0x2DB => 0xFE,
        0x2C7 => 0xFF,

        // Mappings from Table 115 on Page 266 of the PDF spec
        0x2260 => 0xAD,
        0x221E => 0xB0,
        0x2264 => 0xB2,
        0x2265 => 0xB3,
        0x2202 => 0xB6,
        0x2211 => 0xB7,
        0x220F => 0xB8,
        0x3A0 => 0xB9,
        0x3C0 => 0xB9,
        0x222B => 0xBA,
        0x2216 => 0xBD,
        0x3C9 => 0xBD,
        0x221A => 0xC3,
        0x2248 => 0xC5,
        0x2206 => 0xC6,
        0x25CA => 0xD7,
        0x20AC => 0xDB,
        0xF8FF => 0xF0,
        _ => input
    });

}