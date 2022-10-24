using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;

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

        public Int16 ByteSwap(Int16 i)
        {
            return (Int16)(((i << 8) & 0xFF00) + ((i >> 8) & 0xFF));
        }

        public List<string> ReadNoteTable(byte[] table)
        {
            List<string> notes = new List<string>();
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

        // A useless vanity label
        public static readonly byte[] IDLabel = new byte[32]
        {
                0x44, 0x45, 0x58, 0x50, 0x45, 0x44, 0x49, 0x54, 0x49, 0x4F, 0x4E, 0x36, 0x34, 0x20, 0x56, 0x30,
                0x31, 0x20, 0x42, 0x59, 0x20, 0x48, 0x4F, 0x4E, 0x4B, 0x45, 0x59, 0x4B, 0x4F, 0x4E, 0x47, 0x00
        };

        public static byte[] BuildHeader()
        {
            byte[] header = new byte[256];
            byte[] idBlock = GenerateID();

            // Loop through and write each byte
            for (int i = 0; i < 32; i++)
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

        public static byte[] BuildHeader(Mempak mpk)
        {
            // Insert useless vanity label
            byte[] header = BuildHeader(); 
            
            for(int i = 0; i < 32; i++)
            {
                header[i] = IDLabel[i];
            }
            for(int i = 32; i < 256; i++)
            {
                header[i] = mpk.Header[i];
            }

            // Return modified header.
            return header;
        }

        public string Label;
        public string CheckSum1;
        public string CheckSum2;
        public string RealCheckSum;
        public string SerialNumber;
        public int ErrorCode = 0;
        public string ErrorStr = "";
        public List<short> IndexTable = new List<short>();
        public byte[] Header = new byte[256];
        public byte[] Data = new byte[32768];  

        public enum CardType { CARD_NONE, CARD_VIRTUAL, CARD_PHYSICAL };
        public CardType Type = CardType.CARD_NONE;

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
                Data = br.ReadBytes((int)fs.Length);

                // Copy the header.
                for(int i = 0; i < 256; i++)
                {
                    Header[i] = Data[i];
                }

                fs.Seek(0, SeekOrigin.Begin);

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
                this.RealCheckSum = ByteSwap((short)realCkSum).ToString("X2");

                /* 32 bytes of null, then two more ID blocks.
                 * 32 more nulls, one more ID block, 32 null,
                 * for a grand total of 192 redundant bytes. 
                 * If we wanted to, we could parse the other
                 * ID blocks and sanity check, but fuck that. */
                br.ReadBytes(192); // Just chuck 'em

                // Convert our serial number to a hex string.
                this.SerialNumber = string.Join(":", serial.Select(hex => string.Format("{0:X2}", hex)));

                // Read the Index Table
                MemoryStream indexTable = new MemoryStream(br.ReadBytes(512));
                BinaryReader br2 = new BinaryReader(indexTable);
                for (int i = 0; i < 512; i += 2)
                {
                    IndexTable.Add(ByteSwap(br2.ReadInt16()));
                }
                
                byte ckByte = 0;
                
                for (int i = 0x0A; i < 256; i++)
                {
                    ckByte += (byte)indexTable.ReadByte();
                }

                // Read the Note Table
                for (int i = 0; i < 16; i++)
                {
                    MemoryStream header = new MemoryStream(br.ReadBytes(32));
                    notes.Add(new MPKNote(header, indexTable));
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
                                fs.Seek((page * 0x100), SeekOrigin.Begin);
                                note.Data.Write(br.ReadBytes(256), 0, 256);
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
                MemoryStream page = new MemoryStream(drive.ReadMemoryCardFrame(0));
                byte[] cardLabel = new byte[32];

                for (int i = 0; i < 32; i++)
                {
                    cardLabel[i] = (byte)page.ReadByte();
                    // Small fix for old crappy labels
                    if (cardLabel[i] == 0x1F) cardLabel[i] = 0x20;
                }

                this.Label = Encoding.Default.GetString(cardLabel);

                MemoryStream indexTable = new MemoryStream(
                    drive.ReadMemoryCardFrame(1).ToArray().Concat(
                        drive.ReadMemoryCardFrame(2)).ToArray()
                        );

                MemoryStream noteTable = new MemoryStream(
                    drive.ReadMemoryCardFrame(3).ToArray().Concat(
                        drive.ReadMemoryCardFrame(4)).ToArray()
                        );
                BinaryReader br = new BinaryReader(indexTable);
                
                for(int i = 0; i < 512; i += 2)
                {
                    IndexTable.Add(ByteSwap(br.ReadInt16()));
                }

                for (int i = 1; i < 16; i++)
                {
                    byte[] header = new byte[32];
                    for(int j = 0; j < 32; j++)
                    {
                        header[j] = (byte)noteTable.ReadByte();
                    }
                    notes.Add(new MPKNote(new MemoryStream(header), indexTable));
                }
            }
            catch(Exception ex)
            {
                ErrorCode = 2;
                ErrorStr = "Mempak(int, List<MPKNote>) Error: " + ex.Message + "\nSource: " + ex.Source ;
                return;
            }
            drive.StopDexDrive();
        }

        public MemoryStream ReadNoteFromCard(MPKNote note, int comPort)
        {
            DexDrive drive = new DexDrive();
            MemoryStream data = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(data);
            if (drive.StartDexDrive("COM" + comPort))
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
                fs.Write(new byte[] { note.Status }, 0, 1);
                // File Extension
                fs.Write(note.noteExtRaw, 0, 4);
                // File Name
                fs.Write(note.noteTitleRaw, 0, note.noteTitleRaw.Length);
                // Pad with zeroes, I guess?
                fs.Write(new byte[] { 0x00, 0x00, 0x00 }, 0, 3);

                // Write save data
                fs.Write(note.Data.ToArray(), 0, note.Data.ToArray().Length);

                // Close the file
                fs.Close();
            }
        }

        public void DeleteNote(int index, List<MPKNote> notes, List<short> indexTable)
        {
            List<short> notePages = new List<short>();
            short pageIndex = notes[index].StartPage;
            while (pageIndex != 1)
            {
                if(pageIndex != 0) notePages.Add(pageIndex);
                pageIndex = indexTable[pageIndex];
            }

            foreach(short note in notePages)
            {
                indexTable[note] = 0x03;
            }

            //notes.Remove(notes[index]);
            notes.RemoveAt(index);

            // Calculate new checksum for index table
            byte ckByte = 0;
            
            for(int i = 10; i < 256; i++)
            {
                ckByte += (byte)indexTable[i];
            }

            // Set the new checksum.
            indexTable[1] = ckByte;

        }

        public void SaveMPK(string fileName, Mempak mpk, List<MPKNote> notes)
        {
            using(FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                // Write the header
                byte[] header = BuildHeader(mpk);
                fs.Write(header, 0, header.Length);

                // Write the Index Table
                fs.Seek(0x100, SeekOrigin.Begin);
                foreach(short node in mpk.IndexTable)
                {
                    fs.Write(new byte[] { (byte)((node >> 8) & 0xFF) }, 0, 1);
                    fs.Write(new byte[] { (byte)(node & 0xFF) }, 0, 1);
                }

                // Write note table
                foreach (MPKNote note in notes)
                {
                    // Game Code
                    fs.Write(note.gameCodeRaw, 0, note.gameCodeRaw.Length);
                    // Publisher Code
                    fs.Write(note.pubCodeRaw, 0, note.pubCodeRaw.Length);
                    // Start Page
                    fs.Write(new byte[] { (byte)((note.StartPage >> 8) & 0xFF), (byte)(note.StartPage & 0xFF) }, 0, 2);
                    // Status
                    fs.Write(new byte[] { note.Status }, 0, 1);
                    // Reserved/Data Sum (Three zeroes)
                    fs.Write(new byte[] { 0x00, 0x00, 0x00 }, 0, 3);
                    // File Extension
                    fs.Write(note.noteExtRaw, 0, note.noteExtRaw.Length);
                    // File Name
                    fs.Write(note.noteTitleRaw, 0, note.noteTitleRaw.Length);
                }

                // Compensate for < 16 notes by zero filling.
                if (notes.Count < 16)
                {
                    for (int i = 0; i < 16 - notes.Count; i++)
                    {
                        for (int j = 0; j < 32; j++)
                        {
                            fs.Write(new byte[] { 0x00 }, 0, 1);
                        }
                    }
                }

                // Write card data.
                for(int i = 1280; i < 32768; i++)
                {
                    fs.Write(new byte[] { mpk.Data[i] }, 0, 1);
                }
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

        public Int16 ByteSwap(Int16 i)
        {
            return (Int16)(((i << 8) & 0xFF00) + ((i >> 8) & 0xFF));
        }


        //public MPKNote(byte[] header, byte[] index)
        public MPKNote(MemoryStream header, MemoryStream index)
        {
            // Set up readers for both streams
            BinaryReader hr = new BinaryReader(header);
            BinaryReader ir = new BinaryReader(index);

            // Seek past the first 5 pages.
            ir.BaseStream.Position = 10;

            // Calculate the checksum
            for(int i = 10; i < 256; i++)
            {
                // Add the current byte to the Checksum
                CheckByte += ir.ReadByte();
            }

            // Read the note table.
            for (int n = 0; n < 128; n++)
            {
                indexTable.Add(ByteSwap(ir.ReadInt16()));
                //indexTable.Add(ir.ReadInt16());
            }
            
            // Read the Game Code
            for (int i = 0; i < 4; i++)
            {
                gameCodeRaw[i] = hr.ReadByte();
            }

            GameCode = Encoding.Default.GetString(gameCodeRaw);

            // Read the Publisher Code
            for (int i = 0; i < 2; i++)
            {
                pubCodeRaw[i] = hr.ReadByte();
            }

            PubCode += Encoding.Default.GetString(pubCodeRaw);

            // Read the Start Page
            StartPage = ByteSwap(hr.ReadInt16());

            // Read the Status byte.
            Status = hr.ReadByte();

            // Read the note extension
            for(int i = 0; i < 4; i++)
            {
                noteExtRaw[i] = hr.ReadByte();
            }
            
            // Read the note title.
            for(int i = 0; i < 16; i++)
            {
                noteTitleRaw[i] = hr.ReadByte();
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

            while ((indexTable[currentPage] != 0x0001) && (indexTable[currentPage] != 0x0003) && (indexTable[currentPage] > 0) && (indexTable[currentPage] < 129))
            {
                Pages.Add(currentPage);
                currentPage = indexTable[currentPage];
                PageSize++;
            }
        }
    }
}
