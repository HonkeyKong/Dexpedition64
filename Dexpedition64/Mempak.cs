using System;
using System.IO;
using System.Text;
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

        public List<string> ReadNoteTable(byte[] table)
        {
            List<string> notes = new List<string>();
            //string noteCode;
            string noteTitle = "";
            byte[] GameCode = new byte[4];
            byte[] GamePub = new byte[2];
            byte[] GameTitle = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    GameCode[j] = table[j + (i * 32)];
                }

                noteTitle += Encoding.Default.GetString(GameCode) + " - ";

                for (int foo = 4; foo < 6; foo++)
                {
                    GamePub[foo-4] = table[foo + (i * 32)];
                }

                noteTitle += Encoding.Default.GetString(GamePub) + " - ";

                for (int k = 16; k < 32; k++)
                {
                    GameTitle[k-16] = table[k + (i * 32)];
                }
                
                for(int l = 0; l < 16; l++)
                {
                    try
                    {
                        noteTitle += N64Symbols[GameTitle[l]];
                    } catch(System.Collections.Generic.KeyNotFoundException ex)
                    {
                        noteTitle += "";
                    }
                }
                
                notes.Add(noteTitle.ToString());
                noteTitle = "";
            }
            return notes;
        }
        
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

            // Generate a random serial number
            rnd.NextBytes(serial);

            for (i = 0; i < 24; i++)
            {
                IDBlock[i] = serial[i];
            }

            IDBlock[24] = 0;
            IDBlock[25] = 1;
            IDBlock[26] = 1;
            IDBlock[27] = 0;

            // Calculate the first checksum
            ushort ckSum1 = 0;
            for (i = 0; i < 28; i += 2)
            {
                ushort ckWord = (ushort)((IDBlock[i] << 8) + (IDBlock[i + 1]));
                ckSum1 += ckWord;
            }

            // Second checksum is easy
            ushort ckSum2 = (ushort)(0xFFF2 - ckSum1);

            // Write the checksums to the header
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
                0x44, 0x45, 0x58, 0x50, 0x45, 0x44, 0x49, 0x54, 0x49, 0x4F, 0x4E, 0x36, 0x34, 0x20, 0x56, 0x30, 
                0x2E, 0x31, 0x20, 0x42, 0x59, 0x20, 0x48, 0x4F, 0x4E, 0x4B, 0x45, 0x59, 0x4B, 0x4F, 0x4E, 0x47
            };

            byte[] header = new byte[256];
            byte[] idBlock = GenerateID();
            int i;

            // Loop through and write each byte
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

    class MPKNote
    {
        public string GameCode;
        public string PubCode;
        public string NoteTitle;
        public string NoteExtension;
        public short StartPage;
        public byte Status;
        public byte[] Data;
        public int PageSize = 1;
        
        public MPKNote(byte[] header, byte[] index)
        {
            byte[] gameCodeRaw = new byte[4];
            byte[] pubCodeRaw = new byte[2];
            byte[] noteTitleRaw = new byte[16];
            byte[] noteExtRaw = new byte[4];
            byte[] startPageRaw = new byte[2];
            List<short> indexTable = new List<short>();
            
            Mempak mpk = new Mempak();

            for(int n = 0; n < index.Length; n += 2)
            {
                indexTable.Add((short)((short)(index[n] << 8 & 0xFF00) + (index[n+1] & 0xFF)));
            }
            
            for (int i = 0; i < 4; i++)
            {
                gameCodeRaw[i] = header[i];
            }

            GameCode = Encoding.Default.GetString(gameCodeRaw);

            for (int i = 4; i < 6; i++)
            {
                pubCodeRaw[i - 4] = header[i];
            }

            PubCode += Encoding.Default.GetString(pubCodeRaw);

            for (int i = 6; i < 8; i++)
            {
                startPageRaw[i - 6] = header[i];
            }

            StartPage = (short)((short)(startPageRaw[0] << 8 & 0xFF00) + (startPageRaw[1] & 0xFF));

            Status = header[0x08];

            for(int i = 0x0C; i < 0x10; i++)
            {
                noteExtRaw[i - 0x0C] = header[i];
            }
            
            for (int i = 16; i < 32; i++)
            {
                noteTitleRaw[i - 16] = header[i];
            }

            for (int i = 0; i < 4; i++)
            {
                try
                {
                    NoteExtension += mpk.N64Symbols[noteExtRaw[i]];
                }
                catch (System.Collections.Generic.KeyNotFoundException)
                {
                    NoteExtension += "";
                }
            }

            for (int i = 0; i < 16; i++)
            {
                try
                {
                    NoteTitle += mpk.N64Symbols[noteTitleRaw[i]];
                }
                catch (System.Collections.Generic.KeyNotFoundException)
                {
                    NoteTitle += "";
                }
            }

            short startPage;
            startPage = StartPage;
            while (indexTable[startPage] != 0x0001)
            {
                startPage = indexTable[startPage];
                PageSize++;
            }
        }
    }
}
