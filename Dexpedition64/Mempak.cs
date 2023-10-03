﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Collections;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Management;
using System.Diagnostics.Eventing.Reader;

namespace Dexpedition64
{
    public class ProgressEventArgs : EventArgs
    {
        public int ProgressValue { get; }

        public ProgressEventArgs(int progressValue)
        {
            ProgressValue = progressValue;
        }
    }

    class Mempak
    {
        public const int PageSize = 256;
        public const int CardSize = 32768;
        public const int TotalPages = 128;
        public const int NoteEntrySize = 32;
        private const int OpenPage = 0x03;
        private const int IndexTablePage = 1;
        private const int IndexTableBackupPage = 2;
        private const int NoteTablePage = 3;
        private const int noteStartPage = 0x05;

        public string Label;
        public string CheckSum1;
        public string CheckSum2;
        public string RealCheckSum;
        public string SerialNumber;
        public int firstFreePage = 5;
        public int ErrorCode = 0;
        public string ErrorStr = "";
        public List<short> IndexTable = new List<short>();
        public byte[] Header = new byte[256];
        public byte[] Data = new byte[CardSize];
        public int WriteProgress = 0;
        public event EventHandler<ProgressEventArgs> ProgressChanged;

        public enum CardType { CARD_NONE, CARD_VIRTUAL, CARD_PHYSICAL };
        public CardType Type = CardType.CARD_NONE;

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

