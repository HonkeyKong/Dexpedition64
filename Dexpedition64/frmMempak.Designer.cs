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
            this.btnNew = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblMPKStatus = new System.Windows.Forms.Label();
            this.lblCkSum1 = new System.Windows.Forms.Label();
            this.lblCkSum2 = new System.Windows.Forms.Label();
            this.lblLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lstNotes
            // 
            this.lstNotes.FormattingEnabled = true;
            this.lstNotes.Location = new System.Drawing.Point(12, 70);
            this.lstNotes.Name = "lstNotes";
            this.lstNotes.Size = new System.Drawing.Size(424, 225);
            this.lstNotes.TabIndex = 0;
            // 
            // btnNew
            // 
            this.btnNew.Location = new System.Drawing.Point(12, 305);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(91, 39);
            this.btnNew.TabIndex = 1;
            this.btnNew.Text = "New MPK";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(445, 131);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(91, 37);
            this.btnImport.TabIndex = 2;
            this.btnImport.Text = "Import note";
            this.btnImport.UseVisualStyleBackColor = true;
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(445, 174);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(91, 37);
            this.btnExport.TabIndex = 2;
            this.btnExport.Text = "Export Note";
            this.btnExport.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(445, 217);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(91, 37);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "Delete Note";
            this.btnDelete.UseVisualStyleBackColor = true;
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(138, 305);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(91, 39);
            this.btnLoad.TabIndex = 1;
            this.btnLoad.Text = "Load MPK";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(264, 305);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(91, 39);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Save MPK";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // lblMPKStatus
            // 
            this.lblMPKStatus.AutoSize = true;
            this.lblMPKStatus.Location = new System.Drawing.Point(12, 37);
            this.lblMPKStatus.Name = "lblMPKStatus";
            this.lblMPKStatus.Size = new System.Drawing.Size(0, 13);
            this.lblMPKStatus.TabIndex = 3;
            // 
            // lblCkSum1
            // 
            this.lblCkSum1.AutoSize = true;
            this.lblCkSum1.Location = new System.Drawing.Point(442, 80);
            this.lblCkSum1.Name = "lblCkSum1";
            this.lblCkSum1.Size = new System.Drawing.Size(0, 13);
            this.lblCkSum1.TabIndex = 4;
            // 
            // lblCkSum2
            // 
            this.lblCkSum2.AutoSize = true;
            this.lblCkSum2.Location = new System.Drawing.Point(442, 110);
            this.lblCkSum2.Name = "lblCkSum2";
            this.lblCkSum2.Size = new System.Drawing.Size(0, 13);
            this.lblCkSum2.TabIndex = 4;
            // 
            // lblLabel
            // 
            this.lblLabel.AutoSize = true;
            this.lblLabel.Location = new System.Drawing.Point(12, 9);
            this.lblLabel.Name = "lblLabel";
            this.lblLabel.Size = new System.Drawing.Size(125, 13);
            this.lblLabel.TabIndex = 5;
            this.lblLabel.Text = "No Memory Pak Loaded.";
            // 
            // frmMempak
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(548, 355);
            this.Controls.Add(this.lblLabel);
            this.Controls.Add(this.lblCkSum2);
            this.Controls.Add(this.lblCkSum1);
            this.Controls.Add(this.lblMPKStatus);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnNew);
            this.Controls.Add(this.lstNotes);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "frmMempak";
            this.Text = "MPK Manager - Dexpedition64";
            this.Load += new System.EventHandler(this.frmMempak_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstNotes;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblMPKStatus;
        private System.Windows.Forms.Label lblCkSum1;
        private System.Windows.Forms.Label lblCkSum2;
        private System.Windows.Forms.Label lblLabel;
    }
}