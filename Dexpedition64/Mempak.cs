using System;
using System.Collections.Generic;

namespace Dexpedition64
{
    class Mempak
    {
        public Dictionary<byte, string> N64Symbols = new Dictionary<byte, string>
        {
            { 0,  ""}, { 15, " "}, { 16, "0"},
            { 17, "1"}, { 18, "2"}, { 19, "3"}, { 20, "4"},
            { 21, "5"}, { 22, "6"}, { 23, "7"}, { 24, "8"},
            { 25, "9"}, { 26, "A"}, { 27, "B"}, { 28, "C"},
            { 29, "D"}, { 30, "E"}, { 31, "F"}, { 32, "G"},
            { 33, "H"}, { 34, "I"}, { 35, "J"}, { 36, "K"},
            { 37, "L"}, { 38, "M"}, { 39, "N"}, { 40, "O"},
            { 41, "P"}, { 42, "Q"}, { 43, "R"}, { 44, "S"},
            { 45, "T"}, { 46, "U"}, { 47, "V"}, { 48, "W"},
            { 49, "X"}, { 50, "Y"}, { 51, "Z"}, { 52, "!"},
            { 53, "\""}, { 54, "#"}, { 55, "'"}, { 56, "*"},
            { 57, "+"}, { 58, ","}, { 59, "-"}, { 60, "."},
            { 61, "/"}, { 62, ","}, { 63, "="}, { 64, "?"},
            { 65, "@"}, { 66, "。"}, { 67, "゛"}, { 68, "゜"},
            { 69, "ァ"}, { 70, "ィ"}, { 71, "ゥ"}, { 72, "ェ"},
            { 73, "ォ"}, { 74, "ッ"}, { 75, "ャ"}, { 76, "ュ"},
            { 77, "ョ"}, { 78, "ヲ"}, { 79, "ン"}, { 80, "ア"},
            { 81, "イ"}, { 82, "ウ"}, { 83, "エ"}, { 84, "オ"},
            { 85, "カ"}, { 86, "キ"}, { 87, "ク"}, { 88, "ケ"},
            { 89, "コ"}, { 90, "サ"}, { 91, "シ"}, { 92, "ス"},
            { 93, "セ"}, { 94, "ソ"}, { 95, "タ"}, { 96, "チ"},
            { 97, "ツ"}, { 98, "テ"}, { 99, "ト" }, {100, "ナ"},
            { 101, "ニ"}, { 102, "ヌ"}, { 103, "ネ"}, { 104, "ノ"},
            { 105, "ハ"}, { 106, "ヒ"}, { 107, "フ"}, { 108, "ヘ"},
            { 109, "ホ"}, { 110, "マ"}, { 111, "ミ"}, { 112, "ム"},
            { 113, "メ"}, { 114, "モ"}, { 115, "ヤ"}, { 116, "ユ"},
            { 117, "ヨ"}, { 118, "ラ"}, { 119, "リ"}, { 120, "ル"},
            { 121, "レ"}, { 122, "ロ"}, { 123, "ワ"}, { 124, "ガ"},
            { 125, "ギ"}, { 126, "グ"}, { 127, "ゲ"}, { 128, "ゴ"},
            { 129, "ザ"}, { 130, "ジ"}, { 131, "ズ"}, { 132, "ゼ"},
            { 133, "ゾ"}, { 134, "ダ"}, { 135, "ヂ"}, { 136, "ヅ"},
            { 137, "デ"}, { 138, "ド"}, { 139, "バ"}, { 140, "ビ"},
            { 141, "ブ"}, { 142, "ベ"}, { 143, "ボ"}, { 144, "パ"},
            { 145, "ピ"}, { 146, "プ"}, { 147, "ペ"}, { 148, "ポ"}
        };

        public static byte[] FormatPage()
        {
            byte[] openBlock = new byte[16]
            {
                0x00, 0x71, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x03, 0x00, 0x03
            };

            byte[] page = new byte[256];
            int i;

            for(i = 0; i < 16; i++)
            {
                page[i] = openBlock[i];
            }

            for(i = 16; i < 256; i+=2)
            {
                page[i] = 0x00;
                page[i+1] = 0x03;
            }

            return page;
        }

        public static byte[] GenerateID()
        {
            byte[] IDBlock = new byte[32];
            byte[] serial = new byte[24];
            Random rnd = new Random();
            int i;

            rnd.NextBytes(serial);

            for (i = 0; i < 24; i++)
            {
                IDBlock[i] = serial[i];
            }

            IDBlock[24] = 0;
            IDBlock[25] = 1;
            IDBlock[26] = 1;
            IDBlock[27] = 0;

            ushort ckSum1 = 0;
            for (i = 0; i < 28; i += 2)
            {
                ushort ckWord = (ushort)((IDBlock[i] << 8) + (IDBlock[i + 1]));
                ckSum1 += ckWord;
            }

            ushort ckSum2 = (ushort)(0xFFF2 - ckSum1);

            IDBlock[28] = (byte)(ckSum1 >> 8 & 0xFF);
            IDBlock[29] = (byte)(ckSum1 & 0xFF);
            IDBlock[30] = (byte)(ckSum2 >> 8 & 0xFF);
            IDBlock[31] = (byte)(ckSum2 & 0xFF);

            return IDBlock;
        }

        public static byte[] BuildHeader()
        {
            // A useless vanity label
            byte[] IDLabel = new byte[32]
            {
                0X20, 0X44, 0X45, 0X58, 0X50, 0X45, 0X44, 0X49, 0X54, 0X49, 0X4F, 0X4E, 0X20, 0X36, 0X34, 0X20,
                0X2D, 0X20, 0X42, 0x59, 0x20, 0x48, 0x4F, 0x4E, 0x4B, 0x45, 0x59, 0x20, 0x4B, 0x4F, 0x4E, 0x47
            };

            byte[] header = new byte[256];
            byte[] idBlock = GenerateID();
            int i;

            for (i = 0; i < 32; i++)
            {
                header[i] = IDLabel[i];
                header[i + 0x20] = idBlock[i];
                header[i + 0x40] = 0x00;
                header[i + 0x60] = idBlock[i];
                header[i + 0x80] = idBlock[i];
                header[i + 0xA0] = 0x00;
                header[i + 0xC0] = idBlock[i];
                header[i + 0xE0] = 0x00;
            }

            return header;
        }
    }
}
