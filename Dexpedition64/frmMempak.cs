using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Dexpedition64
{
    public partial class frmMempak : Form
    {
        public frmMempak()
        {
            InitializeComponent();
        }

        private void PopulateManager()
        {
            lblLabel.Text = "Label: " + mpk.Label;
            lblSerial.Text = "Serial: " + mpk.SerialNumber;
            lblCkSum1.Text = "Checksum 1: " + mpk.CheckSum1;
            lblCkSum2.Text = "Checksum 2: " + mpk.CheckSum2;
            lblRealCksum.Text = "Calculated: " + mpk.RealCheckSum;
        }

        private bool fileLoaded = false;
        Mempak mpk;
        List<MPKNote> mPKNotes = new List<MPKNote>();

        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog mpkFile = new OpenFileDialog
            {
                Filter = "Memory Pak files (*.mpk)|*.mpk|All files (*.*)|*.*",
                FilterIndex = 0
            };

            if(mpkFile.ShowDialog() == DialogResult.OK)
            {
                
                // Clear the lists
                lstNotes.Items.Clear();
                mPKNotes.Clear();

                mpk = new Mempak(mpkFile.FileName, mPKNotes);

                PopulateManager();

                /*
                // Populate the listbox with current data
                foreach (MPKNote note in mPKNotes)
                {
                    string NoteEntry = "";
                    NoteEntry += note.GameCode + " - " + note.PubCode;
                    NoteEntry += " - " + note.NoteTitle + "." + note.NoteExtension;
                    NoteEntry += " - Page " + note.StartPage.ToString();
                    NoteEntry += ", " + note.PageSize + (note.PageSize == 1 ? " page." : " pages.");

                    lstNotes.Items.Add(NoteEntry);
                }*/

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
                /*string NoteEntry = "";
                NoteEntry += note.GameCode + " - " + note.PubCode;
                NoteEntry += " - " + note.NoteTitle + "." + note.NoteExtension;
                NoteEntry += " - Page " + note.StartPage.ToString();
                NoteEntry += ", " + note.PageSize + (note.PageSize == 1 ? " page." : " pages.");*/

                string NoteEntry = $"{note.GameCode}-{note.PubCode} - {note.NoteTitle}.{note.NoteExtension} - Page {note.StartPage.ToString()}, {note.PageSize} {(note.PageSize == 1 ? "page." : "pages.")}";

                lstNotes.Items.Add(NoteEntry);
            }

        }

        private void frmMempak_Load(object sender, EventArgs e)
        {

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
                    if (mpk.Type == Mempak.CardType.CARD_PHYSICAL) currentNote.Data = mpk.ReadNoteFromCard(currentNote, int.Parse(cbComPort.Text)).ToArray();
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

            PopulateManager();
            RefreshNoteList();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Delete the selected note entry from the note table
            // Mark each used page as free (0x03) on the index table

            try
            {
                if (mpk.Type == Mempak.CardType.CARD_VIRTUAL)
                {
                    lstNotes.Items.Remove(lstNotes.Items[lstNotes.SelectedIndex]);
                    mpk.DeleteNote(lstNotes.SelectedIndex, mPKNotes, mpk.IndexTable);
                }
                else if(mpk.Type == Mempak.CardType.CARD_PHYSICAL)
                {
                    MessageBox.Show("Deleting from physical cards is not supported yet.");
                }
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
                        mpk.SaveMPK(int.Parse(cbComPort.Text), saveFileDialog.FileName);
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
                }
            }
            if(mpk.ErrorCode != 0)
            {
                MessageBox.Show("Error: " + mpk.ErrorStr, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

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

                PopulateManager();

                /*
                lstNotes.Items.Clear();

                // Populate the listbox
                foreach (MPKNote note in mPKNotes)
                {
                    string NoteEntry = "";
                    NoteEntry += note.GameCode + " - " + note.PubCode;
                    NoteEntry += " - " + note.NoteTitle + "." + note.NoteExtension;
                    NoteEntry += " - Page " + note.StartPage.ToString();
                    NoteEntry += ", " + note.PageSize + (note.PageSize == 1 ? " page." : " pages.");

                    lstNotes.Items.Add(NoteEntry);
                }*/

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
            MainForm mfrm = new MainForm();
            mfrm.Show();
        }

        private void lstNotes_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
