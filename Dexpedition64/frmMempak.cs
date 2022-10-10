using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                using (FileStream fs = File.OpenRead(mpkFile.FileName))
                {
                    BinaryReader bf = new BinaryReader(fs);
                    byte[] data = bf.ReadBytes((int)fs.Length);
                    
                    fs.Seek(0, SeekOrigin.Begin);
                    fileLoaded = true;
                    BinaryReader br = new BinaryReader(fs);
                    Mempak mpk = new Mempak();
                    
                    // Read the label
                    byte[] cardLabel = br.ReadBytes(32);
                    
                    // Print the label
                    lblLabel.Text = "Label: " + Encoding.Default.GetString(cardLabel);

                    // Read the serial number
                    byte[] serial = br.ReadBytes(28);
                    
                    // Read the Checksums
                    short checkSum1 = br.ReadInt16();
                    short checkSum2 = br.ReadInt16();
                    
                    // Print our Checksums
                    lblCkSum1.Text = "Checksum 1: " + checkSum1.ToString("X2");
                    lblCkSum2.Text = "Checksum 2: " + checkSum2.ToString("X2");

                    // Calculate the actual checksum
                    ushort realCkSum = 0;
                    for(int i = 0; i < serial.Length; i+=2)
                    {
                        ushort ckWord = (ushort)((serial[i] << 8) + (serial[i + 1]));
                        realCkSum += ckWord;
                    }
                    
                    // The result should be big-endian. Swap it.
                    byte[] swapSum = new byte[2];
                    swapSum[1] = (byte)(realCkSum >> 8 & 0xFF);
                    swapSum[0] = (byte)(realCkSum & 0xFF);
                    lblRealCksum.Text = "Real Sum: " + String.Join("", swapSum.Select(hex => String.Format("{0:X2}", hex)));

                    /* 32 bytes of null, then two more ID blocks.
                     * 32 more nulls, one more ID block, 32 null,
                     * for a grand total of 192 redundant bytes. 
                     * If we wanted to, we could parse the other
                     * ID blocks and sanity check, but fuck that. */
                    br.ReadBytes(192); // Just chuck 'em

                    // Convert our serial number to a hex string.
                    lblMPKStatus.Text = "Serial Number: " + string.Join(":", serial.Select(hex => string.Format("{0:X2}", hex)));

                    // Read the Index Table
                    byte[] indexTable = br.ReadBytes(512);

                    // Read the Note Table
                    for(int i = 0; i < 16; i++)
                    {
                        mPKNotes.Add(new MPKNote(br.ReadBytes(32), indexTable));
                    }

                    // Clear the listbox
                    lstNotes.Items.Clear();

                    // Populate the listbox with current data
                    foreach(MPKNote note in mPKNotes)
                    {
                        string NoteEntry = "";
                        NoteEntry += note.GameCode + " - " + note.PubCode;
                        NoteEntry += " - " + note.NoteTitle + "." + note.NoteExtension;
                        NoteEntry += " - Page " + note.StartPage.ToString();
                        NoteEntry += ", " + note.PageSize + (note.PageSize == 1? " page." : " pages.");

                        lstNotes.Items.Add(NoteEntry);

                        note.Data = new byte[256 * note.PageSize];

                        // Copy the first page of the note.
                        int pageCount = 0;
                        /*for (int i = 5; i < note.Pages.Count; i++)
                        {
                            fs.Seek(0, SeekOrigin.Begin);
                            fs.Seek((note.StartPage * 0x100), SeekOrigin.Current);
                            try
                            {
                                if (note.StartPage != 164)
                                {
                                    for (int j = 0; j < 256; j++)
                                    {
                                        note.Data[j] = br.ReadByte();
                                    }
                                }
                            }
                            catch (System.IO.EndOfStreamException) { }
                        }*/

                        // If there's more than one page, copy them all.
                        /*if (note.PageSize > 1)
                        {*/
                            foreach (short page in note.Pages)
                            {
                                if (page != 164)
                                {
                                    try
                                    {
                                        fs.Seek(0, SeekOrigin.Begin);
                                        fs.Seek((page * 0x100), SeekOrigin.Current);
                                        for (int i = 0; i < 256; i++)
                                        {
                                            note.Data[(0x100 * pageCount) + i] = br.ReadByte();
                                        }
                                    } catch (System.IO.EndOfStreamException) { }
                                }
                                pageCount++;
                            }
                        //}
                    }
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
                MessageBox.Show("Load a file first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (lstNotes.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select a note first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "N64 Save Notes (*.note)|*.note|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 0;
            saveFileDialog.FileName = mPKNotes[lstNotes.SelectedIndex].NoteTitle + ".note";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (FileStream fs = File.OpenWrite(saveFileDialog.FileName))
                {
                    // Write Note Version (We use version 1)
                    fs.Write(new byte[] {0x01}, 0, 1);

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
                    fs.Write(mPKNotes[lstNotes.SelectedIndex].gameCodeRaw, 0, mPKNotes[lstNotes.SelectedIndex].gameCodeRaw.Length);
                    // Publisher Code
                    fs.Write(mPKNotes[lstNotes.SelectedIndex].pubCodeRaw, 0, mPKNotes[lstNotes.SelectedIndex].pubCodeRaw.Length);
                    // Start page (0xCAFE since this is an extracted save)
                    fs.Write(new byte[] { 0xCA, 0xFE }, 0, 2);
                    // Status/Reserved/Data Sum (None of this crap matters)
                    fs.Write(new byte[] { 0x02, 0x00, 0x00, 0x00 }, 0, 4);
                    // File Extension
                    fs.Write(mPKNotes[lstNotes.SelectedIndex].noteExtRaw, 0, mPKNotes[lstNotes.SelectedIndex].noteExtRaw.Length);
                    // File Name
                    fs.Write(mPKNotes[lstNotes.SelectedIndex].noteTitleRaw, 0, mPKNotes[lstNotes.SelectedIndex].noteTitleRaw.Length);
                    
                    // Write save data
                    fs.Write(mPKNotes[lstNotes.SelectedIndex].Data, 0, mPKNotes[lstNotes.SelectedIndex].Data.Length);
                }
            } 
        }
    }
}
