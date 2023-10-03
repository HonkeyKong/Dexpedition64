using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Linq;

namespace Dexpedition64
{
    public partial class frmMempak : Form
    {
        private bool fileLoaded = false;
        private bool cardRead = false;
        Mempak mpk;
        List<MPKNote> mPKNotes = new List<MPKNote>();
        private BackgroundWorker formatWorker = new BackgroundWorker();
        private BackgroundWorker writeWorker = new BackgroundWorker();
        private BackgroundWorker readWorker = new BackgroundWorker();

        public frmMempak()
        {
            InitializeComponent();
        }

        private void PopulateManager()
        {
            lblLabel.Text = $"Label: {mpk.Label}";
            lblSerial.Text = $"Serial: {mpk.SerialNumber}";
            lblCkSum1.Text = $"Checksum:\n0x{mpk.CheckSum1:X4}, {(mpk.RealCheckSum == mpk.CheckSum1 ? "OK" : "BAD")}";
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog mpkFile = new OpenFileDialog
            {
                Filter = "N64 Controller Paks/DexDrive Saves (*.mpk;*.n64)|*.mpk;*.n64|All files (*.*)|*.*",
                FilterIndex = 1
            };

            if (mpkFile.ShowDialog() == DialogResult.OK)
            {
                
                // Clear the lists
                lstNotes.Items.Clear();
                mPKNotes.Clear();

                mpk = new Mempak(mpkFile.FileName, mPKNotes);
                RefreshNoteList();

                fileLoaded = true;
                mpk.Type = Mempak.CardType.CARD_VIRTUAL;
                
                if(mpk.ErrorCode != 0)
                {
                    MessageBox.Show("Error: " + mpk.ErrorStr, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void RefreshNoteList()
        {
            // Clear the lists
            PopulateManager();
            lstNotes.Items.Clear();

            // Populate the listbox with current data
            foreach (MPKNote note in mPKNotes)
            {
                string NoteEntry = $"{note.GameCode}-{note.PubCode} - {note.NoteTitle}.{note.NoteExtension} - Page {note.StartPage.ToString()}, {note.PageSize} {(note.PageSize == 1 ? "page." : "pages.")}";
                lstNotes.Items.Add(NoteEntry);
            }

        }

        private void DumpCard()
        {
            //lblStatus.Text = "Reading Card...";
            pbCardProgress.Value = 0;
            pbCardProgress.Step = 1;
            DexDrive drive = new DexDrive();
            MemoryStream cardStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(cardStream);
            try
            {
                if (drive.StartDexDrive($"COM{int.Parse(cbComPort.Text)}"))
                {
                    try
                    {
                        for (ushort i = 0; i < 128; i++)
                        {
                            // Read a frame from the memory card.
                            byte[] cardData = drive.ReadMemoryCardFrame(i);
                            if (i > 4) writer.Write(cardData);
                            //fs.Write(cardData, 0, cardData.Length);
                            pbCardProgress.PerformStep();
                        }
                        mpk.Data = cardStream.ToArray();
                        drive.StopDexDrive();
                        cardRead = true;
                        MessageBox.Show("Card Read.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message +
                            "\nAre you sure your DexDrive is plugged in?" +
                            "\nTry disconnecting and reconnecting the power.",
                            "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        //lblStatus.Text = "Read Failed.";
                    }
                }
                else
                {
                    MessageBox.Show("Read Failed.", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } catch(FormatException ex)
            {
                MessageBox.Show("COM Port must be a number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Now that we've read the card, read the note data into each respective note.
            foreach(MPKNote note in mPKNotes)
            {
                MemoryStream noteData = new MemoryStream();
                BinaryWriter noteWriter = new BinaryWriter(noteData);
                if (note.Pages.Count == 1) noteWriter.Write(mpk.Data, Mempak.PageSize * (note.StartPage - 5), Mempak.PageSize);
                else foreach (short page in note.Pages) noteWriter.Write(mpk.Data, (Mempak.PageSize * (page - 5)), Mempak.PageSize);
                note.Data = noteData.ToArray();
            }
        }

        private void frmMempak_Load(object sender, EventArgs e)
        {
            // This is just here because I keep clicking the damn form.
        }


        private void btnExport_Click(object sender, EventArgs e)
        {
            if (!fileLoaded)
            {
                MessageBox.Show("Load something first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            if (lstNotes.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select a note first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            MPKNote currentNote = mPKNotes[lstNotes.SelectedIndex];

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "N64 Save Notes (*.note)|*.note|All files (*.*)|*.*",
                FilterIndex = 0,
                FileName = currentNote.NoteExtension != "" ? currentNote.NoteTitle + "." + currentNote.NoteExtension : currentNote.NoteTitle + ".note"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (mpk.Type == Mempak.CardType.CARD_PHYSICAL)
                    {
                        if (!cardRead) DumpCard();
                    }
                } catch(FormatException) {
                    MessageBox.Show("COM Port should be a number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                mpk.SaveNote(currentNote, saveFileDialog.FileName);
            }
            if(mpk.ErrorCode != 0)
            {
                MessageBox.Show("Error: " + mpk.ErrorStr, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            if(fileLoaded == false)
            {
                MessageBox.Show("Create or load an MPK first.");
                return;
            }
            
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "N64 Save Notes (*.note)|*.note|All files (*.*)|*.*",
                FilterIndex = 0
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                MPKNote note = new MPKNote(openFileDialog.FileName);
                mpk.ImportNote(note, mPKNotes);
            }
            
            if (mpk.ErrorCode != 0)
            {
                MessageBox.Show("Error: " + mpk.ErrorStr, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            RefreshNoteList();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Delete the selected note entry from the note table
            // Mark each used page as free (0x03) on the index table

            try
            {
                /*if (mpk.Type == Mempak.CardType.CARD_VIRTUAL)
                {*/
                    mpk.DeleteNote(lstNotes.SelectedIndex, mPKNotes, mpk.IndexTable);
                    lstNotes.Items.Remove(lstNotes.Items[lstNotes.SelectedIndex]);
                /*}
                else if(mpk.Type == Mempak.CardType.CARD_PHYSICAL)
                {
                    MessageBox.Show("Deleting from physical cards is not supported yet.");
                }*/
            } 
            catch(ArgumentOutOfRangeException)
            {
                MessageBox.Show("Please select a note first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            lstNotes.Refresh();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "N64 Memory Paks (*.mpk)|*.mpk|All files (*.*)|*.*",
                FilterIndex = 0,
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if((mpk.Type == Mempak.CardType.CARD_VIRTUAL) && (fileLoaded))
                {
                    mpk.SaveMPK(saveFileDialog.FileName, mpk, mPKNotes);
                }
                else if(mpk.Type == Mempak.CardType.CARD_PHYSICAL && (fileLoaded))
                {
                    try
                    {
                        if (!cardRead) DumpCard();
                        //mpk.SaveMPK(int.Parse(cbComPort.Text), saveFileDialog.FileName);
                        mpk.SaveMPK(saveFileDialog.FileName, mpk, mPKNotes);
                    }
                    catch (FormatException)
                    {
                        mpk.ErrorStr = "COM Port should be a number.";
                        mpk.ErrorCode = 1;
                        return;
                    }
                    catch (ArgumentNullException)
                    {
                        mpk.ErrorStr = "COM Port should contain a value.";
                        mpk.ErrorCode = 6;
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Load a Memory Pak first.");
                    return;
                }
            }
            if(mpk.ErrorCode != 0)
            {
                MessageBox.Show("Error: " + mpk.ErrorStr, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void btnFormat_Click(object sender, EventArgs e)
        {
            // Don't forget to clean up from any previously open files.
            mPKNotes.Clear();
            lstNotes.Items.Clear();
            mpk = new Mempak();
            mpk.FormatCard(int.Parse(cbComPort.Text));
        }


        private void btnNew_Click(object sender, EventArgs e)
        {
            mpk = new Mempak();
            
            // Don't forget to clean up from any previously open files.
            mPKNotes.Clear();
            lstNotes.Items.Clear();
            PopulateManager();
            fileLoaded = true;
        }

        private void btnReadCard_Click(object sender, EventArgs e)
        {
            try
            {
                mPKNotes.Clear();
                try
                {
                    mpk = new Mempak(int.Parse(cbComPort.Text), mPKNotes);
                    cardRead = true;
                }
                catch (FormatException)
                {
                    mpk.ErrorStr = "COM Port should be a number.";
                    mpk.ErrorCode = 1;
                    return;
                } catch (ArgumentNullException)
                {
                    mpk.ErrorStr = "COM Port should contain a value.";
                    mpk.ErrorCode = 6;
                    return;
                }

                RefreshNoteList();

                // If there was an error, show it.
                if(mpk.ErrorCode != 0 ) MessageBox.Show("Error: " + mpk.ErrorStr, "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message +
                    "\nAre you sure your DexDrive is plugged in?" +
                    "\nTry disconnecting and reconnecting the power.",
                    "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            fileLoaded = true;
            mpk.Type = Mempak.CardType.CARD_PHYSICAL;
        }

        private void btnWriteCard_Click(object sender, EventArgs e)
        {
            /*MainForm mfrm = new MainForm();
            mfrm.Show();*/
            /*OpenFileDialog writeFile = new OpenFileDialog();
            writeFile.Filter = "Memory Pak files (*.mpk)|*.mpk|All files (*.*)|*.*";
            writeFile.FilterIndex = 0;*/

            if (!fileLoaded)
            {
                MessageBox.Show("Load a file or open a card first.");
                return;
            }

            pbCardProgress.Value = 0;
            pbCardProgress.Step = 1;
            DexDrive drive = new DexDrive();
            if (drive.StartDexDrive($"COM{cbComPort.Text}"))
            {
                MemoryStream cardData = new MemoryStream(mpk.Data);
                BinaryReader br = new BinaryReader(cardData);
                byte[] cardBuf = new byte[256];

                // Write the label and ID block.
                //drive.WriteMemoryCardFrame(0, mpk.Header.ToArray());
                drive.WriteMemoryCardFrame(0, Mempak.BuildHeader());

                //drive.WriteMemoryCardFrame(1, mpk.IndexTable.ToArray());

                byte[] indexBytes = mpk.IndexTable.ToArray().SelectMany(shortValue => BitConverter.GetBytes(shortValue).Reverse()).ToArray();
                drive.WriteMemoryCardFrame(1, indexBytes);
                drive.WriteMemoryCardFrame(2, indexBytes);

                // Write the note table
                MemoryStream noteTable = new MemoryStream();
                BinaryWriter ntWriter = new BinaryWriter(noteTable);

                foreach (MPKNote note in mPKNotes)
                {
                    // Game Code
                    ntWriter.Write(note.gameCodeRaw, 0, note.gameCodeRaw.Length);
                    // Publisher Code
                    ntWriter.Write(note.pubCodeRaw, 0, note.pubCodeRaw.Length);
                    // Start Page
                    ntWriter.Write(new byte[] { (byte)((note.StartPage >> 8) & 0xFF), (byte)(note.StartPage & 0xFF) }, 0, 2);
                    // Status
                    ntWriter.Write(new byte[] { note.Status }, 0, 1);
                    // Reserved/Data Sum (Three zeroes)
                    ntWriter.Write(new byte[] { 0x00, 0x00, 0x00 }, 0, 3);
                    // File Extension
                    //fs.Write(note.noteExtRaw, 0, note.noteExtRaw.Length);
                    ntWriter.Write(note.noteExtRaw, 0, 4);
                    // File Name
                    //fs.Write(note.noteTitleRaw, 0, note.noteTitleRaw.Length);
                    ntWriter.Write(note.noteTitleRaw, 0, 16);
                }

                // Compensate for < 16 notes by zero filling.
                if (mPKNotes.Count < 16)
                {
                    for (int i = 0; i < 16 - mPKNotes.Count; i++)
                    {
                        for (int j = 0; j < 32; j++)
                        {
                            ntWriter.Write(new byte[] { 0x00 }, 0, 1);
                        }
                    }
                }
                BinaryReader ntReader = new BinaryReader(noteTable);
                ntReader.BaseStream.Position = 0;
                // Write the Note Table to card.
                drive.WriteMemoryCardFrame(3, ntReader.ReadBytes(Mempak.PageSize));
                drive.WriteMemoryCardFrame(4, ntReader.ReadBytes(Mempak.PageSize));

                // Write the card data
                for (ushort i = 5; i < 128; i++)
                {
                    // Read a frame from the file.
                    cardBuf = br.ReadBytes(256);

                    if (!drive.WriteMemoryCardFrame(i, cardBuf))
                    {
                        MessageBox.Show("Writing frame failed.", "Write Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    else
                    {
                        pbCardProgress.PerformStep();
                    }

                    cardBuf.Initialize();
                }
                drive.StopDexDrive();
                MessageBox.Show("Card Written.", "Write Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Writing card failed.", "Write Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lstNotes_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
