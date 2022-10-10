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

        private void btnLoad_Click(object sender, EventArgs e)
        {
            /* Note: Eventually all this crap will be broken out
             * into individual functions, in order to make it
             * possible to read the tables from a memory card 
             * without dumping it to disk first.                */

            OpenFileDialog mpkFile = new OpenFileDialog();
            mpkFile.Filter = "Memory Pak files (*.mpk)|*.mpk|All files (*.*)|*.*";
            mpkFile.FilterIndex = 0;
            mpkFile.ShowDialog();
            if (mpkFile.FileName != "")
            {
                using (FileStream fs = File.OpenRead(mpkFile.FileName))
                {
                    BinaryReader br = new BinaryReader(fs);
                    Mempak mpk = new Mempak();
                    
                    // Read the header
                    byte[] cardLabel = br.ReadBytes(32);

                    // Read the serial number
                    byte[] serial = br.ReadBytes(28);
                    
                    // Read the Checksums
                    short checkSum1 = br.ReadInt16();
                    short checkSum2 = br.ReadInt16();

                    /* 32 bytes of null, then two more ID blocks.
                     * 32 more nulls, one more ID block, 32 null,
                     * for a grand total of 192 redundant bytes. 
                     * If we wanted to, we could parse the other
                     * ID blocks and sanity check, but fuck it.  */
                    br.ReadBytes(192); // Just chuck 'em

                    // Print our label.
                    lblLabel.Text = "Label: " + Encoding.Default.GetString(cardLabel);

                    // Convert our serial number to a hex string.
                    lblMPKStatus.Text = "Serial Number: " + string.Join(":", serial.Select(hex => string.Format("{0:X2}", hex)));

                    // Print our Checksums
                    lblCkSum1.Text = "Checksum 1: " + checkSum1.ToString("X2");
                    lblCkSum2.Text = "Checksum 2: " + checkSum2.ToString("X2");

                    // Read the Index Table
                    byte[] indexTable = br.ReadBytes(512);

                    // Read the Note Table
                    List<MPKNote> mPKNotes = new List<MPKNote>();
                    for(int i = 0; i < 16; i++)
                    {
                        mPKNotes.Add(new MPKNote(br.ReadBytes(32)));
                    }

                    // Clear the listbox
                    lstNotes.Items.Clear();

                    // Populate the listbox with current data
                    foreach(MPKNote note in mPKNotes)
                    {
                        string NoteEntry = "";
                        NoteEntry += note.GameCode + " - " + note.PubCode;
                        NoteEntry += " - " + note.NoteTitle + "." + note.NoteExtension;
                        NoteEntry += " - Page " + note.startPage.ToString();
                        
                        // Seek to page in index table.
                        int numPages = 0;
                        int startPage = note.startPage;
                        while (indexTable[startPage] != 0x01)
                        {
                            startPage++;
                            if(indexTable[startPage] != 0x00) numPages++;
                        }
                        
                        NoteEntry += ", " + numPages + (numPages == 1? " page." : " pages.");

                        lstNotes.Items.Add(NoteEntry);
                    }
                }
            }
        }

        private void frmMempak_Load(object sender, EventArgs e)
        {

        }
    }
}
