using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

namespace Dexpedition64
{
    public partial class frmMempak : Form
    {
        private bool fileLoaded = false;
        Mempak mpk;
#pragma warning disable IDE0044 // Add readonly modifier
        List<MPKNote> mPKNotes = new List<MPKNote>();
#pragma warning restore IDE0044 // Add readonly modifier

        public frmMempak()
        {
            InitializeComponent();
        }

        private void PopulateManager()
        {
            lblLabel.Text = $"Label: {mpk.Label}";
            lblSerial.Text = $"Serial: {mpk.SerialNumber}";
            lblCkSum1.Text = $"Checksum: 0x{mpk.CheckSum1:X4}";
            if (mpk.RealCheckSum == mpk.CheckSum1) lblCkSum1.ForeColor = System.Drawing.Color.Green;
            else lblCkSum1.ForeColor = System.Drawing.Color.Red;
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

                mpk = new Mempak(mpkFile.FileName, mPKNotes) { Type = Mempak.CardType.CARD_VIRTUAL };
                RefreshNoteList();

                fileLoaded = true;
                
                if(mpk.ErrorCode != 0)
                {
                    MessageBox.Show("Error: " + mpk.ErrorStr, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    mpk.ErrorCode = 0;
                }
            }
        }

        private void RefreshNoteList()
        {
            // Clear the lists
            PopulateManager();
            lstNotes.Items.Clear();
            lblFreePages.Text = $"{mpk.FreePages(mPKNotes)} pages free";

            // Populate the listbox with current data
            foreach (MPKNote note in mPKNotes)
            {
                string NoteEntry = $"{note.GameCode}-{note.PubCode} - {note.NoteTitle}.{note.NoteExtension} - Page {note.StartPage}, {note.PageSize} {(note.PageSize == 1 ? "page." : "pages.")}";
                lstNotes.Items.Add(NoteEntry);
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

            if (saveFileDialog.ShowDialog() == DialogResult.OK) mpk.SaveNote(currentNote, saveFileDialog.FileName);

            if(mpk.ErrorCode != 0)
            {
                MessageBox.Show("Error: " + mpk.ErrorStr, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                mpk.ErrorCode = 0;
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
                if(!mpk.ImportNote(note, mPKNotes)) MessageBox.Show("Failed to import save.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            if (mpk.ErrorCode != 0)
            {
                MessageBox.Show("Error: " + mpk.ErrorStr, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                mpk.ErrorCode = 0;
            }
            RefreshNoteList();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                mpk.DeleteNote(lstNotes.SelectedIndex, mPKNotes);
            } 
            catch(ArgumentOutOfRangeException ex)
            {
                MessageBox.Show($"Please select a note first.\nSelected Index = {lstNotes.SelectedIndex}\nMessage: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            RefreshNoteList();
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
                if(fileLoaded) {
                    mpk.SaveMPK(saveFileDialog.FileName, mpk, mPKNotes);
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
                mpk.ErrorCode = 0;
            }

        }

        private void btnFormat_Click(object sender, EventArgs e)
        {
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
                    mpk = new Mempak(int.Parse(cbComPort.Text), mPKNotes, pbCardProgress);
                }
                catch (FormatException)
                {
                    mpk.ErrorStr = "COM Port should be a number.";
                    mpk.ErrorCode = 1;
                    return;
                } catch (ArgumentNullException)
                {
                    mpk.ErrorStr = "COM Port should contain a value.";
                    mpk.ErrorCode = 5;
                    return;
                }

                RefreshNoteList();

                // If there was an error, show it.
                if (mpk.ErrorCode != 0)
                {
                    MessageBox.Show("Error: " + mpk.ErrorStr, "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    mpk.ErrorCode = 0;
                }
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
            if (!fileLoaded)
            {
                MessageBox.Show("Load a file or open a card first.");
                return;
            }
            DialogResult yn = MessageBox.Show(
            "Write Card?\nWARNING: ALL DATA CURRENTLY ON\nCARD WILL BE OVERWRITTEN.",
            "Write Card?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (yn == DialogResult.No)
            {
                MessageBox.Show("Write cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (yn == DialogResult.Yes)
            {
                pbCardProgress.Minimum = 0;
                pbCardProgress.Maximum = 128;
                pbCardProgress.Value = 0;
                pbCardProgress.Step = 1;
                DexDrive drive = new DexDrive();
                if (drive.StartDexDrive($"COM{cbComPort.Text}"))
                {
                    // Write the label and ID block.
                    drive.WriteMemoryCardFrame(0, Mempak.BuildHeader());
                    pbCardProgress.PerformStep();

                    // Write the Index Table.
                    byte[] indexBytes = mpk.IndexTable.ToArray().SelectMany(shortValue => BitConverter.GetBytes(shortValue).Reverse()).ToArray();
                    drive.WriteMemoryCardFrame(1, indexBytes);
                    pbCardProgress.PerformStep();
                    drive.WriteMemoryCardFrame(2, indexBytes);
                    pbCardProgress.PerformStep();

                    // Write the note table
                    MemoryStream noteTable = new MemoryStream();
                    BinaryWriter ntWriter = new BinaryWriter(noteTable);

                    foreach (MPKNote note in mPKNotes)
                    {
                        // Game Code
                        ntWriter.Write(note.GameCodeRaw, 0, note.GameCodeRaw.Length);
                        // Publisher Code
                        ntWriter.Write(note.PubCodeRaw, 0, note.PubCodeRaw.Length);
                        // Start Page
                        ntWriter.Write(new byte[] { (byte)((note.StartPage >> 8) & 0xFF), (byte)(note.StartPage & 0xFF) }, 0, 2);
                        // Status
                        ntWriter.Write(new byte[] { note.Status }, 0, 1);
                        // Reserved/Data Sum (Three zeroes)
                        ntWriter.Write(new byte[] { 0x00, 0x00, 0x00 }, 0, 3);
                        // File Extension
                        ntWriter.Write(note.NoteExtRaw, 0, 4);
                        // File Name
                        ntWriter.Write(note.NoteTitleRaw, 0, 16);
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
                    drive.WriteMemoryCardFrame(3, ntReader.ReadBytes(mpk.PageSize));
                    pbCardProgress.PerformStep();
                    drive.WriteMemoryCardFrame(4, ntReader.ReadBytes(mpk.PageSize));
                    pbCardProgress.PerformStep();

                    short pagesWritten = 0;
                    ushort frameToWrite = 5;
                    foreach (MPKNote note in mPKNotes)
                    {
                        MemoryStream noteStream = new MemoryStream(note.Data);
                        BinaryReader noteReader = new BinaryReader(noteStream);
                        for (int i = 0; i < note.PageSize; i++) // Change to note.PageSize
                        {
                            try
                            {
                                if (!drive.WriteMemoryCardFrame(frameToWrite, noteReader.ReadBytes(mpk.PageSize)))
                                {
                                    MessageBox.Show("Writing frame failed.", "Write Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                                else
                                {
                                    pbCardProgress.PerformStep();
                                    pagesWritten++;
                                    frameToWrite++;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error!\nMessage: {ex.Message}\nType: {ex.GetType()}\nSource: {ex.Source}");
                            }
                        }
                    }

                    // Zero-fill the rest of the card.
                    while (pagesWritten < 123)
                    {
                        if (!drive.WriteMemoryCardFrame((ushort)(pagesWritten + 5), DexDrive.BlankPage()))
                        {
                            MessageBox.Show("Writing frame failed.", "Write Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        pagesWritten++;
                        frameToWrite++;
                        pbCardProgress.PerformStep();
                    }

                    drive.StopDexDrive();
                    MessageBox.Show("Card Written.", "Write Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Writing card failed.", "Write Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void lstNotes_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void lstNotes_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy; // Allow the drop
            }
            else
            {
                e.Effect = DragDropEffects.None; // Don't allow the drop
            }
        }

        private void lstNotes_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string filePath in files)
                {
                    string fileExtension = Path.GetExtension(filePath);
                    
                    // Check the file type based on its extension
                    if (fileExtension.Equals(".note", StringComparison.OrdinalIgnoreCase))
                    {
                        if (fileLoaded)
                        {
                            MPKNote note = new MPKNote(filePath);
                            if (!mpk.ImportNote(note, mPKNotes)) MessageBox.Show("Failed to import save.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            else RefreshNoteList();
                        } else
                        {
                            MessageBox.Show("Load a file or read a card first.", "No Card", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    else if (fileExtension.Equals(".mpk", StringComparison.OrdinalIgnoreCase) ||
                             fileExtension.Equals(".n64", StringComparison.OrdinalIgnoreCase))
                    {
                        // Clear the lists
                        lstNotes.Items.Clear();
                        mPKNotes.Clear();

                        mpk = new Mempak(filePath, mPKNotes) { Type = Mempak.CardType.CARD_VIRTUAL };
                        RefreshNoteList();

                        fileLoaded = true;

                        if (mpk.ErrorCode != 0)
                        {
                            MessageBox.Show("Error: " + mpk.ErrorStr, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            mpk.ErrorCode = 0;
                        }
                    }
                    // Add more cases for other file types if needed
                }
            }

        }
    }
}
