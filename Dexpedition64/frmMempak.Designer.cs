namespace Dexpedition64
{
    partial class frmMempak
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lstNotes = new System.Windows.Forms.ListBox();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.lblSerial = new System.Windows.Forms.Label();
            this.lblCkSum1 = new System.Windows.Forms.Label();
            this.lblLabel = new System.Windows.Forms.Label();
            this.btnNew = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnReadCard = new System.Windows.Forms.Button();
            this.btnWriteCard = new System.Windows.Forms.Button();
            this.cbComPort = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnFormat = new System.Windows.Forms.Button();
            this.pbCardProgress = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // lstNotes
            // 
            this.lstNotes.FormattingEnabled = true;
            this.lstNotes.Location = new System.Drawing.Point(12, 56);
            this.lstNotes.Name = "lstNotes";
            this.lstNotes.Size = new System.Drawing.Size(399, 225);
            this.lstNotes.TabIndex = 0;
            this.lstNotes.SelectedIndexChanged += new System.EventHandler(this.lstNotes_SelectedIndexChanged);
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(417, 173);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(91, 37);
            this.btnImport.TabIndex = 2;
            this.btnImport.Text = "Import note";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(417, 216);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(91, 37);
            this.btnExport.TabIndex = 2;
            this.btnExport.Text = "Export Note";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(417, 259);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(91, 37);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "Delete Note";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(417, 130);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(91, 37);
            this.btnLoad.TabIndex = 1;
            this.btnLoad.Text = "Load MPK";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // lblSerial
            // 
            this.lblSerial.AutoSize = true;
            this.lblSerial.Location = new System.Drawing.Point(9, 39);
            this.lblSerial.Name = "lblSerial";
            this.lblSerial.Size = new System.Drawing.Size(0, 13);
            this.lblSerial.TabIndex = 3;
            // 
            // lblCkSum1
            // 
            this.lblCkSum1.AutoSize = true;
            this.lblCkSum1.Location = new System.Drawing.Point(414, 11);
            this.lblCkSum1.Name = "lblCkSum1";
            this.lblCkSum1.Size = new System.Drawing.Size(0, 13);
            this.lblCkSum1.TabIndex = 4;
            // 
            // lblLabel
            // 
            this.lblLabel.AutoSize = true;
            this.lblLabel.Location = new System.Drawing.Point(9, 11);
            this.lblLabel.Name = "lblLabel";
            this.lblLabel.Size = new System.Drawing.Size(125, 13);
            this.lblLabel.TabIndex = 5;
            this.lblLabel.Text = "No Memory Pak Loaded.";
            // 
            // btnNew
            // 
            this.btnNew.Location = new System.Drawing.Point(417, 45);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(91, 37);
            this.btnNew.TabIndex = 6;
            this.btnNew.Text = "New MPK";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(417, 88);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(91, 36);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "Save MPK";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnReadCard
            // 
            this.btnReadCard.Location = new System.Drawing.Point(12, 293);
            this.btnReadCard.Name = "btnReadCard";
            this.btnReadCard.Size = new System.Drawing.Size(75, 39);
            this.btnReadCard.TabIndex = 8;
            this.btnReadCard.Text = "Browse Card";
            this.btnReadCard.UseVisualStyleBackColor = true;
            this.btnReadCard.Click += new System.EventHandler(this.btnReadCard_Click);
            // 
            // btnWriteCard
            // 
            this.btnWriteCard.Location = new System.Drawing.Point(93, 293);
            this.btnWriteCard.Name = "btnWriteCard";
            this.btnWriteCard.Size = new System.Drawing.Size(75, 39);
            this.btnWriteCard.TabIndex = 9;
            this.btnWriteCard.Text = "Write Card";
            this.btnWriteCard.UseVisualStyleBackColor = true;
            this.btnWriteCard.Click += new System.EventHandler(this.btnWriteCard_Click);
            // 
            // cbComPort
            // 
            this.cbComPort.FormattingEnabled = true;
            this.cbComPort.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12"});
            this.cbComPort.Location = new System.Drawing.Point(473, 303);
            this.cbComPort.Name = "cbComPort";
            this.cbComPort.Size = new System.Drawing.Size(41, 21);
            this.cbComPort.TabIndex = 11;
            this.cbComPort.Text = "1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(414, 306);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "COM Port";
            // 
            // btnFormat
            // 
            this.btnFormat.Location = new System.Drawing.Point(174, 293);
            this.btnFormat.Name = "btnFormat";
            this.btnFormat.Size = new System.Drawing.Size(75, 39);
            this.btnFormat.TabIndex = 2;
            this.btnFormat.Text = "Format Card";
            this.btnFormat.UseVisualStyleBackColor = true;
            this.btnFormat.Click += new System.EventHandler(this.btnFormat_Click);
            // 
            // pbCardProgress
            // 
            this.pbCardProgress.Location = new System.Drawing.Point(255, 293);
            this.pbCardProgress.Name = "pbCardProgress";
            this.pbCardProgress.Size = new System.Drawing.Size(155, 39);
            this.pbCardProgress.TabIndex = 13;
            // 
            // frmMempak
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(522, 340);
            this.Controls.Add(this.pbCardProgress);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbComPort);
            this.Controls.Add(this.btnWriteCard);
            this.Controls.Add(this.btnReadCard);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnNew);
            this.Controls.Add(this.lblLabel);
            this.Controls.Add(this.lblCkSum1);
            this.Controls.Add(this.lblSerial);
            this.Controls.Add(this.btnFormat);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.lstNotes);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "frmMempak";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "MPK Manager - Dexpedition64";
            this.Load += new System.EventHandler(this.frmMempak_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstNotes;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Label lblSerial;
        private System.Windows.Forms.Label lblCkSum1;
        private System.Windows.Forms.Label lblLabel;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnReadCard;
        private System.Windows.Forms.Button btnWriteCard;
        private System.Windows.Forms.ComboBox cbComPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnFormat;
        private System.Windows.Forms.ProgressBar pbCardProgress;
    }
}