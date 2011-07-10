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
            this.pnlSplit = new System.Windows.Forms.TableLayoutPanel();
            this.grpStatus = new System.Windows.Forms.GroupBox();
            this.pnlStatus = new System.Windows.Forms.Panel();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.grpModels = new System.Windows.Forms.GroupBox();
            this.pnlModels = new System.Windows.Forms.TableLayoutPanel();
            this.lblTaggerFile = new System.Windows.Forms.Label();
            this.txtTaggerFile = new System.Windows.Forms.TextBox();
            this.txtLemmatizerFile = new System.Windows.Forms.TextBox();
            this.lblLemmatizerFile = new System.Windows.Forms.Label();
            this.grpButtons = new System.Windows.Forms.GroupBox();
            this.pnlButtons = new System.Windows.Forms.TableLayoutPanel();
            this.btnTag = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpInputOutput = new System.Windows.Forms.GroupBox();
            this.pnlInputOutput = new System.Windows.Forms.TableLayoutPanel();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.lblInput = new System.Windows.Forms.Label();
            this.chkIncludeSubfolders = new System.Windows.Forms.CheckBox();
            this.lblOutput = new System.Windows.Forms.Label();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.dlgInputFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.dlgInputFile = new System.Windows.Forms.OpenFileDialog();
            this.dlgOutputFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.dlgOutputFile = new System.Windows.Forms.SaveFileDialog();
            this.btnBrowseTaggerFile = new System.Windows.Forms.Button();
            this.btnBrowseLemmatizerFile = new System.Windows.Forms.Button();
            this.btnSelectInputFolder = new System.Windows.Forms.Button();
            this.btnSelectInputFile = new System.Windows.Forms.Button();
            this.btnSelectOutputFile = new System.Windows.Forms.Button();
            this.btnSelectOutputFolder = new System.Windows.Forms.Button();
            this.pnlSplit.SuspendLayout();
            this.grpStatus.SuspendLayout();
            this.pnlStatus.SuspendLayout();
            this.grpModels.SuspendLayout();
            this.pnlModels.SuspendLayout();
            this.grpButtons.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.grpInputOutput.SuspendLayout();
            this.pnlInputOutput.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlSplit
            // 
            this.pnlSplit.AutoSize = true;
            this.pnlSplit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlSplit.ColumnCount = 1;
            this.pnlSplit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlSplit.Controls.Add(this.grpStatus, 0, 3);
            this.pnlSplit.Controls.Add(this.grpModels, 0, 1);
            this.pnlSplit.Controls.Add(this.grpButtons, 0, 2);
            this.pnlSplit.Controls.Add(this.grpInputOutput, 0, 0);
            this.pnlSplit.Location = new System.Drawing.Point(3, 0);
            this.pnlSplit.Name = "pnlSplit";
            this.pnlSplit.RowCount = 4;
            this.pnlSplit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlSplit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlSplit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlSplit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlSplit.Size = new System.Drawing.Size(588, 533);
            this.pnlSplit.TabIndex = 0;
            // 
            // grpStatus
            // 
            this.grpStatus.AutoSize = true;
            this.grpStatus.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpStatus.Controls.Add(this.pnlStatus);
            this.grpStatus.Location = new System.Drawing.Point(3, 322);
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
            this.txtStatus.TabIndex = 13;
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
            // grpModels
            // 
            this.grpModels.AutoSize = true;
            this.grpModels.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpModels.Controls.Add(this.pnlModels);
            this.grpModels.Location = new System.Drawing.Point(3, 137);
            this.grpModels.Name = "grpModels";
            this.grpModels.Size = new System.Drawing.Size(582, 103);
            this.grpModels.TabIndex = 1;
            this.grpModels.TabStop = false;
            this.grpModels.Text = "Modeli";
            // 
            // pnlModels
            // 
            this.pnlModels.AutoSize = true;
            this.pnlModels.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlModels.ColumnCount = 3;
            this.pnlModels.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.pnlModels.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 395F));
            this.pnlModels.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.pnlModels.Controls.Add(this.lblTaggerFile, 0, 0);
            this.pnlModels.Controls.Add(this.txtTaggerFile, 1, 0);
            this.pnlModels.Controls.Add(this.btnBrowseTaggerFile, 2, 0);
            this.pnlModels.Controls.Add(this.txtLemmatizerFile, 1, 2);
            this.pnlModels.Controls.Add(this.btnBrowseLemmatizerFile, 2, 2);
            this.pnlModels.Controls.Add(this.lblLemmatizerFile, 0, 2);
            this.pnlModels.Location = new System.Drawing.Point(6, 19);
            this.pnlModels.Name = "pnlModels";
            this.pnlModels.RowCount = 3;
            this.pnlModels.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlModels.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 5F));
            this.pnlModels.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlModels.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.pnlModels.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.pnlModels.Size = new System.Drawing.Size(570, 65);
            this.pnlModels.TabIndex = 1;
            // 
            // lblTaggerFile
            // 
            this.lblTaggerFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblTaggerFile.AutoSize = true;
            this.lblTaggerFile.Location = new System.Drawing.Point(3, 7);
            this.lblTaggerFile.Name = "lblTaggerFile";
            this.lblTaggerFile.Size = new System.Drawing.Size(133, 15);
            this.lblTaggerFile.TabIndex = 3;
            this.lblTaggerFile.Text = "Model za označevanje:";
            // 
            // txtTaggerFile
            // 
            this.txtTaggerFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTaggerFile.BackColor = System.Drawing.Color.White;
            this.txtTaggerFile.Location = new System.Drawing.Point(140, 5);
            this.txtTaggerFile.Margin = new System.Windows.Forms.Padding(0, 3, 1, 3);
            this.txtTaggerFile.Name = "txtTaggerFile";
            this.txtTaggerFile.Size = new System.Drawing.Size(394, 20);
            this.txtTaggerFile.TabIndex = 7;
            // 
            // txtLemmatizerFile
            // 
            this.txtLemmatizerFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLemmatizerFile.BackColor = System.Drawing.Color.White;
            this.txtLemmatizerFile.Location = new System.Drawing.Point(140, 40);
            this.txtLemmatizerFile.Margin = new System.Windows.Forms.Padding(0, 3, 1, 3);
            this.txtLemmatizerFile.Name = "txtLemmatizerFile";
            this.txtLemmatizerFile.Size = new System.Drawing.Size(394, 20);
            this.txtLemmatizerFile.TabIndex = 9;
            // 
            // lblLemmatizerFile
            // 
            this.lblLemmatizerFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblLemmatizerFile.AutoSize = true;
            this.lblLemmatizerFile.Location = new System.Drawing.Point(3, 42);
            this.lblLemmatizerFile.Name = "lblLemmatizerFile";
            this.lblLemmatizerFile.Size = new System.Drawing.Size(130, 15);
            this.lblLemmatizerFile.TabIndex = 8;
            this.lblLemmatizerFile.Text = "Model za lematizacijo:";
            // 
            // grpButtons
            // 
            this.grpButtons.AutoSize = true;
            this.grpButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpButtons.Controls.Add(this.pnlButtons);
            this.grpButtons.Location = new System.Drawing.Point(3, 246);
            this.grpButtons.Name = "grpButtons";
            this.grpButtons.Size = new System.Drawing.Size(582, 70);
            this.grpButtons.TabIndex = 2;
            this.grpButtons.TabStop = false;
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
            this.pnlButtons.TabIndex = 2;
            // 
            // btnTag
            // 
            this.btnTag.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTag.Location = new System.Drawing.Point(3, 3);
            this.btnTag.Name = "btnTag";
            this.btnTag.Size = new System.Drawing.Size(126, 26);
            this.btnTag.TabIndex = 11;
            this.btnTag.Text = "Označi besedilo";
            this.btnTag.UseVisualStyleBackColor = true;
            this.btnTag.Click += new System.EventHandler(this.btnTag_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnCancel.Enabled = false;
            this.btnCancel.Location = new System.Drawing.Point(132, 3);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(126, 26);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "Prekini označevanje";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // grpInputOutput
            // 
            this.grpInputOutput.AutoSize = true;
            this.grpInputOutput.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpInputOutput.Controls.Add(this.pnlInputOutput);
            this.grpInputOutput.Location = new System.Drawing.Point(3, 3);
            this.grpInputOutput.Name = "grpInputOutput";
            this.grpInputOutput.Size = new System.Drawing.Size(582, 128);
            this.grpInputOutput.TabIndex = 0;
            this.grpInputOutput.TabStop = false;
            this.grpInputOutput.Text = "Besedila";
            // 
            // pnlInputOutput
            // 
            this.pnlInputOutput.AutoSize = true;
            this.pnlInputOutput.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlInputOutput.ColumnCount = 4;
            this.pnlInputOutput.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.pnlInputOutput.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 363F));
            this.pnlInputOutput.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.pnlInputOutput.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.pnlInputOutput.Controls.Add(this.btnSelectInputFolder, 3, 0);
            this.pnlInputOutput.Controls.Add(this.btnSelectInputFile, 2, 0);
            this.pnlInputOutput.Controls.Add(this.txtInput, 1, 0);
            this.pnlInputOutput.Controls.Add(this.lblInput, 0, 0);
            this.pnlInputOutput.Controls.Add(this.chkIncludeSubfolders, 1, 1);
            this.pnlInputOutput.Controls.Add(this.lblOutput, 0, 3);
            this.pnlInputOutput.Controls.Add(this.btnSelectOutputFile, 2, 3);
            this.pnlInputOutput.Controls.Add(this.btnSelectOutputFolder, 2, 3);
            this.pnlInputOutput.Controls.Add(this.txtOutput, 1, 3);
            this.pnlInputOutput.Location = new System.Drawing.Point(6, 19);
            this.pnlInputOutput.Name = "pnlInputOutput";
            this.pnlInputOutput.RowCount = 4;
            this.pnlInputOutput.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlInputOutput.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlInputOutput.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 5F));
            this.pnlInputOutput.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlInputOutput.Size = new System.Drawing.Size(570, 90);
            this.pnlInputOutput.TabIndex = 0;
            // 
            // txtInput
            // 
            this.txtInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInput.BackColor = System.Drawing.Color.White;
            this.txtInput.Location = new System.Drawing.Point(140, 5);
            this.txtInput.Margin = new System.Windows.Forms.Padding(0, 3, 1, 3);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(362, 20);
            this.txtInput.TabIndex = 0;
            // 
            // lblInput
            // 
            this.lblInput.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblInput.AutoSize = true;
            this.lblInput.Location = new System.Drawing.Point(3, 7);
            this.lblInput.Name = "lblInput";
            this.lblInput.Size = new System.Drawing.Size(102, 15);
            this.lblInput.TabIndex = 6;
            this.lblInput.Text = "Vhodne datoteke:";
            // 
            // chkIncludeSubfolders
            // 
            this.chkIncludeSubfolders.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkIncludeSubfolders.AutoSize = true;
            this.chkIncludeSubfolders.Location = new System.Drawing.Point(140, 33);
            this.chkIncludeSubfolders.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.chkIncludeSubfolders.Name = "chkIncludeSubfolders";
            this.chkIncludeSubfolders.Size = new System.Drawing.Size(117, 19);
            this.chkIncludeSubfolders.TabIndex = 3;
            this.chkIncludeSubfolders.Text = "Vključi podmape";
            this.chkIncludeSubfolders.UseVisualStyleBackColor = true;
            // 
            // lblOutput
            // 
            this.lblOutput.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblOutput.AutoSize = true;
            this.lblOutput.Location = new System.Drawing.Point(3, 60);
            this.lblOutput.Name = "lblOutput";
            this.lblOutput.Size = new System.Drawing.Size(104, 30);
            this.lblOutput.TabIndex = 8;
            this.lblOutput.Text = "Izhodna datoteka (mapa):";
            // 
            // txtOutput
            // 
            this.txtOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutput.BackColor = System.Drawing.Color.White;
            this.txtOutput.Location = new System.Drawing.Point(140, 65);
            this.txtOutput.Margin = new System.Windows.Forms.Padding(0, 3, 1, 3);
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.Size = new System.Drawing.Size(362, 20);
            this.txtOutput.TabIndex = 4;
            // 
            // dlgInputFolder
            // 
            this.dlgInputFolder.Description = "Izberi mapo z vhodnimi datotekami:";
            this.dlgInputFolder.ShowNewFolderButton = false;
            // 
            // dlgInputFile
            // 
            this.dlgInputFile.DefaultExt = "txt";
            this.dlgInputFile.Filter = "Tekstovne datoteke|*.txt|Datoteke XML-TEI|*.xml";
            this.dlgInputFile.Title = "Izberi vhodno datoteko";
            // 
            // dlgOutputFolder
            // 
            this.dlgOutputFolder.Description = "Izberi mapo za izhodne datoteke:";
            // 
            // dlgOutputFile
            // 
            this.dlgOutputFile.DefaultExt = "txt";
            this.dlgOutputFile.Filter = "Tekstovne datoteke|*.txt|Datoteke XML-TEI|*.xml";
            this.dlgOutputFile.Title = "Določi izhodno datoteko";
            // 
            // btnBrowseTaggerFile
            // 
            this.btnBrowseTaggerFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseTaggerFile.Image = global::PosTaggerTagGui.Properties.Resources.folder_page_white;
            this.btnBrowseTaggerFile.Location = new System.Drawing.Point(535, 3);
            this.btnBrowseTaggerFile.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.btnBrowseTaggerFile.Name = "btnBrowseTaggerFile";
            this.btnBrowseTaggerFile.Size = new System.Drawing.Size(32, 24);
            this.btnBrowseTaggerFile.TabIndex = 8;
            this.btnBrowseTaggerFile.UseVisualStyleBackColor = true;
            // 
            // btnBrowseLemmatizerFile
            // 
            this.btnBrowseLemmatizerFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseLemmatizerFile.Image = global::PosTaggerTagGui.Properties.Resources.folder_page_white;
            this.btnBrowseLemmatizerFile.Location = new System.Drawing.Point(535, 38);
            this.btnBrowseLemmatizerFile.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.btnBrowseLemmatizerFile.Name = "btnBrowseLemmatizerFile";
            this.btnBrowseLemmatizerFile.Size = new System.Drawing.Size(32, 24);
            this.btnBrowseLemmatizerFile.TabIndex = 10;
            this.btnBrowseLemmatizerFile.UseVisualStyleBackColor = true;
            // 
            // btnSelectInputFolder
            // 
            this.btnSelectInputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectInputFolder.Image = global::PosTaggerTagGui.Properties.Resources.folder;
            this.btnSelectInputFolder.Location = new System.Drawing.Point(535, 3);
            this.btnSelectInputFolder.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.btnSelectInputFolder.Name = "btnSelectInputFolder";
            this.btnSelectInputFolder.Size = new System.Drawing.Size(32, 24);
            this.btnSelectInputFolder.TabIndex = 2;
            this.btnSelectInputFolder.UseVisualStyleBackColor = true;
            this.btnSelectInputFolder.Click += new System.EventHandler(this.btnSelectInputFolder_Click);
            // 
            // btnSelectInputFile
            // 
            this.btnSelectInputFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectInputFile.Image = global::PosTaggerTagGui.Properties.Resources.folder_page_white;
            this.btnSelectInputFile.Location = new System.Drawing.Point(503, 3);
            this.btnSelectInputFile.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.btnSelectInputFile.Name = "btnSelectInputFile";
            this.btnSelectInputFile.Size = new System.Drawing.Size(32, 24);
            this.btnSelectInputFile.TabIndex = 1;
            this.btnSelectInputFile.UseVisualStyleBackColor = true;
            this.btnSelectInputFile.Click += new System.EventHandler(this.btnSelectInputFile_Click);
            // 
            // btnSelectOutputFile
            // 
            this.btnSelectOutputFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectOutputFile.Image = global::PosTaggerTagGui.Properties.Resources.folder_page_white;
            this.btnSelectOutputFile.Location = new System.Drawing.Point(503, 63);
            this.btnSelectOutputFile.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.btnSelectOutputFile.Name = "btnSelectOutputFile";
            this.btnSelectOutputFile.Size = new System.Drawing.Size(32, 24);
            this.btnSelectOutputFile.TabIndex = 5;
            this.btnSelectOutputFile.UseVisualStyleBackColor = true;
            this.btnSelectOutputFile.Click += new System.EventHandler(this.btnSelectOutputFile_Click);
            // 
            // btnSelectOutputFolder
            // 
            this.btnSelectOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectOutputFolder.Image = global::PosTaggerTagGui.Properties.Resources.folder;
            this.btnSelectOutputFolder.Location = new System.Drawing.Point(535, 63);
            this.btnSelectOutputFolder.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.btnSelectOutputFolder.Name = "btnSelectOutputFolder";
            this.btnSelectOutputFolder.Size = new System.Drawing.Size(32, 24);
            this.btnSelectOutputFolder.TabIndex = 6;
            this.btnSelectOutputFolder.UseVisualStyleBackColor = true;
            this.btnSelectOutputFolder.Click += new System.EventHandler(this.btnSelectOutputFolder_Click);
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
            this.grpModels.ResumeLayout(false);
            this.grpModels.PerformLayout();
            this.pnlModels.ResumeLayout(false);
            this.pnlModels.PerformLayout();
            this.grpButtons.ResumeLayout(false);
            this.grpButtons.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.grpInputOutput.ResumeLayout(false);
            this.grpInputOutput.PerformLayout();
            this.pnlInputOutput.ResumeLayout(false);
            this.pnlInputOutput.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel pnlSplit;
        private System.Windows.Forms.GroupBox grpModels;
        private System.Windows.Forms.GroupBox grpStatus;
        private System.Windows.Forms.TableLayoutPanel pnlModels;
        private System.Windows.Forms.Panel pnlStatus;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblTaggerFile;
        private System.Windows.Forms.TextBox txtTaggerFile;
        private System.Windows.Forms.Button btnBrowseTaggerFile;
        private System.Windows.Forms.TextBox txtLemmatizerFile;
        private System.Windows.Forms.Button btnBrowseLemmatizerFile;
        private System.Windows.Forms.Label lblLemmatizerFile;
        private System.Windows.Forms.GroupBox grpButtons;
        private System.Windows.Forms.TableLayoutPanel pnlButtons;
        private System.Windows.Forms.Button btnTag;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpInputOutput;
        private System.Windows.Forms.TableLayoutPanel pnlInputOutput;
        private System.Windows.Forms.Button btnSelectInputFolder;
        private System.Windows.Forms.Button btnSelectInputFile;
        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.Label lblInput;
        private System.Windows.Forms.FolderBrowserDialog dlgInputFolder;
        private System.Windows.Forms.OpenFileDialog dlgInputFile;
        private System.Windows.Forms.CheckBox chkIncludeSubfolders;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.Label lblOutput;
        private System.Windows.Forms.Button btnSelectOutputFolder;
        private System.Windows.Forms.Button btnSelectOutputFile;
        private System.Windows.Forms.FolderBrowserDialog dlgOutputFolder;
        private System.Windows.Forms.SaveFileDialog dlgOutputFile;

    }
}

