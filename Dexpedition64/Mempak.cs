﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Dexpedition64
{
    class Mempak
    {
        public static readonly Dictionary<byte, string> N64Symbols = new Dictionary<byte, string>
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
                0x31, 0x20, 0x42, 0x59, 0x20, 0x48, 0x4F, 0x4E, 0x4B, 0x45, 0x59, 0x4B, 0x4F, 0x4E, 0x47, 0x00
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

        public string Label;
        public string CheckSum1;
        public string CheckSum2;
        public string RealCheckSum;
        public string SerialNumber;
        public int ErrorCode = 0;
        public string ErrorStr = "";

        public enum CardType { CARD_NONE, CARD_VIRTUAL, CARD_PHYSICAL };
        public CardType Type = CardType.CARD_NONE;

        public string ShowError()
        {
            return this.ErrorStr;
        }

        private ushort CalculateChecksum(byte[] serial)
        {
            ushort checksum = 0;
            for (int i = 0; i < serial.Length; i += 2)
            {
                ushort ckWord = (ushort)((serial[i] << 8) + (serial[i + 1]));
                checksum += ckWord;
            }

            return checksum;
        }

        public Mempak(string fileName, List<MPKNote> notes)
        {

            using (FileStream fs = File.OpenRead(fileName))
            {
                BinaryReader br = new BinaryReader(fs);
                byte[] data = br.ReadBytes((int)fs.Length);

                fs.Seek(0, SeekOrigin.Begin);
                //BinaryReader br = new BinaryReader(fs);

                // Read the label
                byte[] cardLabel = br.ReadBytes(32);

                // Print the label
                this.Label = "Label: " + Encoding.Default.GetString(cardLabel);

                // Read the serial number
                byte[] serial = br.ReadBytes(28);

                // Read the Checksums
                short checkSum1 = br.ReadInt16();
                short checkSum2 = br.ReadInt16();

                // Print our Checksums
                this.CheckSum1 = checkSum1.ToString("X2");
                this.CheckSum2 = checkSum2.ToString("X2");

                // Calculate the actual checksum
                ushort realCkSum = CalculateChecksum(serial);
              
                // The result should be big-endian. Swap it.
                byte[] swapSum = new byte[2];
                swapSum[1] = (byte)(realCkSum >> 8 & 0xFF);
                swapSum[0] = (byte)(realCkSum & 0xFF);
                this.RealCheckSum = String.Join("", swapSum.Select(hex => String.Format("{0:X2}", hex)));

                /* 32 bytes of null, then two more ID blocks.
                 * 32 more nulls, one more ID block, 32 null,
                 * for a grand total of 192 redundant bytes. 
                 * If we wanted to, we could parse the other
                 * ID blocks and sanity check, but fuck that. */
                br.ReadBytes(192); // Just chuck 'em

                // Convert our serial number to a hex string.
                this.SerialNumber = string.Join(":", serial.Select(hex => string.Format("{0:X2}", hex)));

                // Read the Index Table
                byte[] indexTable = br.ReadBytes(512);
                byte ckByte = 0;
                for (int i = 0x0A; i < 256; i++)
                {
                    ckByte += indexTable[i];
                }

                // Read the Note Table
                for (int i = 0; i < 16; i++)
                {
                    notes.Add(new MPKNote(br.ReadBytes(32), indexTable));
                }


                // Copy the note.
                foreach (MPKNote note in notes)
                {
                    note.Data = new MemoryStream();

                    // Copy the note.
                    int pageCount = 0;
                    foreach (short page in note.Pages)
                    {
                        if ((page != 0) && (page < 128))
                        {
                            try
                            {
                                fs.Seek(0, SeekOrigin.Begin);
                                fs.Seek((page * 0x100), SeekOrigin.Current);
                            for (int i = 0; i < 0xFF; i++)
                            {
                                    note.Data.Write(new byte[] { br.ReadByte() }, 0, 1);
                                }
                            }
                            catch (System.IO.EndOfStreamException) {
                                ErrorCode = 3;
                                ErrorStr += "End of Stream reached at " + note.NoteTitle.ToString() + ", page " + page + "\n";
                            }
                            pageCount++;
                        }
                    }
                }
            }
        }

        public Mempak(int comPort, List<MPKNote> notes)
        {
            DexDrive drive = new DexDrive();
            if (!drive.StartDexDrive("COM" + comPort))
            {
                ErrorCode = 1;
                ErrorStr = "Failed to initialize DexDrive on COM" + comPort + ".";
                return;
            }

            try
            {
                byte[] page = drive.ReadMemoryCardFrame(0);
                byte[] cardLabel = new byte[32];
                
                for(int i = 0; i < 32; i++)
                {
                    cardLabel[i] = (byte)page[i];
                }

                this.Label = "Label: " + Encoding.Default.GetString(cardLabel);

                MemoryStream indexTable = new MemoryStream(
                    drive.ReadMemoryCardFrame(1).Concat(
                        drive.ReadMemoryCardFrame(2)).ToArray()
                        );

                MemoryStream note = new MemoryStream(
                    drive.ReadMemoryCardFrame(3).Concat(
                        drive.ReadMemoryCardFrame(4)).ToArray()
                        );

                for (int i = 1; i < 16; i++)
                {
                    byte[] header = new byte[32];
                    for(int j = 0; j < 32; j++)
                    {
                        header[j] = (byte)note.ReadByte();
                    }
                    notes.Add(new MPKNote(header, indexTable.ToArray()));
                }
            }
            catch(Exception ex)
            {
                ErrorCode = 2;
                ErrorStr = "Error: " + ex.Message + "\nSource: " + ex.Source ;
                return;
            }
            drive.StopDexDrive();
        }

        public MemoryStream ReadNoteFromCard(MPKNote note)
        {
            DexDrive drive = new DexDrive();
            MemoryStream data = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(data);
            if (drive.StartDexDrive("COM1"))
            {
                foreach (short page in note.Pages)
                {
                    bw.Write(drive.ReadMemoryCardFrame((ushort)page));
                }
                drive.StopDexDrive();
            }
            return data;
        }

        public void SaveNote(MPKNote note, string fileName)
        {
            using (FileStream fs = File.OpenWrite(fileName))
            {
                // Write Note Version (We use version 1)
                fs.Write(new byte[] { 0x01 }, 0, 1);

                // Write signature
                fs.Write(new byte[] { 0x4D, 0x50, 0x4B, 0x4E, 0x6F, 0x74, 0x65 }, 0, 7);

                // Write reserved bytes
                fs.Write(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, 7);

                // Write number of comment blocks (2 16-byte blocks)
                fs.Write(new byte[] { 0x02 }, 0, 1);

                // Write comment data
                fs.Write(new byte[] {
                        0x44, 0x45, 0x58, 0x50, 0x45, 0x44, 0x49, 0x54, 0x49, 0x4F, 0x4E, 0x36, 0x34, 0x20, 0x56, 0x30,
                        0x31, 0x20, 0x42, 0x59, 0x20, 0x48, 0x4F, 0x4E, 0x4B, 0x45, 0x59, 0x4B, 0x4F, 0x4E, 0x47, 0x00
                    }, 0, 32);

                // Write note entry
                // Game Code
                fs.Write(note.gameCodeRaw, 0, note.gameCodeRaw.Length);
                // Publisher Code
                fs.Write(note.pubCodeRaw, 0, note.pubCodeRaw.Length);
                // Start page (0xCAFE since this is an extracted save)
                fs.Write(new byte[] { 0xCA, 0xFE }, 0, 2);
                // Status/Reserved/Data Sum (None of this crap matters)
                fs.Write(new byte[] { 0x02, 0x00, 0x00, 0x00 }, 0, 4);
                // File Extension
                fs.Write(note.noteExtRaw, 0, note.noteExtRaw.Length);
                // File Name
                fs.Write(note.noteTitleRaw, 0, note.noteTitleRaw.Length);

                // Write save data
                fs.Write(note.Data.ToArray(), 0, (int)note.Data.Length);

                // Close the file
                fs.Close();
            }
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
        public MemoryStream Data;
        public int PageSize = 1;
        public byte CheckByte = 0;
        public List<short> Pages = new List<short>();

        public byte[] gameCodeRaw = new byte[4];
        public byte[] pubCodeRaw = new byte[2];
        public byte[] noteTitleRaw = new byte[16];
        public byte[] noteExtRaw = new byte[4];
        public byte[] startPageRaw = new byte[2];
        List<short> indexTable = new List<short>();

        /*private readonly Dictionary<byte, string> N64Symbols = new Dictionary<byte, string>
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
        };*/

        public MPKNote(byte[] header, byte[] index)
        {
            // Calculate the checksum
            for(int i = 10; i < 256; i++)
            {
                CheckByte += index[i];
            }

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
                    NoteExtension += Mempak.N64Symbols[noteExtRaw[i]];
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
                    NoteTitle += Mempak.N64Symbols[noteTitleRaw[i]];
                }
                catch (System.Collections.Generic.KeyNotFoundException)
                {
                    NoteTitle += "";
                }
            }

            short currentPage;
            currentPage = StartPage;
            Pages.Add(currentPage);

            while ((indexTable[currentPage] != 0x0001) && (indexTable[currentPage] != 0x0003) && (indexTable[currentPage] != 0x0000))
            {
                currentPage = indexTable[currentPage];
                Pages.Add(currentPage);
                PageSize++;
            }
        }
    }
}
