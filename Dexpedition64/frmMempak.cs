using System;
using System.IO;
using System.Collections.Generic;
/*using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;*/
using System.Windows.Forms;

namespace Dexpedition64
{
    public partial class frmMempak : Form
    {
        public frmMempak()
        {
            InitializeComponent();
        }

        private bool fileLoaded = false;
        Mempak mpk;
        List<MPKNote> mPKNotes = new List<MPKNote>();

        private void btnLoad_Click(object sender, EventArgs e)
        {
            /* Note: Eventually all this crap will be broken out
             * into individual functions, in order to make it
             * possible to read the tables from a memory card 
             * without dumping it to disk first.                */
            OpenFileDialog mpkFile = new OpenFileDialog();
            mpkFile.Filter = "Memory Pak files (*.mpk)|*.mpk|All files (*.*)|*.*";
            mpkFile.FilterIndex = 0;

            if(mpkFile.ShowDialog() == DialogResult.OK)
            {
                lstNotes.Items.Clear();
                mPKNotes.Clear();
                mpk = new Mempak(mpkFile.FileName, mPKNotes);

                lblLabel.Text = mpk.Label;
                lblSerial.Text = mpk.SerialNumber;

                // Clear the listbox
                lstNotes.Items.Clear();

                // Populate the listbox with current data
                foreach (MPKNote note in mPKNotes)
                {
                    string NoteEntry = "";
                    NoteEntry += note.GameCode + " - " + note.PubCode;
                    NoteEntry += " - " + note.NoteTitle + "." + note.NoteExtension;
                    NoteEntry += " - Page " + note.StartPage.ToString();
                    NoteEntry += ", " + note.PageSize + (note.PageSize == 1 ? " page." : " pages.");

                    lstNotes.Items.Add(NoteEntry);
                }
                fileLoaded = true;
                mpk.Type = Mempak.CardType.CARD_VIRTUAL;
                if(mpk.ErrorCode != 0)
                {
                    MessageBox.Show("Error: " + mpk.ErrorStr, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "N64 Save Notes (*.note)|*.note|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 0;
            saveFileDialog.FileName = currentNote.NoteTitle + ".note";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (mpk.Type == Mempak.CardType.CARD_PHYSICAL) currentNote.Data = mpk.ReadNoteFromCard(currentNote);
                mpk.SaveNote(currentNote, saveFileDialog.FileName);
            }
            if(mpk.ErrorCode != 0)
            {
                MessageBox.Show("Error: " + mpk.ErrorStr, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not implemented yet.");
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Delete the selected note entry from the note table
            // Mark each used page as free (0x03) on the index table

            MessageBox.Show("Not implemented yet.");
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not implemented yet.");
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not implemented yet.");
        }

        private void btnReadCard_Click(object sender, EventArgs e)
        {
            try
            {
                mPKNotes.Clear();
                mpk = new Mempak(1, mPKNotes);
                lblLabel.Text = mpk.Label;
                lstNotes.Items.Clear();
                foreach (MPKNote note in mPKNotes)
                {
                    string NoteEntry = "";
                    NoteEntry += note.GameCode + " - " + note.PubCode;
                    NoteEntry += " - " + note.NoteTitle + "." + note.NoteExtension;
                    NoteEntry += " - Page " + note.StartPage.ToString();
                    NoteEntry += ", " + note.PageSize + (note.PageSize == 1 ? " page." : " pages.");

                    lstNotes.Items.Add(NoteEntry);
                }
                if(mpk.ErrorStr != "") MessageBox.Show(mpk.ErrorStr);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message +
                    "\nAre you sure your DexDrive is plugged in?" +
                    "\nTry disconnecting and reconnecting the power.",
                    "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            fileLoaded = true;
            mpk.Type = Mempak.CardType.CARD_PHYSICAL;
        }
    }
}
