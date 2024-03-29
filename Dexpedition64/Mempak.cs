﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Dexpedition64
{
    class Mempak
    {
        public int ErrorCode = 0;
        public int FirstFreePage = 5;
        public readonly int PageSize = 256;
        public readonly int CardSize = 32768;
        public readonly int TotalPages = 128;
        public readonly int NoteEntrySize = 32;
        private readonly short OpenPage = 0x0003;

        public string Label { get; set; }
        public string CheckSum1 {  get; set; }
        public string CheckSum2 { get; set; }
        public string RealCheckSum { get; set; }
        public string SerialNumber { get; set; }
        public string ErrorStr { get; set; }
        public byte[] Header { get; set; }
        public byte[] rawData { get; set; }
        public List<short> IndexTable = new List<short>();
        
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

        public int FreePages(List<MPKNote> notes)
        {
            int pagesUsed = 0;
            foreach(MPKNote note in notes)
            {
                pagesUsed += note.PageSize;
            }
            return 123 - pagesUsed;
        }

        public void FormatCard(int comPort)
        {
            System.Windows.Forms.DialogResult yn = MessageBox.Show(
                "Format Card?\nWARNING: ALL DATA WILL BE LOST.", 
                "Format Card?", MessageBoxButtons.YesNo);
            if (yn == System.Windows.Forms.DialogResult.No)
            {
                MessageBox.Show("Format cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (yn == System.Windows.Forms.DialogResult.Yes)
            {
                DexDrive drive = new DexDrive();
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

                        // Make a quick Index table and fill it with blanks.
                        List<short> table = new List<short>();
                        for (ushort i = 0; i < 128; i++) table.Add(this.OpenPage);

                        // Calculate the checksum.
                        byte ckByte = 0;
                        for (int i = 10; i < 128; i++) ckByte += (byte)table[i];
                        // Write the checksum to the index table.
                        table[1] = ckByte;

                        // Write the index table to the card. (Big-Endian)
                        for (ushort i = 1; i < 3; i++) 
                        {
                            if (!drive.WriteMemoryCardFrame(i, table.SelectMany(shortValue =>
                                BitConverter.GetBytes(shortValue).Reverse()).ToArray()))
                            {
                                showError(strFailed);
                                return;
                            }
                        }

                        // Clear the note table
                        for (ushort i = 3; i < 5; i++)
                        {
                            if (!drive.WriteMemoryCardFrame(i, DexDrive.BlankPage()))
                            {
                                showError(strFailed);
                                return;
                            }
                        }

                        // We could clear out the entire card here, but there's really no point.
                        // The index table has all pages marked as free, and the note table is empty.
                        // As far as the N64 is concerned, this is now an empty memory card.
                        drive.StopDexDrive();
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

        public static byte[] GenerateID()
        {
            byte[] IDBlock = new byte[32];
            byte[] serial = new byte[24];
            Random rnd = new Random();

            // Generate a random serial number
            rnd.NextBytes(serial);
            
            for (int i = 0; i < serial.Length; i++) IDBlock[i] = serial[i];
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

        private static bool IsText(string input)
        {
            // The first part of this before the || is basically isControl(), but ignoring zeroes.
            foreach (char c in input) if (((c != 0x00) && (c < 0x20)) || (c > 0x7F)) return false;
            return true;
        }

        public static byte[] BuildHeader(string Label)
        {
            // If the label isn't ASCII, default it.
            if (!IsText(Label)) Label = "Dexpedition64 V01 by HonkeyKong";
            byte[] header = new byte[256];
            byte[] idBlock = GenerateID();

            // Just in case some smartass finds a way to 
            // cram more than 32 characters in the text box...
            byte[] LabelBytes = new byte[32];
            if(Label.Length >= 32) Array.Copy(Encoding.ASCII.GetBytes(Label), LabelBytes, 32);
            else Array.Copy(Encoding.ASCII.GetBytes(Label), LabelBytes, Label.Length);

            // Loop through and write each byte
            for (int i = 0; i < 32; i++)
            {
                header[i] = LabelBytes[i];
                header[i + 0x20] = idBlock[i];
                header[i + 0x40] = LabelBytes[i];
                header[i + 0x60] = idBlock[i];
                header[i + 0x80] = idBlock[i];
                header[i + 0xA0] = 0x00;
                header[i + 0xC0] = idBlock[i];
                header[i + 0xE0] = LabelBytes[i];
            }
            return header;
        }

        public static byte[] BuildHeader()
        {
            return BuildHeader("Dexpedition64 V01 By HonkeyKong");
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

        private void UpdateIndexTable()
        {
            // Calculate and set the new checksum.
            byte ckByte = 0;
            for (int i = 10; i < 128; i++) ckByte += (byte)this.IndexTable[i];
            this.IndexTable[1] = ckByte;
        }

        public void RebuildIndexTable(List<MPKNote> notes)
        {
            short page = 5;
            for (int i = 0; i < 128; i++)
            {
                this.IndexTable[i] = 3;
            }

            int totalPages = 0;
            foreach (MPKNote note in notes)
            {
                note.StartPage = page;
                for (int i = 0; i < note.PageSize - 1; i++)
                {
                    // Write the next block in sequence.
                    this.IndexTable[page] = (short)(page + 1);
                    page++;
                }
                // Write a 1 for the last page.
                this.IndexTable[page] = 1;
                page++;
                totalPages += note.PageSize;
            }

            this.FirstFreePage = 5 + totalPages;

            UpdateIndexTable();
        }

        public void ReadHeaderPage(byte[] page)
        {
            MemoryStream PageStream = new MemoryStream(page);
            BinaryReader br = new BinaryReader(PageStream);
            // Make a card label.
            byte[] cardLabel = new byte[32];
            // Read the card label.
            cardLabel = br.ReadBytes(32);

            // Read the serial number
            byte[] serial = br.ReadBytes(28);

            // Read the checksums
            short checkSum1 = br.ReadInt16();
            short checkSum2 = br.ReadInt16();

            // Convert our checksums to strings
            this.CheckSum1 = checkSum1.ToString("X2");
            this.CheckSum2 = checkSum2.ToString("X2");

            // Calculate the actual checksum
            ushort realCkSum = CalculateChecksum(serial);

            // The result should be big-endian. Swap it.
            this.RealCheckSum = ByteSwap((short)realCkSum).ToString("X2");

            // We're sanity checking this shit right here.
            // Label Backup #1
            byte[] LabelBackup1Bytes = br.ReadBytes(32);
            // ID Block Backups
            byte[] idBlockBackup = br.ReadBytes(32);
            byte[] idBlockBackup2 = br.ReadBytes(32);
            /// According to bryc, this is used as a scratch area
            /// in some games, so we're just going to ignore it.
            /// The label is apparently also used as a scratch area
            /// sometimes, that's why I now make backups.
            br.ReadBytes(32);
            // ID Block Backup 3
            byte[] idBlockBackup3 = br.ReadBytes(32);
            // Second label backup
            byte[] LabelBackup2Bytes = br.ReadBytes(32);

            // Now comes the fun part. Sanity checking all this shit.
            if ((!IsText(Encoding.ASCII.GetString(cardLabel))) && ((LabelBackup1Bytes[0] != 0x00) || (LabelBackup2Bytes[0] != 0x00)))
            {
                // Label is probably fucked, restore from backup?
                if ((LabelBackup1Bytes.SequenceEqual(LabelBackup2Bytes)) && (IsText(Encoding.ASCII.GetString(LabelBackup1Bytes))))
                {
                    // Looks like we're actually using my backup section.
                    MessageBox.Show("Card label is corrupt. Attempting to repair with backup 1.", "Bad Label", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Array.Copy(LabelBackup1Bytes, cardLabel, LabelBackup1Bytes.Length);
                    this.Label = Encoding.UTF8.GetString(LabelBackup1Bytes);
                }
                else // Backup 1 might be fucked too. Copy from backup 2, I guess?
                {
                    MessageBox.Show("Card label and first backup appear to be corrupt. Attempting to repair label from backup #2.", "Bad Label", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (IsText(Encoding.ASCII.GetString(LabelBackup2Bytes)))
                    {
                        MessageBox.Show("Successfully repaired card label.", "Repair Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Array.Copy(LabelBackup2Bytes, cardLabel, LabelBackup2Bytes.Length);
                        this.Label = Encoding.UTF8.GetString(LabelBackup2Bytes);
                    }
                    else
                    {
                        MessageBox.Show("Repair failed, possibly no backup. Using default label.", "Repair failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        this.Label = "Dexpedition64 v01 By HonkeyKong";
                    }
                }
            }

            // Set the label
            this.Label = Encoding.ASCII.GetString(cardLabel);

            // Sanity check the ID block backups to make sure they match.
            // If they don't, just throw a warning. It'll get rebuilt anyway.
            //! This happens way too often, and is actually really annoying.
            /*if ((!idBlockBackup.SequenceEqual(idBlockBackup2))) || (!idBlockBackup.SequenceEqual(idBlockBackup3)))
            {
                MessageBox.Show("ID Block backups don't match.\n" +
                    "This could mean your original ID block is corrupt,\n" +
                    "or your backups are just messed up.\n" +
                    "This will be fixed when saving an MPK or rewriting the card.",
                    "ID Mismatch!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }*/

            // Convert our serial number to a hex string.
            this.SerialNumber = string.Join("", serial.Select(hex => string.Format("{0:X2}", hex)));
        }

        public Mempak()
        {
            // Initialize the fields and properties for a blank Mempak object
            this.Type = CardType.CARD_VIRTUAL;
            this.CheckSum1 = string.Empty;
            this.CheckSum2 = string.Empty;
            this.RealCheckSum = string.Empty;
            this.SerialNumber = string.Empty;
            this.IndexTable = new List<short>();
            this.FirstFreePage = 5;

            byte[] Header = BuildHeader();
            byte[] serial = new byte[28];
            
            for(int i = 0; i < 28; i++) serial[i] = Header[i + 0x20];

            this.SerialNumber = string.Join("", serial.Select(hex => string.Format("{0:X2}", hex)));

            // Write the Index table.
            for (int i = 0; i < 128; i++) this.IndexTable.Add(OpenPage);

            UpdateIndexTable();
        }

        public Mempak(string fileName, List<MPKNote> notes)
        {
            rawData = File.ReadAllBytes(fileName);
            using (FileStream fs = File.OpenRead(fileName))
            {
                // Set up a reader for the file
                BinaryReader br = new BinaryReader(fs);
                
                // Set the default seek position
                short SeekPos = 0x00;

                // Check for the existence of a magic string.
                byte[] MagicString = br.ReadBytes(12);
                string HeaderString = Encoding.ASCII.GetString(MagicString);

                // If this string exists, we've got a DexDrive save. Jump up the Seek Position.
                if (HeaderString.Equals("123-456-STD\0", StringComparison.Ordinal)) SeekPos = 0x1040;
                
                // Rewind the file to the start of the MPK.
                fs.Seek(SeekPos, SeekOrigin.Begin);

                this.ReadHeaderPage(br.ReadBytes(256));

                // Read the Index Table
                for (int i = 0; i < 128; i ++) IndexTable.Add(ByteSwap(br.ReadInt16()));
                
                // Let's read the backup and compare it to the 
                // first index table. Sanity checking for the win!
                List<short> IndexTable2 = new List<short>();
                for (int i = 0; i < 128; i++) IndexTable2.Add(ByteSwap(br.ReadInt16()));

                if (!IndexTable.SequenceEqual(IndexTable2)) MessageBox.Show(
                    "Index and backup Index tables do not match.\n" +
                    "File system errors could be ahead.", "WARNING!", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Recalculate the checksum, that's all this function does.
                UpdateIndexTable();

                int totalPages = 0;
                
                // Read the Note Table
                for (int i = 0; i < 16; i++)
                {
                    // Store the current position before reading the note entry
                    short SavedPosition = (short)fs.Position;

                    MemoryStream header = new MemoryStream(br.ReadBytes(32));
                    if (header.ToArray()[0] != 0x00)
                    {
                        MPKNote note = new MPKNote();
                        note.ParseHeader(header.ToArray());
                        /// It seems like some games just zero out the start page
                        /// and start a new note entry, or is this a DexDrive thing?
                        /// Either way, this should fix invalid note table entries.
                        if (note.StartPage >= 5)
                        {
                            MemoryStream noteStream = new MemoryStream();
                            BinaryWriter noteData = new BinaryWriter(noteStream);

                            short nextNote = note.StartPage;
                            short lastNote = note.StartPage;

                            while (nextNote != 0x0001)
                            {
                                // Set the position to read the note data
                                fs.Position = SeekPos + (this.PageSize * nextNote);
                                noteData.Write(br.ReadBytes(this.PageSize));
                                note.Pages.Add(nextNote);
                                lastNote = nextNote; // Update lastNote
                                nextNote = IndexTable[nextNote];
                            }

                            // Calculate the page size based on the difference between last page and start page
                            note.PageSize = lastNote - note.StartPage + 1;
                            note.Data = noteStream.ToArray();
                            totalPages += note.PageSize;
                            notes.Add(note);
                        }
                    }

                    // Restore the position to read the next note entry
                    fs.Position = SavedPosition + 32; // Move past the header of the next note entry
                }
                this.FirstFreePage = 5 + totalPages;
            }
            this.Type = CardType.CARD_VIRTUAL;
        }

        public Mempak(int comPort, List<MPKNote> notes, ProgressBar pBar)
        {
            this.Type = Mempak.CardType.CARD_PHYSICAL;
            DexDrive drive = new DexDrive();
            pBar.Minimum = 0;
            pBar.Maximum = 128;
            pBar.Value = 0;
            pBar.Step = 1;
            if (!drive.StartDexDrive($"COM{comPort}"))
            {
                ErrorCode = 1;
                ErrorStr = $"Failed to initialize DexDrive on COM{comPort}.";
                return;
            }

            try
            {
                this.ReadHeaderPage(drive.ReadMemoryCardFrame(0));
                pBar.PerformStep();

                MemoryStream indexTable = new MemoryStream(drive.ReadMemoryCardFrame(1).ToArray().Concat(drive.ReadMemoryCardFrame(2)).ToArray());
                pBar.PerformStep(); pBar.PerformStep();
                
                MemoryStream noteTable = new MemoryStream(drive.ReadMemoryCardFrame(3).ToArray().Concat(drive.ReadMemoryCardFrame(4)).ToArray());
                pBar.PerformStep(); pBar.PerformStep();

                BinaryReader br = new BinaryReader(indexTable);
                
                // Populate our Index Table list
                for(int i = 0; i < 128; i++) IndexTable.Add(ByteSwap(br.ReadInt16()));

                // Populate the Index Table Backup
                List<short> IndexTable2 = new List<short>();
                for (int i = 0; i < 128; i++) IndexTable2.Add(ByteSwap(br.ReadInt16()));

                // Read out the rest of the card into RAM.
                byte[] Data = new byte[this.CardSize - (this.PageSize * 5)];
                MemoryStream dataStream = new MemoryStream(Data);
                BinaryWriter cardWriter = new BinaryWriter(dataStream);
                for(ushort i = 5; i < 128; i++)
                {
                    cardWriter.Write(drive.ReadMemoryCardFrame(i));
                    pBar.PerformStep();
                }

                // One last set of reads, just to get the first 5 pages into RAM.
                // While we're at it, concatenate the rest of the data as well.
                // BAM. Raw card image.
                rawData = drive.ReadMemoryCardFrame(0).Concat(
                    drive.ReadMemoryCardFrame(1).Concat(
                        drive.ReadMemoryCardFrame(2).Concat(
                            drive.ReadMemoryCardFrame(3).Concat(
                                drive.ReadMemoryCardFrame(4).Concat(Data))))).ToArray();
                
                // We're done with the drive now, I think.
                drive.StopDexDrive();

                // Read the Note Table
                BinaryReader nr = new BinaryReader(noteTable);
                for (int i = 0; i < 16; i++)
                {
                    MemoryStream header = new MemoryStream(nr.ReadBytes(32));
                    MPKNote note = new MPKNote();
                    if (header.ToArray()[0] != 0x00)
                    {
                        note.ParseHeader(header.ToArray());
                        if (note.StartPage >= 5)
                        {
                            MemoryStream noteStream = new MemoryStream();
                            BinaryWriter noteData = new BinaryWriter(noteStream);

                            if (IndexTable[note.StartPage] == 0x0001)
                            {
                                noteData.Write(Data, this.PageSize * (note.StartPage - 5), this.PageSize);
                                note.PageSize = 1;
                            }
                            else
                            {
                                short nextNote = note.StartPage;
                                while (IndexTable[nextNote] != 0x0001)
                                {
                                    note.PageSize++;
                                    noteData.Write(Data, this.PageSize * (nextNote - 5), this.PageSize);
                                    note.Pages.Add(nextNote);
                                    nextNote++;
                                }
                                if (IndexTable[nextNote] == 0x0001)
                                {
                                    noteData.Write(Data, this.PageSize * (nextNote - 4), this.PageSize);
                                }
                            }
                            note.Data = noteStream.ToArray();
                            notes.Add(note);
                        }
                    }
                }
                if (!IndexTable.SequenceEqual(IndexTable2)) MessageBox.Show(
                    "Index and backup Index tables do not match.\n" +
                    "File system errors could be ahead.", "WARNING!",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch(ArgumentNullException)
            {
                ErrorCode = 6;
                ErrorStr = $"Couldn't read card. Did you forget to put it in the reader?";
                return;
            }
            catch (Exception ex)
            {
                ErrorCode = 2;
                ErrorStr = $"{ex.Message}\nType: {ex.GetType()}\nSource: {ex.Source}";
                return;
            }
        }

        public bool ImportNote(MPKNote note, List<MPKNote> notes)
        {
            if(this.Type == CardType.CARD_NONE)
            {
                MessageBox.Show("Please load or create an MPK file first.", "No File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            note.PageSize = note.Data.Length / PageSize;
            if(note.PageSize > this.FreePages(notes))
            {
                MessageBox.Show($"File is too large.\nDelete {note.PageSize - this.FreePages(notes)} pages to free up enough space.\nFile size: {note.PageSize}, Free pages: {this.FreePages(notes)}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            
            if(notes.Count > 16)
            {
                MessageBox.Show("How the hell did you get more than 16 notes? Fixing that... *grumble grumble*", "What?!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                while (notes.Count > 16) notes.RemoveAt(16);
                return false;
            }
            
            if(notes.Count == 16)
            {
                MessageBox.Show("Index table is full. Delete at least one file to make room.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            note.StartPage = (short)this.FirstFreePage;
            this.FirstFreePage += note.PageSize;
            notes.Add(note);
            this.RebuildIndexTable(notes);
            return true;
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
                    byte[] timestampBytes = BitConverter.GetBytes(unixTimestamp);
                    // Truncate to the first 5 bytes
                    byte[] truncatedTimestamp = new byte[5];
                    Array.Copy(timestampBytes, truncatedTimestamp, 5);
                    Array.Reverse(truncatedTimestamp);
                    fs.Write(truncatedTimestamp, 0, 5);
                }
                else
                {
                    MessageBox.Show("Error: The UNIX timestamp exceeds 5 bytes.", "what the fuck", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Write number of comment blocks (2 16-byte blocks in our case)
                fs.Write(new byte[] { 0x02 }, 0, 1);

                // Write comment data (New Hotness!)
                string comment = "Dexpedition64 V01 By HonkeyKong";
                byte[] commentBytes = new byte[32];
                Array.Copy(Encoding.ASCII.GetBytes(comment), commentBytes, comment.Length);
                
                // Write it to the file.
                fs.Write(commentBytes, 0, commentBytes.Length);

                // Write note entry
                // Game Code
                fs.Write(note.GameCodeRaw, 0, 4);
                // Publisher Code
                fs.Write(note.PubCodeRaw, 0, 2);
                // Start page (0xCAFE since this is an extracted save)
                fs.Write(new byte[] { 0xCA, 0xFE }, 0, 2);
                // Status
                fs.Write(new byte[] { note.Status }, 0, 1);
                // Reserved/Data Sum (None of this crap matters)
                fs.Write(new byte[] { 0x00, 0x00, 0x00 }, 0, 3);
                // File Extension
                fs.Write(note.NoteExtRaw, 0, 4);
                // File Name
                fs.Write(note.NoteTitleRaw, 0, 16);
                
                // Write save data
                fs.Write(note.Data, 0, note.Data.Length);

                // Close the file
                fs.Close();
                MessageBox.Show("Note saved.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void DeleteNote(int index, List<MPKNote> notes)
        {
            notes.RemoveAt(index);
            this.RebuildIndexTable(notes);
        }

        public void SaveMPK(string fileName, Mempak mpk, List<MPKNote> notes)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                // Write a new header with a new serial number,
                // so the N64 doesn't get confused in multiplayer.
                fs.Write(BuildHeader(mpk.Label), 0, mpk.PageSize);

                // Write the Index Table
                for (int i = 0; i < 2; i++)
                {
                    foreach (short node in mpk.IndexTable)
                    {
                        fs.Write(new byte[] { (byte)((node >> 8) & 0xFF) }, 0, 1);
                        fs.Write(new byte[] { (byte)(node & 0xFF) }, 0, 1);
                    }
                }

                // Write note table
                foreach (MPKNote note in notes)
                {
                    // Game Code
                    fs.Write(note.GameCodeRaw, 0, note.GameCodeRaw.Length);
                    // Publisher Code
                    fs.Write(note.PubCodeRaw, 0, note.PubCodeRaw.Length);
                    // Start Page
                    fs.Write(new byte[] { (byte)((note.StartPage >> 8) & 0xFF), (byte)(note.StartPage & 0xFF) }, 0, 2);
                    // Status
                    fs.Write(new byte[] { note.Status }, 0, 1);
                    // Reserved/Data Sum (Three zeroes)
                    fs.Write(new byte[] { 0x00, 0x00, 0x00 }, 0, 3);
                    // File Extension
                    fs.Write(note.NoteExtRaw, 0, 4);
                    // File Name
                    fs.Write(note.NoteTitleRaw, 0, 16);
                }

                byte[] zeroes = new byte[32];
                // Compensate for < 16 notes by zero filling.
                if (notes.Count < 16) for (int i = 0; i < 16 - notes.Count; i++) fs.Write(zeroes, 0, zeroes.Length);

                // Write notes.
                foreach(MPKNote note in notes) fs.Write(note.Data, 0, note.Data.Length);

                // Zero fill the rest of the card.
                zeroes = new byte[256];
                while (fs.Position < CardSize) fs.Write(zeroes, 0, 256);

                fs.Close();
                MessageBox.Show("File written successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    class MPKNote
    {
        public int PageSize = 1;
        public readonly int EntrySize = 16;
        public string GameCode { get; set; }
        public string PubCode { get; set; }
        public string NoteTitle { get; set; }
        public string MagicString { get; set; }
        public string NoteExtension { get; set; }
        public string Comment{ get; set; }
        public int CommentBlocks { get; set; }
        public byte Status { get; set; }
        public byte Version { get; set; }
        public short Reserved { get; set; }
        public short StartPage { get; set; }
        public byte[] Data { get; set; }
        public byte[] RawData { get; set; }
        public byte[] GameCodeRaw { get; set; }
        public byte[] PubCodeRaw { get; set; }
        public byte[] NoteTitleRaw { get; set; }
        public byte[] NoteExtRaw { get; set; }
        public byte[] NoteHeader { get; set; }
        public List<short> Pages = new List<short>();

        public void ParseHeader(byte[] header)
        {
            using (MemoryStream stream = new MemoryStream(header))
            using (BinaryReader hr = new BinaryReader(stream))
            {

                // Read the Game Code
                GameCodeRaw = hr.ReadBytes(4);
                GameCode = Encoding.ASCII.GetString(GameCodeRaw);

                // Read the Publisher Code
                PubCodeRaw = hr.ReadBytes(2);
                PubCode = Encoding.ASCII.GetString(PubCodeRaw);

                // Read the Start Page
                StartPage = ByteSwap(hr.ReadInt16());

                // Read the Status byte.
                Status = hr.ReadByte();
                
                // Read and chuck the reserved/data sum bits.
                hr.ReadBytes(3);

                // Read the note extension
                NoteExtRaw = hr.ReadBytes(4);

                // Read the note title.
                NoteTitleRaw = hr.ReadBytes(16);
            }

            for (int i = 0; i < 4; i++)
            {
                try
                {
                    NoteExtension += Mempak.N64Symbols[NoteExtRaw[i]];
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
                    NoteTitle += Mempak.N64Symbols[NoteTitleRaw[i]];
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

        public MPKNote() {
            // Basically a null constructor
            this.NoteExtRaw = null;
            this.NoteTitleRaw = null;
            this.NoteExtension = "";
            this.NoteTitle = "";
            this.PageSize = 1;
        }

        public MPKNote(string fileName)
        {
            this.RawData = File.ReadAllBytes(fileName);
            using (MemoryStream stream = new MemoryStream(this.RawData))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Version = reader.ReadByte();
                MagicString = Encoding.ASCII.GetString(reader.ReadBytes(7));
                Reserved = reader.ReadInt16();

                byte[] timestampBytes = reader.ReadBytes(5);
                Array.Reverse(timestampBytes);

                CommentBlocks = reader.ReadByte();

                // Read comment data (each block is 16 bytes)
                byte[] commentDataBytes = reader.ReadBytes(16 * CommentBlocks);
                Comment = Encoding.UTF8.GetString(commentDataBytes).TrimEnd('\0');
                NoteHeader = reader.ReadBytes(32);
                this.ParseHeader(NoteHeader);
                Data = reader.ReadBytes(this.RawData.Length - 0x10 - (0x10 * CommentBlocks) - EntrySize).ToArray();
            }
        }
    }
}