        public void FormatCard(int comPort)
        {
            System.Windows.Forms.DialogResult yn = MessageBox.Show("Format Card?\nWARNING: ALL DATA WILL BE LOST.", "Format Card?", MessageBoxButtons.YesNo);
            if (yn == System.Windows.Forms.DialogResult.No)
            {
                MessageBox.Show("Format cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (yn == System.Windows.Forms.DialogResult.Yes)
            {
                DexDrive drive = new DexDrive();
                WriteProgress = 0;
                //ProgressChanged?.Invoke(this, new ProgressEventArgs(WriteProgress));

                if (drive.StartDexDrive($"COM{comPort}"))
                {
                    try
                    {
                        string strFailed = "Writing frame failed.";
                        void showError(string msg)
                        {
                            MessageBox.Show(msg, "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        if (!drive.WriteMemoryCardFrame(0, Mempak.BuildHeader()))
                        {
                            showError(strFailed);
                            return;
                        }
                        WriteProgress += 16;

                        // Make a quick Index table and fill it with blanks.
                        List<short> table = new List<short>();
                        for (ushort i = 0; i < 128; i++)
                        {
                            table.Add(0x0003);
                        }

                        // Calculate the checksum.
                        byte ckByte = 0;
                        for (int i = 10; i < 128; i++)
                        {
                            ckByte += (byte)this.IndexTable[i];
                        }
                        // Write the checksum to the index table.
                        table[1] = ckByte;

                        // Write the index table to the card.
                        for (ushort i = 1; i < 3; i++) 
                        {
                            if (!drive.WriteMemoryCardFrame(i, table.ToArray().SelectMany(BitConverter.GetBytes).ToArray()))
                            {
                                showError(strFailed);
                                return;
                            }
                            WriteProgress += 16;
                        }

                        // Clear the note table
                        for (ushort i = 3; i < 5; i++)
                        {
                            if (!drive.WriteMemoryCardFrame(i, DexDrive.BlankPage()))
                            {
                                showError(strFailed);
                                return;
                            }
                            WriteProgress += 16;
                        }

                        // And we're done!
                        drive.StopDexDrive();
                        WriteProgress += 4;
                        MessageBox.Show("Card formatted.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message +
                            "\nAre you sure your DexDrive is plugged in?" +
                            "\nTry disconnecting and reconnecting the power.",
                            "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Format Failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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

                for (int offset = 4; offset < 6; offset++)
                {
                    GamePub[offset-4] = table[offset + (i * 32)];
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

            // Generate a random serial number
            rnd.NextBytes(serial);

            for (int i = 0; i < serial.Length; i++)
            {
                IDBlock[i] = serial[i];
            }

            IDBlock[24] = 0;
            IDBlock[25] = 1;
            IDBlock[26] = 1;
            IDBlock[27] = 0;

            // Calculate the first checksum
            ushort ckSum1 = 0;
            for (int i = 0; i < 28; i += 2)
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
                //header[i + 0x40] = 0x00;
                header[i + 0x40] = IDLabel[i];
                header[i + 0x60] = idBlock[i];
                header[i + 0x80] = idBlock[i];
                header[i + 0xA0] = 0x00;
                header[i + 0xC0] = idBlock[i];
                //header[i + 0xE0] = 0x00;
                header[i + 0xE0] = IDLabel[i];
            }

            return header;
        }

        public static byte[] BuildHeader(Mempak mpk)
        {
            bool backupFound = false;
            // Insert useless vanity label
            byte[] header = BuildHeader();

            short labelOffset = 0;

            if (mpk.Header[0] == 0x00)
            {
                // Check for backup of card label at 0x40
                if (mpk.Header[0x40] != 0x00) {
                    labelOffset = 0x40;
                    backupFound = true;
                }
                // Check for other backup at 0xE0
                if ((mpk.Header[0xE0] != 0x00) && (!backupFound)) {
                    labelOffset = 0xE0;
                    backupFound = true;
                }
            }

            for(int i = 0; i < 32; i++)
            {
                if ((labelOffset == 0) || ((labelOffset != 0) && (!backupFound))) header[i] = mpk.Header[i];
                else header[i] = header[labelOffset + i];
            }

            for(int i = 32; i < 256; i++)
            {
                header[i] = mpk.Header[i];
            }

            // Return modified header.
            return header;
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

        private void writeMPKData(byte[] src)
        {
            int offset = PageSize * firstFreePage;

            for (int i = 0; i < src.Length; i++)
            {
                this.Data[offset + i] = src[i];
            }

            int sizeInPages = src.Length / PageSize;
            updateIndexTable(firstFreePage, (short)sizeInPages);
            
            // Mark the first free page after the data.
            firstFreePage += (int)sizeInPages;
        }

        // This version simply updates the check byte.
        private void updateIndexTable()
        {
            // Calculate and set the new checksum.
            byte ckByte = 0;
            for (int i = 10; i < 128; i++)
            {
                ckByte += (byte)this.IndexTable[i];
            }
            this.IndexTable[1] = ckByte;
        }

        // This version updates the specific page with the data offset.f
        private void updateIndexTable(int pageNum, short sizeinPages) {

            if (sizeinPages > 1)
            {
                int pageCount = 1;
                for (int i = pageNum; i < (pageNum + sizeinPages); i++)
                {
                    if (pageCount < sizeinPages) this.IndexTable[i] = (short)(i + 1); else this.IndexTable[i] = 1;
                    pageCount++;
                    //if(pageCount == sizeinPages) break;
                }
            } else this.IndexTable[pageNum] = 1;

            // Calculate and set the new checksum.
            byte ckByte = 0;
            for (int i = 10; i < 128; i++)
            {
                ckByte += (byte)this.IndexTable[i];
            }
            this.IndexTable[1] = ckByte;            
        }

        private void clearIndexPage(ushort pageNum)
        {
            this.IndexTable[pageNum] = OpenPage;
        }

        public Mempak()
        {
            // Initialize the fields and properties for a blank Mempak object
            this.Type = CardType.CARD_VIRTUAL;
            this.Data = new byte[CardSize];
            this.Label = "DEXPEDITION64 V01 BY HONKEYKONG";
            this.CheckSum1 = string.Empty;
            this.CheckSum2 = string.Empty;
            this.RealCheckSum = string.Empty;
            this.SerialNumber = string.Empty;
            this.IndexTable = new List<short>();
            this.Header = BuildHeader();
            this.firstFreePage = 5;

            byte[] serial = new byte[28];
            for(int i = 0; i < 28; i++)
            {
                serial[i] = this.Header[i + 0x20];
            }

            this.SerialNumber = string.Join("", serial.Select(hex => string.Format("{0:X2}", hex)));

            // Write the Index table.
            for (int i = 0; i < 128; i++) {
                this.IndexTable.Add(OpenPage);
            }

            updateIndexTable();

            //MessageBox.Show($"New MPK created with serial number {this.SerialNumber}");
        }

        public Mempak(string fileName, List<MPKNote> notes)
        {
            bool dexDriveSave = false;
            // Read the file into an array
            byte[] fileData = File.ReadAllBytes(fileName);
            
            // Read the header into an array
            byte[] magicString = new byte[12];
            Array.Copy(fileData, magicString, 12);
            
            string headerString = System.Text.Encoding.UTF8.GetString(magicString);
            if(headerString == "123-456-STD\0")
            {
                MessageBox.Show("DexDrive image detected. Converting to MPK...");
                dexDriveSave = true;
                byte[] rawCard = new byte[CardSize];
                Array.Copy(fileData, 0x1040, rawCard, 0, CardSize);
                fileName = "tmpCard.tmp";
                File.WriteAllBytes(fileName, rawCard);
            }
            
            using (FileStream fs = File.OpenRead(fileName))
            {
                BinaryReader br = new BinaryReader(fs);
                Data = br.ReadBytes((int)fs.Length);

                // Copy the header.
                if (dexDriveSave)
                {
                    Array.Copy(BuildHeader(), Header, 256);
                }
                else
                {
                    Array.Copy(Data, Header, 256);
                }

                fs.Seek(0, SeekOrigin.Begin);

                byte[] cardLabel = new byte[32];
                Array.Copy(Header, cardLabel, 32);

                // For posterity's sake, zoom up 32 bytes
                br.ReadBytes(32);

                // Print the label
                this.Label = Encoding.ASCII.GetString(cardLabel);

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
                this.SerialNumber = string.Join("", serial.Select(hex => string.Format("{0:X2}", hex)));

                // Read the Index Table
                MemoryStream indexTable = new MemoryStream(br.ReadBytes(512));
                BinaryReader br2 = new BinaryReader(indexTable);
                for (int i = 0; i < 128; i ++)
                {
                    IndexTable.Add(ByteSwap(br2.ReadInt16()));
                }

                updateIndexTable();

                int totalPages = 0;
                // Read the Note Table
                for (int i = 0; i < 16; i++)
                {
                    MemoryStream header = new MemoryStream(br.ReadBytes(32));
                    MPKNote note = new MPKNote();
                    if (header.ToArray()[0] != 0x00)
                    {
                        note.ParseHeader(header.ToArray());
                        //MessageBox.Show($"Start Page: {note.StartPage.ToString()}");
                        MemoryStream noteStream = new MemoryStream();
                        BinaryWriter noteData = new BinaryWriter(noteStream);

                        if (IndexTable[note.StartPage] == 0x0001)
                        {
                            noteData.Write(Data, Mempak.PageSize * note.StartPage, Mempak.PageSize);
                            note.PageSize = 1;
                        }
                        else
                        {
                            short nextNote = note.StartPage;
                            while (IndexTable[nextNote] != 0x0001)
                            {
                                note.PageSize++;
                                noteData.Write(Data, Mempak.PageSize * nextNote, Mempak.PageSize);
                                note.Pages.Add(nextNote);
                                nextNote++;
                            }
                        }

                        note.Data = noteStream.ToArray();
                        totalPages += note.PageSize;
                        notes.Add(note);
                    }
                }

                this.firstFreePage = 5 + totalPages;
            }
            File.Delete("tmpCard.tmp");
        }

        public Mempak(int comPort, List<MPKNote> notes)
        {
            DexDrive drive = new DexDrive();
            if (!drive.StartDexDrive($"COM{comPort}"))
            {
                ErrorCode = 1;
                ErrorStr = $"Failed to initialize DexDrive on COM{comPort}.";
                return;
            }

            try
            {
                MemoryStream page = new MemoryStream(drive.ReadMemoryCardFrame(0));
                BinaryReader pr = new BinaryReader(page);

                byte[] cardLabel = pr.ReadBytes(32);

                // Read the serial number
                byte[] serial = pr.ReadBytes(28);

                // Read the Checksums
                short checkSum1 = pr.ReadInt16();
                short checkSum2 = pr.ReadInt16();

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

                for (int i = 0; i < 32; i++)
                {
                    cardLabel[i] = (byte)page.ReadByte();
                    // Small fix for old crappy labels
                    if (cardLabel[i] == 0x1F) cardLabel[i] = 0x20;
                }

                this.Label = Encoding.Default.GetString(cardLabel);
                this.SerialNumber = string.Join("", serial.Select(hex => string.Format("{0:X2}", hex)));

                MemoryStream indexTable = new MemoryStream(drive.ReadMemoryCardFrame(1).ToArray().Concat(drive.ReadMemoryCardFrame(2)).ToArray());

                MemoryStream noteTable = new MemoryStream(drive.ReadMemoryCardFrame(3).ToArray().Concat(drive.ReadMemoryCardFrame(4)).ToArray());

                BinaryReader br = new BinaryReader(indexTable);
                
                for(int i = 0; i < 128; i++)
                {
                    IndexTable.Add(ByteSwap(br.ReadInt16()));
                }

                // Read out the rest of the card into RAM.
                MemoryStream dataStream = new MemoryStream(Data);
                BinaryWriter cardWriter = new BinaryWriter(dataStream);
                for(ushort i = 5; i < 128; i++)
                {
                    cardWriter.Write(drive.ReadMemoryCardFrame(i));
                }
                Array.Copy(dataStream.ToArray(), Data, dataStream.Length);
                File.WriteAllBytes("test.data", Data);

                // Read the Note Table
                BinaryReader nr = new BinaryReader(noteTable);
                for (int i = 0; i < 16; i++)
                {
                    MemoryStream header = new MemoryStream(nr.ReadBytes(32));
                    MPKNote note = new MPKNote();
                    if (header.ToArray()[0] != 0x00)
                    {
                        note.ParseHeader(header.ToArray());
                        //MessageBox.Show($"Start Page: {note.StartPage.ToString()}");
                        MemoryStream noteStream = new MemoryStream();
                        BinaryWriter noteData = new BinaryWriter(noteStream);

                        if (IndexTable[note.StartPage] == 0x0001)
                        {
                            noteData.Write(Data, Mempak.PageSize * (note.StartPage - 5), Mempak.PageSize);
                            note.PageSize = 1;
                        }
                        else
                        {
                            short nextNote = note.StartPage;
                            while (IndexTable[nextNote] != 0x0001)
                            {
                                note.PageSize++;
                                noteData.Write(Data, Mempak.PageSize * (nextNote - 5), Mempak.PageSize);
                                note.Pages.Add(nextNote);
                                nextNote++;
                            }
                            if (IndexTable[nextNote] == 0x0001)
                            {
                                noteData.Write(Data, Mempak.PageSize * (nextNote - 4), Mempak.PageSize);
                            }
                        }
                        note.Data = noteStream.ToArray();
                        notes.Add(note);
                    }
                }
                // end read note table
            }
            catch (Exception ex)
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

            // This is just some debugging shit
            string Pages = "";
            if (note.Pages.Count == 1) Pages = note.StartPage.ToString();
            else foreach (short page in note.Pages) Pages += $"{page.ToString()} ";
            MessageBox.Show($"Pages being read:\n{Pages}");

            if (drive.StartDexDrive($"COM{comPort}"))
            {
                /*  Read two pages back. This should be OK since we're only
                    reading notes, so the furthest it should go back is 3.
                    It's silly that we even have to do that, but there's
                    a bug in the firmware that causes the first part of
                    the data read back to be null, so we do this as a way
                    to "warm up" the read function of the DexDrive.     */
                /*drive.ReadMemoryCardFrame((ushort)(note.StartPage - 2));
                drive.ReadMemoryCardFrame((ushort)(note.StartPage - 1));*/

                if (note.Pages.Count == 1) bw.Write(drive.ReadMemoryCardFrame((ushort)note.StartPage));
                else foreach (short page in note.Pages) bw.Write(drive.ReadMemoryCardFrame((ushort)page));

                drive.StopDexDrive();
            }
            return data;
        }

        public MemoryStream ReadNoteFromFile(Mempak mpk, MPKNote note)
        {
            MemoryStream data = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(data);

            // This is just some debugging shit
            string Pages = "";
            if (note.Pages.Count == 1) Pages = note.StartPage.ToString();
            else foreach (short page in note.Pages) Pages += $"{page.ToString()} ";
            MessageBox.Show($"Pages being read:\n{Pages}");

            //if (note.Pages.Count == 1) bw.Write(drive.ReadMemoryCardFrame((ushort)note.StartPage));
            //else foreach (short page in note.Pages) bw.Write(drive.ReadMemoryCardFrame((ushort)page));
            
            if (note.Pages.Count == 1) bw.Write(mpk.Data, (PageSize * (note.StartPage - 4)), PageSize);
            else foreach (short page in note.Pages) bw.Write(mpk.Data, (PageSize * (page - 4)), PageSize);
            note.Data = data.ToArray();

            MessageBox.Show($"Note {note.NoteTitle}{(note.NoteExtension != null ? ".{note.noteExtension}" : "")} read.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return data;
        }

        public void DeleteNote(MPKNote note, int startPage)
        {
            int currentIndex = startPage;
            while (currentIndex != -1)
            {
                // Find the next block in the sequence
                int nextIndex = this.IndexTable[currentIndex];

                // Mark the current block as free
                this.IndexTable[currentIndex] = OpenPage;

                // Move to the next block in the sequence
                currentIndex = nextIndex;
                if (nextIndex == 0x0001) break;
            }
        }

        public void InsertNote(short startPage, short sizeInPages)
        {
            int currentIndex = 0;
            while (currentIndex < sizeInPages)
            {
                if (IndexTable[currentIndex] == 0x0003)
                {
                    // Find the next free block if available
                    int nextFreeIndex = currentIndex + 1;
                    while (nextFreeIndex < IndexTable.Count && IndexTable[nextFreeIndex] != 0x0003)
                    {
                        nextFreeIndex++;
                    }

                    // Found a free block, insert the entry here
                    IndexTable[currentIndex] = (short)(currentIndex + 1);

                    // Update the sequence if there's a free block next
                    if (nextFreeIndex < IndexTable.Count)
                    {
                        IndexTable[currentIndex] = (short)nextFreeIndex;
                    }
                    else
                    {
                        // This is the last entry in the sequence
                        IndexTable[currentIndex] = 0x0001;
                    }

                    break;
                }

                currentIndex = IndexTable[currentIndex];
            }
        }


        public void ImportNote(MPKNote note, List<MPKNote> notes)//, Mempak mpk)
        {
            /*if(this.Type == CardType.CARD_PHYSICAL)
            {
                MessageBox.Show("Not supported on physical cards yet.", "Not implemented.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }*/

            if(this.Type == CardType.CARD_NONE)
            {
                MessageBox.Show("Please load or create an MPK file first.", "No File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            note.StartPage = (short)firstFreePage;
            note.PageSize = note.Data.Length / PageSize;
            updateIndexTable(firstFreePage, (short)note.PageSize);
            updateIndexTable();
            firstFreePage += note.PageSize;
            notes.Add(note);
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
                fs.Write(new byte[] { 0x00, 0x00 }, 0, 2);

                // Create a timestamp.
                long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                // Ensure the timestamp fits within a 5-byte array
                if (unixTimestamp >= 0 && unixTimestamp <= 0xFFFFFFFFFF) // 5 bytes (40 bits)
                {
                    // Convert the timestamp to a 5-byte array (big-endian)
                    byte[] timestampBytes = BitConverter.GetBytes(unixTimestamp);
                    // Truncate to the first 5 bytes
                    byte[] truncatedTimestamp = new byte[5];
                    Array.Copy(timestampBytes, truncatedTimestamp, 5);
                    fs.Write(timestampBytes, 0, 5);
                }
                else
                {
                    MessageBox.Show("Error: The UNIX timestamp exceeds 5 bytes.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Write number of comment blocks (2 16-byte blocks)
                fs.Write(new byte[] { 0x02 }, 0, 1);

                // Write comment data
                fs.Write(new byte[] {
                        0x44, 0x45, 0x58, 0x50, 0x45, 0x44, 0x49, 0x54, 0x49, 0x4F, 0x4E, 0x36, 0x34, 0x20, 0x56, 0x30,
                        0x31, 0x20, 0x42, 0x59, 0x20, 0x48, 0x4F, 0x4E, 0x4B, 0x45, 0x59, 0x4B, 0x4F, 0x4E, 0x47, 0x00
                    }, 0, 32);
                
                // Write note entry
                // Game Code
                fs.Write(note.gameCodeRaw, 0, 4);
                // Publisher Code
                fs.Write(note.pubCodeRaw, 0, 2);
                // Start page (0xCAFE since this is an extracted save)
                fs.Write(new byte[] { 0xCA, 0xFE }, 0, 2);
                // Status
                fs.Write(new byte[] { note.Status }, 0, 1);
                // Reserved/Data Sum (None of this crap matters)
                fs.Write(new byte[] { 0x00, 0x00, 0x00 }, 0, 3);
                // File Extension
                fs.Write(note.noteExtRaw, 0, 4);
                // File Name
                fs.Write(note.noteTitleRaw, 0, 16);
                
                // Write save data
                fs.Write(note.Data, 0, note.Data.Length);

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

            notes.RemoveAt(index);

            // Calculate new checksum for index table
            /*byte ckByte = 0;
            
            for(int i = 10; i < 256; i++)
            {
                ckByte += (byte)indexTable[i];
            }

            // Set the new checksum.
            indexTable[1] = ckByte;*/
            updateIndexTable();

        }

        public void SaveMPK(int comPort, string fileName)
        {
            using (FileStream fs = File.OpenWrite(fileName))
            {
                DexDrive drive = new DexDrive();
                if (drive.StartDexDrive($"COM{comPort}"))
                {
                    try
                    {
                        for (ushort i = 0; i < 128; i++)
                        {
                            // Read a frame from the memory card.
                            byte[] cardData = drive.ReadMemoryCardFrame(i);
                            fs.Write(cardData, 0, cardData.Length);
                        }
                        drive.StopDexDrive();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message +
                            "\nAre you sure your DexDrive is plugged in?" +
                            "\nTry disconnecting and reconnecting the power.",
                            "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        ErrorCode = 6;
                        ErrorStr = "Read Failed.";
                    }
                }
                else
                {
                    ErrorCode = 5;
                    ErrorStr = "Read from card failed.";
                }
            }
        }

        public void SaveMPK(string fileName, Mempak mpk, List<MPKNote> notes)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                // Write the header
                if (mpk.Header[0x39] == 0x00)
                {
                    // Header is fucked, build a new one.
                    byte[] header = BuildHeader();
                    fs.Write(header, 0, header.Length);
                }
                else fs.Write(mpk.Header, 0, mpk.Header.Length);

                // Write the Index Table
                //fs.Seek(0x100, SeekOrigin.Begin);
                for (int i = 0; i < 2; i++)
                {
                    foreach (short node in mpk.IndexTable)
                    {
                        fs.Write(new byte[] { (byte)((node >> 8) & 0xFF) }, 0, 1);
                        fs.Write(new byte[] { (byte)(node & 0xFF) }, 0, 1);
                    }
                }
                // Write the index table again
                /*foreach (short node in mpk.IndexTable)
                {
                    fs.Write(new byte[] { (byte)((node >> 8) & 0xFF) }, 0, 1);
                    fs.Write(new byte[] { (byte)(node & 0xFF) }, 0, 1);
                }*/

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
                    //fs.Write(note.noteExtRaw, 0, note.noteExtRaw.Length);
                    fs.Write(note.noteExtRaw, 0, 4);
                    // File Name
                    //fs.Write(note.noteTitleRaw, 0, note.noteTitleRaw.Length);
                    fs.Write(note.noteTitleRaw, 0, 16);
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

                // Write notes.
                int pageCount = 0;
                foreach(MPKNote note in notes)
                {
                    fs.Write(note.Data, 0, note.Data.Length);
                    pageCount += note.PageSize;
                }

                // Zero fill the rest of the card.
                for (int i = pageCount; i < 123; i++)
                {
                    fs.Write(DexDrive.BlankPage(), 0, Mempak.PageSize);
                }

                fs.Close();
                MessageBox.Show("File written successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    class MPKNote
    {
        public string GameCode { get; set; }
        public string PubCode { get; set; }
        public string NoteTitle { get; set; }
        public string MagicString { get; set; }
        public string NoteExtension { get; set; }

        public short StartPage;
        public byte Status;
        public byte Version;
        public byte[] Data;
        public byte[] rawData;
        public byte[] rawTimestamp = new byte[5];
        public int PageSize = 1;
        public byte CheckByte = 0;
        public const int EntrySize = 16;
        public List<short> Pages = new List<short>();
        public byte[] gameCodeRaw = new byte[4];
        public byte[] pubCodeRaw = new byte[2];
        public byte[] noteTitleRaw = new byte[16];
        public byte[] noteExtRaw = new byte[4];
        public byte[] startPageRaw = new byte[2];
        public byte[] NoteEntry = new byte[16];
        public byte[] noteHeader = new byte[32];
        //List<short> indexTable = new List<short>();
        
        public short Reserved { get; set; }
        public Int64 Timestamp { get; set; }
        public int commentBlocks { get; set; }
        public string Comment{ get; set; }

        public void ParseHeader(byte[] header)
        {
            using (MemoryStream stream = new MemoryStream(header))
            using (BinaryReader hr = new BinaryReader(stream))
            {

                // Read the Game Code
                gameCodeRaw = hr.ReadBytes(4);
                GameCode = Encoding.Default.GetString(gameCodeRaw);

                // Read the Publisher Code
                pubCodeRaw = hr.ReadBytes(2);
                PubCode = Encoding.Default.GetString(pubCodeRaw);

                // Read the Start Page
                StartPage = ByteSwap(hr.ReadInt16());

                // Read the Status byte.
                Status = hr.ReadByte();
                
                // Read and chuck the reserved/data sum bits.
                hr.ReadBytes(3);

                // Read the note extension
                noteExtRaw = hr.ReadBytes(4);

                // Read the note title.
                noteTitleRaw = hr.ReadBytes(16);
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
        }

        public Int16 ByteSwap(Int16 i)
        {
            return (Int16)(((i << 8) & 0xFF00) + ((i >> 8) & 0xFF));
        }

        public MPKNote(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                this.noteHeader = reader.ReadBytes(32);
                this.ParseHeader(noteHeader);
                Data = reader.ReadBytes(data.Length - 32).ToArray();
            }
        }

        public MPKNote() {
            // Basically a null constructor
            this.noteExtRaw = null;
            this.noteTitleRaw = null;
            this.NoteExtension = "";
            this.NoteTitle = "";
            this.PageSize = 1;
        }

        public MPKNote(string fileName)
        {
            this.rawData = File.ReadAllBytes(fileName);
            using (MemoryStream stream = new MemoryStream(this.rawData))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Version = reader.ReadByte();
                MagicString = Encoding.ASCII.GetString(reader.ReadBytes(7));
                Reserved = reader.ReadInt16();

                byte[] timestampBytes = reader.ReadBytes(5);
                Array.Reverse(timestampBytes);
                //Timestamp = BitConverter.ToUInt64(timestampBytes, 0); 
                // I don't give a shit about timestamps right now.
                // This is for inserting into a card.

                commentBlocks = reader.ReadByte();

                // Read comment data (each block is 16 bytes)
                byte[] commentDataBytes = reader.ReadBytes(16 * commentBlocks);
                Comment = Encoding.UTF8.GetString(commentDataBytes).TrimEnd('\0');
                noteHeader = reader.ReadBytes(32);
                this.ParseHeader(noteHeader);
                Data = reader.ReadBytes(this.rawData.Length - 0x10 - (0x10 * commentBlocks) - EntrySize).ToArray();
            }
        }


        public MPKNote(MemoryStream header, MemoryStream index)
        {
            // Set up readers for both streams
            BinaryReader ir = new BinaryReader(index);

            // Seek past the first 5 pages.
            ir.BaseStream.Position = 10;

            // Calculate the checksum
            for(int i = 10; i < 256; i++)
            {
                // Add the current byte to the Checksum
                CheckByte += ir.ReadByte();
            }

            this.ParseHeader(header.ToArray());
        }
    }
}
