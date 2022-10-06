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
            using (FileStream fs = File.OpenWrite(@"output.mpk"))
            {
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
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void btnFormat_Click(object sender, EventArgs e)
        {
            using (FileStream fs = File.OpenRead(@"C:\Users\honke\Downloads\New.mpk"))
            {
                DexDrive drive = new DexDrive();
                InitDexDrive(drive, cbComPort.Text);
                BinaryReader br = new BinaryReader(fs);
                byte[] cardBuf = new byte[256];
                ushort i;
                lblStatus.Text = "Formatting Card...";
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
                        //Console.Write(".");
                        toolStripProgressBar1.PerformStep();
                    }

                    cardBuf.Initialize();
                }
                drive.StopDexDrive();
                lblStatus.Text = "Card formatted.";
            }
        }

        private void btnWrite_Click(object sender, EventArgs e)
        {
            using (FileStream fs = File.OpenRead(@"C:\Users\honke\Downloads\test.mpk"))
            {
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
                        //Console.Write(".");
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
