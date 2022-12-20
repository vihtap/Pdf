﻿namespace Melville.CCITT;

internal static class HorizontalSpanEncoder
{
    public static bool Write(ref BitTarget target, bool firstRunWhite, int firstRun, int secondRun)
    {
        return secondRun < 0?
            WriteRun(ref target, firstRunWhite, firstRun):
            WriteTwoRunSequence(ref target, firstRunWhite, firstRun, secondRun);
    }

    private static bool WriteTwoRunSequence(ref BitTarget target, bool firstRunWhite, int firstRun, int secondRun)
    {
        return target.TryWriteBits(0b001, 3) &&
               WriteRun(ref target, firstRunWhite, firstRun) &&
               WriteRun(ref target, !firstRunWhite, secondRun);
    }

    private static bool WriteRun(ref BitTarget target, bool whiteRun, int length) =>
        (whiteRun ? whiteEncoder : blackEncoder).WriteRun(ref target, length);

    private static HorizontalRunEncoder whiteEncoder = new(new (byte, uint)[]
        {
            (8, 0b00110101),
            (6, 0b000111),
            (4, 0b0111),
            (4, 0b1000),
            (4, 0b1011),
            (4, 0b1100),
            (4, 0b1110),
            (4, 0b1111),
            (5, 0b10011),
            (5, 0b10100),
            (5, 0b00111),
            (5, 0b01000),
            (6, 0b001000),
            (6, 0b000011),
            (6, 0b110100),
            (6, 0b110101),
            (6, 0b101010),
            (6, 0b101011),
            (7, 0b0100111),
            (7, 0b0001100),
            (7, 0b0001000),
            (7, 0b0010111),
            (7, 0b0000011),
            (7, 0b0000100),
            (7, 0b0101000),
            (7, 0b0101011),
            (7, 0b0010011),
            (7, 0b0100100),
            (7, 0b0011000),
            (8, 0b00000010),
            (8, 0b00000011),
            (8, 0b00011010),
            (8, 0b00011011),
            (8, 0b00010010),
            (8, 0b00010011),
            (8, 0b00010100),
            (8, 0b00010101),
            (8, 0b00010110),
            (8, 0b00010111),
            (8, 0b00101000),
            (8, 0b00101001),
            (8, 0b00101010),
            (8, 0b00101011),
            (8, 0b00101100),
            (8, 0b00101101),
            (8, 0b00000100),
            (8, 0b00000101),
            (8, 0b00001010),
            (8, 0b00001011),
            (8, 0b01010010),
            (8, 0b01010011),
            (8, 0b01010100),
            (8, 0b01010101),
            (8, 0b00100100),
            (8, 0b00100101),
            (8, 0b01011000),
            (8, 0b01011001),
            (8, 0b01011010),
            (8, 0b01011011),
            (8, 0b01001010),
            (8, 0b01001011),
            (8, 0b00110010),
            (8, 0b00110011),
            (8, 0b00110100)
        },
        new (byte, uint)[]
        {
            (5, 0b11011),
            (5, 0b10010),
            (6, 0b010111),
            (7, 0b0110111),
            (8, 0b00110110),
            (8, 0b00110111),
            (8, 0b01100100),
            (8, 0b01100101),
            (8, 0b01101000),
            (8, 0b01100111),
            (9, 0b011001100),
            (9, 0b011001101),
            (9, 0b011010010),
            (9, 0b011010011),
            (9, 0b011010100),
            (9, 0b011010101),
            (9, 0b011010110),
            (9, 0b011010111),
            (9, 0b011011000),
            (9, 0b011011001),
            (9, 0b011011010),
            (9, 0b011011011),
            (9, 0b010011000),
            (9, 0b010011001),
            (9, 0b010011010),
            (6, 0b011000),
            (9, 0b010011011),
            (11, 0b00000001000),
            (11, 0b00000001100),
            (11, 0b00000001101),
            (12, 0b000000010010),
            (12, 0b000000010011),
            (12, 0b000000010100),
            (12, 0b000000010101),
            (12, 0b000000010110),
            (12, 0b000000010111),
            (12, 0b000000011100),
            (12, 0b000000011101),
            (12, 0b000000011110),
            (12, 0b000000011111),
        }
    );

