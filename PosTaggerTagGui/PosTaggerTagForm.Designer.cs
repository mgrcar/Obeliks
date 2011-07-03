namespace PosTaggerTagGui
{
    partial class PosTaggerTagForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PosTaggerTagForm));
            this.pnlSplit = new System.Windows.Forms.TableLayoutPanel();
            this.grpStatus = new System.Windows.Forms.GroupBox();
            this.pnlStatus = new System.Windows.Forms.Panel();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.grpSettings = new System.Windows.Forms.GroupBox();
            this.pnlSettings = new System.Windows.Forms.TableLayoutPanel();
            this.lblEnterTextFile = new System.Windows.Forms.Label();
            this.btnBrowseTextFile = new System.Windows.Forms.Button();
            this.txtTextFile = new System.Windows.Forms.TextBox();
            this.lblEnterTaggerFile = new System.Windows.Forms.Label();
            this.txtTaggerFile = new System.Windows.Forms.TextBox();
            this.btnBrowseTaggerFile = new System.Windows.Forms.Button();
            this.txtLemmatizerFile = new System.Windows.Forms.TextBox();
            this.btnBrowseLemmatizerFile = new System.Windows.Forms.Button();
            this.lblEnterLemmatizerFile = new System.Windows.Forms.Label();
            this.txtOutputFile = new System.Windows.Forms.TextBox();
            this.btnBrowseOutputFile = new System.Windows.Forms.Button();
            this.lblEnterOutputFile = new System.Windows.Forms.Label();
            this.grpButtons = new System.Windows.Forms.GroupBox();
            this.pnlButtons = new System.Windows.Forms.TableLayoutPanel();
            this.btnTag = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlSplit.SuspendLayout();
            this.grpStatus.SuspendLayout();
            this.pnlStatus.SuspendLayout();
            this.grpSettings.SuspendLayout();
            this.pnlSettings.SuspendLayout();
            this.grpButtons.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlSplit
            // 
            this.pnlSplit.AutoSize = true;
            this.pnlSplit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlSplit.ColumnCount = 1;
            this.pnlSplit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlSplit.Controls.Add(this.grpStatus, 0, 2);
            this.pnlSplit.Controls.Add(this.grpSettings, 0, 0);
            this.pnlSplit.Controls.Add(this.grpButtons, 0, 1);
            this.pnlSplit.Location = new System.Drawing.Point(3, 0);
            this.pnlSplit.Name = "pnlSplit";
            this.pnlSplit.RowCount = 3;
            this.pnlSplit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlSplit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlSplit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlSplit.Size = new System.Drawing.Size(588, 471);
            this.pnlSplit.TabIndex = 0;
            // 
            // grpStatus
            // 
            this.grpStatus.AutoSize = true;
            this.grpStatus.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpStatus.Controls.Add(this.pnlStatus);
            this.grpStatus.Location = new System.Drawing.Point(3, 260);
            this.grpStatus.Name = "grpStatus";
            this.grpStatus.Size = new System.Drawing.Size(582, 208);
            this.grpStatus.TabIndex = 3;
            this.grpStatus.TabStop = false;
            this.grpStatus.Text = "Status";
            // 
            // pnlStatus
            // 
            this.pnlStatus.Controls.Add(this.txtStatus);
            this.pnlStatus.Controls.Add(this.progressBar);
            this.pnlStatus.Location = new System.Drawing.Point(6, 19);
            this.pnlStatus.Name = "pnlStatus";
            this.pnlStatus.Size = new System.Drawing.Size(570, 170);
            this.pnlStatus.TabIndex = 0;
            // 
            // txtStatus
            // 
            this.txtStatus.BackColor = System.Drawing.Color.Black;
            this.txtStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtStatus.Font = new System.Drawing.Font("Courier New", 8.150944F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStatus.ForeColor = System.Drawing.Color.Lime;
            this.txtStatus.Location = new System.Drawing.Point(0, 0);
            this.txtStatus.Multiline = true;
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ReadOnly = true;
            this.txtStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtStatus.Size = new System.Drawing.Size(570, 147);
            this.txtStatus.TabIndex = 2;
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar.Location = new System.Drawing.Point(0, 147);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(570, 23);
            this.progressBar.TabIndex = 3;
            this.progressBar.Visible = false;
            // 
            // grpSettings
            // 
            this.grpSettings.AutoSize = true;
            this.grpSettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpSettings.Controls.Add(this.pnlSettings);
            this.grpSettings.Location = new System.Drawing.Point(3, 3);
            this.grpSettings.Name = "grpSettings";
            this.grpSettings.Size = new System.Drawing.Size(582, 175);
            this.grpSettings.TabIndex = 1;
            this.grpSettings.TabStop = false;
            this.grpSettings.Text = "Nastavitve";
            // 
            // pnlSettings
            // 
            this.pnlSettings.AutoSize = true;
            this.pnlSettings.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlSettings.ColumnCount = 3;
            this.pnlSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.pnlSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 395F));
            this.pnlSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.pnlSettings.Controls.Add(this.lblEnterTextFile, 0, 0);
            this.pnlSettings.Controls.Add(this.btnBrowseTextFile, 2, 0);
            this.pnlSettings.Controls.Add(this.txtTextFile, 1, 0);
            this.pnlSettings.Controls.Add(this.lblEnterTaggerFile, 0, 2);
            this.pnlSettings.Controls.Add(this.txtTaggerFile, 1, 2);
            this.pnlSettings.Controls.Add(this.btnBrowseTaggerFile, 2, 2);
            this.pnlSettings.Controls.Add(this.txtLemmatizerFile, 1, 4);
            this.pnlSettings.Controls.Add(this.btnBrowseLemmatizerFile, 2, 4);
            this.pnlSettings.Controls.Add(this.lblEnterLemmatizerFile, 0, 4);
            this.pnlSettings.Controls.Add(this.txtOutputFile, 1, 6);
            this.pnlSettings.Controls.Add(this.btnBrowseOutputFile, 2, 6);
            this.pnlSettings.Controls.Add(this.lblEnterOutputFile, 0, 6);
            this.pnlSettings.Location = new System.Drawing.Point(6, 19);
            this.pnlSettings.Name = "pnlSettings";
            this.pnlSettings.RowCount = 7;
            this.pnlSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 5F));
            this.pnlSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 5F));
            this.pnlSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.pnlSettings.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.pnlSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.pnlSettings.Size = new System.Drawing.Size(570, 137);
            this.pnlSettings.TabIndex = 1;
            // 
            // lblEnterTextFile
            // 
            this.lblEnterTextFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblEnterTextFile.AutoSize = true;
            this.lblEnterTextFile.Location = new System.Drawing.Point(3, 6);
            this.lblEnterTextFile.Name = "lblEnterTextFile";
            this.lblEnterTextFile.Size = new System.Drawing.Size(129, 15);
            this.lblEnterTextFile.TabIndex = 0;
            this.lblEnterTextFile.Text = "Datoteka z besedilom:";
            // 
            // btnBrowseTextFile
            // 
            this.btnBrowseTextFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseTextFile.Image = global::PosTaggerTagGui.Properties.Resources.folder;
            this.btnBrowseTextFile.Location = new System.Drawing.Point(535, 3);
            this.btnBrowseTextFile.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.btnBrowseTextFile.Name = "btnBrowseTextFile";
            this.btnBrowseTextFile.Size = new System.Drawing.Size(32, 22);
            this.btnBrowseTextFile.TabIndex = 2;
            this.btnBrowseTextFile.UseVisualStyleBackColor = true;
            // 
            // txtTextFile
            // 
            this.txtTextFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTextFile.BackColor = System.Drawing.Color.White;
            this.txtTextFile.Location = new System.Drawing.Point(140, 7);
            this.txtTextFile.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.txtTextFile.Name = "txtTextFile";
            this.txtTextFile.Size = new System.Drawing.Size(395, 20);
            this.txtTextFile.TabIndex = 1;
            // 
            // lblEnterTaggerFile
            // 
            this.lblEnterTaggerFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblEnterTaggerFile.AutoSize = true;
            this.lblEnterTaggerFile.Location = new System.Drawing.Point(3, 39);
            this.lblEnterTaggerFile.Name = "lblEnterTaggerFile";
            this.lblEnterTaggerFile.Size = new System.Drawing.Size(133, 15);
            this.lblEnterTaggerFile.TabIndex = 3;
            this.lblEnterTaggerFile.Text = "Model za označevanje:";
            // 
            // txtTaggerFile
            // 
            this.txtTaggerFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTaggerFile.BackColor = System.Drawing.Color.White;
            this.txtTaggerFile.Location = new System.Drawing.Point(140, 37);
            this.txtTaggerFile.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.txtTaggerFile.Name = "txtTaggerFile";
            this.txtTaggerFile.Size = new System.Drawing.Size(395, 20);
            this.txtTaggerFile.TabIndex = 4;
            // 
            // btnBrowseTaggerFile
            // 
            this.btnBrowseTaggerFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseTaggerFile.Image = ((System.Drawing.Image)(resources.GetObject("btnBrowseTaggerFile.Image")));
            this.btnBrowseTaggerFile.Location = new System.Drawing.Point(535, 36);
            this.btnBrowseTaggerFile.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.btnBrowseTaggerFile.Name = "btnBrowseTaggerFile";
            this.btnBrowseTaggerFile.Size = new System.Drawing.Size(32, 22);
            this.btnBrowseTaggerFile.TabIndex = 5;
            this.btnBrowseTaggerFile.UseVisualStyleBackColor = true;
            // 
            // txtLemmatizerFile
            // 
            this.txtLemmatizerFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLemmatizerFile.BackColor = System.Drawing.Color.White;
            this.txtLemmatizerFile.Location = new System.Drawing.Point(140, 70);
            this.txtLemmatizerFile.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.txtLemmatizerFile.Name = "txtLemmatizerFile";
            this.txtLemmatizerFile.Size = new System.Drawing.Size(395, 20);
            this.txtLemmatizerFile.TabIndex = 9;
            // 
            // btnBrowseLemmatizerFile
            // 
            this.btnBrowseLemmatizerFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseLemmatizerFile.Image = ((System.Drawing.Image)(resources.GetObject("btnBrowseLemmatizerFile.Image")));
            this.btnBrowseLemmatizerFile.Location = new System.Drawing.Point(535, 69);
            this.btnBrowseLemmatizerFile.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.btnBrowseLemmatizerFile.Name = "btnBrowseLemmatizerFile";
            this.btnBrowseLemmatizerFile.Size = new System.Drawing.Size(32, 22);
            this.btnBrowseLemmatizerFile.TabIndex = 10;
            this.btnBrowseLemmatizerFile.UseVisualStyleBackColor = true;
            // 
            // lblEnterLemmatizerFile
            // 
            this.lblEnterLemmatizerFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblEnterLemmatizerFile.AutoSize = true;
            this.lblEnterLemmatizerFile.Location = new System.Drawing.Point(3, 72);
            this.lblEnterLemmatizerFile.Name = "lblEnterLemmatizerFile";
            this.lblEnterLemmatizerFile.Size = new System.Drawing.Size(130, 15);
            this.lblEnterLemmatizerFile.TabIndex = 8;
            this.lblEnterLemmatizerFile.Text = "Model za lematizacijo:";
            // 
            // txtOutputFile
            // 
            this.txtOutputFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutputFile.BackColor = System.Drawing.Color.LightYellow;
            this.txtOutputFile.Location = new System.Drawing.Point(140, 113);
            this.txtOutputFile.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.txtOutputFile.Name = "txtOutputFile";
            this.txtOutputFile.Size = new System.Drawing.Size(395, 20);
            this.txtOutputFile.TabIndex = 12;
            // 
            // btnBrowseOutputFile
            // 
            this.btnBrowseOutputFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseOutputFile.Image = ((System.Drawing.Image)(resources.GetObject("btnBrowseOutputFile.Image")));
            this.btnBrowseOutputFile.Location = new System.Drawing.Point(535, 112);
            this.btnBrowseOutputFile.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.btnBrowseOutputFile.Name = "btnBrowseOutputFile";
            this.btnBrowseOutputFile.Size = new System.Drawing.Size(32, 22);
            this.btnBrowseOutputFile.TabIndex = 13;
            this.btnBrowseOutputFile.UseVisualStyleBackColor = true;
            // 
            // lblEnterOutputFile
            // 
            this.lblEnterOutputFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblEnterOutputFile.AutoSize = true;
            this.lblEnterOutputFile.Location = new System.Drawing.Point(3, 115);
            this.lblEnterOutputFile.Name = "lblEnterOutputFile";
            this.lblEnterOutputFile.Size = new System.Drawing.Size(104, 15);
            this.lblEnterOutputFile.TabIndex = 11;
            this.lblEnterOutputFile.Text = "Izhodna datoteka:";
            // 
            // grpButtons
            // 
            this.grpButtons.AutoSize = true;
            this.grpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpButtons.Controls.Add(this.pnlButtons);
            this.grpButtons.Location = new System.Drawing.Point(3, 184);
            this.grpButtons.Name = "grpButtons";
            this.grpButtons.Size = new System.Drawing.Size(582, 70);
            this.grpButtons.TabIndex = 2;
            this.grpButtons.TabStop = false;
            this.grpButtons.Text = "Ukazi";
            // 
            // pnlButtons
            // 
            this.pnlButtons.AutoSize = true;
            this.pnlButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlButtons.ColumnCount = 2;
            this.pnlButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 132F));
            this.pnlButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 438F));
            this.pnlButtons.Controls.Add(this.btnTag, 0, 0);
            this.pnlButtons.Controls.Add(this.btnCancel, 1, 0);
            this.pnlButtons.Location = new System.Drawing.Point(6, 19);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.RowCount = 1;
            this.pnlButtons.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlButtons.Size = new System.Drawing.Size(570, 32);
            this.pnlButtons.TabIndex = 0;
            // 
            // btnTag
            // 
            this.btnTag.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTag.Location = new System.Drawing.Point(3, 3);
            this.btnTag.Name = "btnTag";
            this.btnTag.Size = new System.Drawing.Size(126, 26);
            this.btnTag.TabIndex = 0;
            this.btnTag.Text = "Označi besedilo";
            this.btnTag.UseVisualStyleBackColor = true;
            this.btnTag.Click += new System.EventHandler(this.btnTag_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnCancel.Enabled = false;
            this.btnCancel.Location = new System.Drawing.Point(135, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(126, 26);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Prekini označevanje";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // PosTaggerTagForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1078, 635);
            this.Controls.Add(this.pnlSplit);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "PosTaggerTagForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Označevalnik";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PosTaggerTagForm_FormClosing);
            this.pnlSplit.ResumeLayout(false);
            this.pnlSplit.PerformLayout();
            this.grpStatus.ResumeLayout(false);
            this.pnlStatus.ResumeLayout(false);
            this.pnlStatus.PerformLayout();
            this.grpSettings.ResumeLayout(false);
            this.grpSettings.PerformLayout();
            this.pnlSettings.ResumeLayout(false);
            this.pnlSettings.PerformLayout();
            this.grpButtons.ResumeLayout(false);
            this.grpButtons.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel pnlSplit;
        private System.Windows.Forms.GroupBox grpSettings;
        private System.Windows.Forms.GroupBox grpStatus;
        private System.Windows.Forms.TableLayoutPanel pnlSettings;
        private System.Windows.Forms.Label lblEnterTextFile;
        private System.Windows.Forms.TextBox txtTextFile;
        private System.Windows.Forms.Panel pnlStatus;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button btnBrowseTextFile;
        private System.Windows.Forms.Label lblEnterTaggerFile;
        private System.Windows.Forms.TextBox txtTaggerFile;
        private System.Windows.Forms.Button btnBrowseTaggerFile;
        private System.Windows.Forms.TextBox txtLemmatizerFile;
        private System.Windows.Forms.Button btnBrowseLemmatizerFile;
        private System.Windows.Forms.Label lblEnterLemmatizerFile;
        private System.Windows.Forms.TextBox txtOutputFile;
        private System.Windows.Forms.Button btnBrowseOutputFile;
        private System.Windows.Forms.Label lblEnterOutputFile;
        private System.Windows.Forms.GroupBox grpButtons;
        private System.Windows.Forms.TableLayoutPanel pnlButtons;
        private System.Windows.Forms.Button btnTag;
        private System.Windows.Forms.Button btnCancel;

    }
}

