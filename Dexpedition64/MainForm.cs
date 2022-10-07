using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dexpedition64
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void InitDexDrive(DexDrive drive, string comPort)
        {
            string ret = drive.StartDexDrive(comPort);
            if (ret != null)
            {
                Console.WriteLine(ret + " DexDrive detected.");
                statusStrip1.Text = (ret + " DexDrive detected.");
            }
            if (ret == "PSX")
            {
                statusStrip1.Text = "PSX DexDrive unsupported. Try MemCardRex: https://github.com/ShendoXT/memcardrex";
                return;
            }
            else
            {
                statusStrip1.Text =  "Error: DexDrive not detected.";
                return;
            }
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "Memory Pak files (*.mpk)|*.mpk|All files (*.*)|*.*";
            saveFile.FilterIndex = 0;
            saveFile.ShowDialog();
            if (saveFile.FileName != "")
            {
                using (FileStream fs = File.OpenWrite(saveFile.FileName))
                {
                    lblStatus.Text = "Reading Card...";
                    toolStripProgressBar1.Value = 0;
                    toolStripProgressBar1.Step = 1;
                    DexDrive drive = new DexDrive();
                    InitDexDrive(drive, cbComPort.Text);
                    ushort i;
                    for (i = 0; i < 128; i++)
                    {
                        // Read a frame from the memory card.
                        byte[] cardData = drive.ReadMemoryCardFrame(i);
                        fs.Write(cardData, 0, cardData.Length);
                        toolStripProgressBar1.PerformStep();
                    }
                    drive.StopDexDrive();
                    lblStatus.Text = "Card Read.";
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void btnFormat_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.DialogResult yn = MessageBox.Show("Format Card? WARNING: ALL DATA WILL BE LOST.", "Format Card", MessageBoxButtons.YesNo);
            if (yn == System.Windows.Forms.DialogResult.No)
            {
                lblStatus.Text = "Format cancelled.";
                return;
            }
            else if (yn == System.Windows.Forms.DialogResult.Yes)
            {
                toolStripProgressBar1.Value = 0;
                toolStripProgressBar1.Step = 25;
                DexDrive drive = new DexDrive();
                InitDexDrive(drive, cbComPort.Text);
                ushort i;

                lblStatus.Text = "Formatting Card...";

                string strFailed = "Writing frame failed.";

                for (i = 0; i < 3; i++)
                {
                    if (!drive.WriteMemoryCardFrame(i, CardData.formatPage[i]))
                    {
                        lblStatus.Text = strFailed;
                        return;
                    }
                    toolStripProgressBar1.PerformStep();
                }

                for (i = 3; i < 5; i++)
                {
                    if (!drive.WriteMemoryCardFrame(i, DexDrive.BlankPage()))
                    {
                        lblStatus.Text = strFailed;
                        return;
                    }
                    toolStripProgressBar1.PerformStep();
                }

                drive.StopDexDrive();
                toolStripProgressBar1.Value = 127;
                lblStatus.Text = "Card formatted.";
            }
        }

        private void btnWrite_Click(object sender, EventArgs e)
        {
            OpenFileDialog writeFile = new OpenFileDialog();
            writeFile.Filter = "Memory Pak files (*.mpk)|*.mpk|All files (*.*)|*.*";
            writeFile.FilterIndex = 0;
            writeFile.ShowDialog();
            if (writeFile.FileName != "")
            {
                using (FileStream fs = File.OpenRead(writeFile.FileName))
                {
                    toolStripProgressBar1.Value = 0;
                    toolStripProgressBar1.Step = 1;
                    DexDrive drive = new DexDrive();
                    InitDexDrive(drive, cbComPort.Text);
                    BinaryReader br = new BinaryReader(fs);
                    byte[] cardBuf = new byte[256];
                    ushort i;
                    lblStatus.Text = "Writing Card...";
                    for (i = 0; i < 128; i++)
                    {
                        // Read a frame from the file.
                        cardBuf = br.ReadBytes(256);

                        if (!drive.WriteMemoryCardFrame(i, cardBuf))
                        {
                            lblStatus.Text = "Writing frame failed.";
                            return;
                        }
                        else
                        {
                            toolStripProgressBar1.PerformStep();
                        }

                        cardBuf.Initialize();
                    }
                    drive.StopDexDrive();
                    lblStatus.Text = "Card Written.";
                }
            }
        }
    }
}