    private static HorizontalRunEncoder blackEncoder = new(new (byte, uint)[]
        {
            (10, 0b0000110111),
            (3, 0b010),
            (2, 0b11),
            (2, 0b10),
            (3, 0b011),
            (4, 0b0011),
            (4, 0b0010),
            (5, 0b00011),
            (6, 0b000101),
            (6, 0b000100),
            (7, 0b0000100),
            (7, 0b0000101),
            (7, 0b0000111),
            (8, 0b00000100),
            (8, 0b00000111),
            (9, 0b000011000),
            (10, 0b0000010111),
            (10, 0b0000011000),
            (10, 0b0000001000),
            (11, 0b00001100111),
            (11, 0b00001101000),
            (11, 0b00001101100),
            (11, 0b00000110111),
            (11, 0b00000101000),
            (11, 0b00000010111),
            (11, 0b00000011000),
            (12, 0b000011001010),
            (12, 0b000011001011),
            (12, 0b000011001100),
            (12, 0b000011001101),
            (12, 0b000001101000),
            (12, 0b000001101001),
            (12, 0b000001101010),
            (12, 0b000001101011),
            (12, 0b000011010010),
            (12, 0b000011010011),
            (12, 0b000011010100),
            (12, 0b000011010101),
            (12, 0b000011010110),
            (12, 0b000011010111),
            (12, 0b000001101100),
            (12, 0b000001101101),
            (12, 0b000011011010),
            (12, 0b000011011011),
            (12, 0b000001010100),
            (12, 0b000001010101),
            (12, 0b000001010110),
            (12, 0b000001010111),
            (12, 0b000001100100),
            (12, 0b000001100101),
            (12, 0b000001010010),
            (12, 0b000001010011),
            (12, 0b000000100100),
            (12, 0b000000110111),
            (12, 0b000000111000),
            (12, 0b000000100111),
            (12, 0b000000101000),
            (12, 0b000001011000),
            (12, 0b000001011001),
            (12, 0b000000101011),
            (12, 0b000000101100),
            (12, 0b000001011010),
            (12, 0b000001100110),
            (12, 0b000001100111)
        },
        new (byte, uint)[]
        {
            (10, 0b0000001111),
            (12, 0b000011001000),
            (12, 0b000011001001),
            (12, 0b000001011011),
            (12, 0b000000110011),
            (12, 0b000000110100),
            (12, 0b000000110101),
            (13, 0b0000001101100),
            (13, 0b0000001101101),
            (13, 0b0000001001010),
            (13, 0b0000001001011),
            (13, 0b0000001001100),
            (13, 0b0000001001101),
            (13, 0b0000001110010),
            (13, 0b0000001110011),
            (13, 0b0000001110100),
            (13, 0b0000001110101),
            (13, 0b0000001110110),
            (13, 0b0000001110111),
            (13, 0b0000001010010),
            (13, 0b0000001010011),
            (13, 0b0000001010100),
            (13, 0b0000001010101),
            (13, 0b0000001011010),
            (13, 0b0000001011011),
            (13, 0b0000001100100),
            (13, 0b0000001100101),
            (11, 0b00000001000),
            (11, 0b00000001100),
            (11, 0b00000001101),
            (12, 0b000000010010),
            (12, 0b000000010011),
            (12, 0b000000010100),
            (12, 0b000000010101),
            (12, 0b000000010110),
            (12, 0b000000010111),
            (12, 0b000000011100),
            (12, 0b000000011101),
            (12, 0b000000011110),
            (12, 0b000000011111),
        }
    );
}