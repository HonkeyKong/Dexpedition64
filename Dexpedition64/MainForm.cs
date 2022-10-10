using System;
/*using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;*/
using System.IO;
/*using System.Linq;
using System.Text;
using System.Threading.Tasks;*/
using System.Windows.Forms;

namespace Dexpedition64
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void ShowDexError()
        {
            MessageBox.Show("Error: Failed to init DexDrive." +
                            "\nAre you sure your DexDrive is plugged in?" +
                            "\nTry disconnecting and reconnecting the power.",
                            "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "Memory Pak files (*.mpk)|*.mpk|All files (*.*)|*.*";
            saveFile.FilterIndex = 0;

            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                using (FileStream fs = File.OpenWrite(saveFile.FileName))
                {
                    lblStatus.Text = "Reading Card...";
                    pbCardProgress.Value = 0;
                    pbCardProgress.Step = 1;
                    DexDrive drive = new DexDrive();
                    if (drive.StartDexDrive(cbComPort.Text))
                    {
                        try
                        {
                            ushort i;
                            for (i = 0; i < 128; i++)
                            {
                                // Read a frame from the memory card.
                                byte[] cardData = drive.ReadMemoryCardFrame(i);
                                fs.Write(cardData, 0, cardData.Length);
                                pbCardProgress.PerformStep();
                            }
                            drive.StopDexDrive();
                            lblStatus.Text = "Card Read.";
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show("Error: " + ex.Message +
                                "\nAre you sure your DexDrive is plugged in?" +
                                "\nTry disconnecting and reconnecting the power.",
                                "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            lblStatus.Text = "Read Failed.";
                        }
                    }
                    else
                    {
                        ShowDexError();
                        lblStatus.Text = "Read Failed.";
                    }
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void btnFormat_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.DialogResult yn = MessageBox.Show("Format Card?\nWARNING: ALL DATA WILL BE LOST.", "Format Card?", MessageBoxButtons.YesNo);
            if (yn == System.Windows.Forms.DialogResult.No)
            {
                lblStatus.Text = "Format cancelled.";
                return;
            }
            else if (yn == System.Windows.Forms.DialogResult.Yes)
            {
                pbCardProgress.Value = 0;
                pbCardProgress.Step = 25;
                DexDrive drive = new DexDrive();

                if (drive.StartDexDrive(cbComPort.Text))
                {
                    try
                    {
                        ushort i;

                        lblStatus.Text = "Formatting Card...";
                        string strFailed = "Writing frame failed.";

                        if (!drive.WriteMemoryCardFrame(0, Mempak.BuildHeader()))
                        {
                            lblStatus.Text = strFailed;
                            return;
                        }
                        pbCardProgress.PerformStep();

                        for (i = 1; i < 3; i++)
                        {
                            if (!drive.WriteMemoryCardFrame(i, Mempak.FormatPage()))
                            {
                                lblStatus.Text = strFailed;
                                return;
                            }
                            pbCardProgress.PerformStep();
                        }

                        for (i = 3; i < 5; i++)
                        {
                            if (!drive.WriteMemoryCardFrame(i, DexDrive.BlankPage()))
                            {
                                lblStatus.Text = strFailed;
                                return;
                            }
                            pbCardProgress.PerformStep();
                        }

                        drive.StopDexDrive();
                        pbCardProgress.Value = 127;
                        lblStatus.Text = "Card formatted.";
                    }
                    catch (Exception ex)
                    {
                        lblStatus.Text = "Format Failed.";

                        MessageBox.Show("Error: " + ex.Message +
                            "\nAre you sure your DexDrive is plugged in?" +
                            "\nTry disconnecting and reconnecting the power.",
                            "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    ShowDexError();
                    lblStatus.Text = "Format Failed.";
                }
            }
        }

        private void btnWrite_Click(object sender, EventArgs e)
        {
            OpenFileDialog writeFile = new OpenFileDialog();
            writeFile.Filter = "Memory Pak files (*.mpk)|*.mpk|All files (*.*)|*.*";
            writeFile.FilterIndex = 0;
            
            if (writeFile.ShowDialog() == DialogResult.OK)
            {
                using (FileStream fs = File.OpenRead(writeFile.FileName))
                {
                    pbCardProgress.Value = 0;
                    pbCardProgress.Step = 1;
                    DexDrive drive = new DexDrive();
                    if (drive.StartDexDrive(cbComPort.Text))
                    {
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
                                pbCardProgress.PerformStep();
                            }

                            cardBuf.Initialize();
                        }
                        drive.StopDexDrive();
                        lblStatus.Text = "Card Written.";
                    }
                    else
                    {
                        ShowDexError();
                        lblStatus.Text = "Write failed.";
                    }
                }
            }
        }

        private void btnManager_Click(object sender, EventArgs e)
        {
            Form fm = new frmMempak();
            fm.Show();
        }
    }
}
