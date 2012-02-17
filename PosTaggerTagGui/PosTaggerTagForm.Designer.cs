using Latino;

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PosTaggerTagForm));
            this.pnlSplit = new System.Windows.Forms.TableLayoutPanel();
            this.grpStatus = new System.Windows.Forms.GroupBox();
            this.pnlStatus = new System.Windows.Forms.Panel();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.grpModels = new System.Windows.Forms.GroupBox();
            this.pnlModels = new System.Windows.Forms.TableLayoutPanel();
            this.lblTaggerFile = new System.Windows.Forms.Label();
            this.txtTaggerFile = new System.Windows.Forms.TextBox();
            this.btnTaggerFile = new System.Windows.Forms.Button();
            this.txtLemmatizerFile = new System.Windows.Forms.TextBox();
            this.btnLemmatizerFile = new System.Windows.Forms.Button();
            this.lblLemmatizerFile = new System.Windows.Forms.Label();
            this.grpButtons = new System.Windows.Forms.GroupBox();
            this.pnlButtons = new System.Windows.Forms.TableLayoutPanel();
            this.btnTag = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnAbout = new System.Windows.Forms.Button();
            this.grpFiles = new System.Windows.Forms.GroupBox();
            this.pnlFiles = new System.Windows.Forms.TableLayoutPanel();
            this.btnInputFolder = new System.Windows.Forms.Button();
            this.btnInputFile = new System.Windows.Forms.Button();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.lblInput = new System.Windows.Forms.Label();
            this.chkIncludeSubfolders = new System.Windows.Forms.CheckBox();
            this.lblOutput = new System.Windows.Forms.Label();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.chkOverwriteFiles = new System.Windows.Forms.CheckBox();
            this.btnOutputFolder = new System.Windows.Forms.Button();
            this.btnOutputFile = new System.Windows.Forms.Button();
            this.dlgInputFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.dlgInputFile = new System.Windows.Forms.OpenFileDialog();
            this.dlgOutputFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.dlgOutputFile = new System.Windows.Forms.SaveFileDialog();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.dlgTaggerFile = new System.Windows.Forms.OpenFileDialog();
            this.dlgLemmatizerFile = new System.Windows.Forms.OpenFileDialog();
            this.pnlSplit.SuspendLayout();
            this.grpStatus.SuspendLayout();
            this.pnlStatus.SuspendLayout();
            this.grpModels.SuspendLayout();
            this.pnlModels.SuspendLayout();
            this.grpButtons.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.grpFiles.SuspendLayout();
            this.pnlFiles.SuspendLayout();
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
            this.pnlSplit.Controls.Add(this.grpFiles, 0, 0);
            this.pnlSplit.Location = new System.Drawing.Point(3, 0);
            this.pnlSplit.Name = "pnlSplit";
            this.pnlSplit.RowCount = 4;
            this.pnlSplit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlSplit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlSplit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlSplit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlSplit.Size = new System.Drawing.Size(588, 553);
            this.pnlSplit.TabIndex = 0;
            // 
            // grpStatus
            // 
            this.grpStatus.AutoSize = true;
            this.grpStatus.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpStatus.Controls.Add(this.pnlStatus);
            this.grpStatus.Location = new System.Drawing.Point(3, 342);
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
            this.txtStatus.TabIndex = 14;
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
            this.grpModels.Location = new System.Drawing.Point(3, 162);
            this.grpModels.Name = "grpModels";
            this.grpModels.Size = new System.Drawing.Size(582, 98);
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
            this.pnlModels.Controls.Add(this.btnTaggerFile, 2, 0);
            this.pnlModels.Controls.Add(this.txtLemmatizerFile, 1, 1);
            this.pnlModels.Controls.Add(this.btnLemmatizerFile, 2, 1);
            this.pnlModels.Controls.Add(this.lblLemmatizerFile, 0, 1);
            this.pnlModels.Location = new System.Drawing.Point(6, 19);
            this.pnlModels.Name = "pnlModels";
            this.pnlModels.RowCount = 2;
            this.pnlModels.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlModels.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlModels.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.pnlModels.Size = new System.Drawing.Size(570, 60);
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
            // btnTaggerFile
            // 
            this.btnTaggerFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTaggerFile.Image = global::PosTaggerTagGui.Properties.Resources.folder_page_white;
            this.btnTaggerFile.Location = new System.Drawing.Point(535, 3);
            this.btnTaggerFile.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.btnTaggerFile.Name = "btnTaggerFile";
            this.btnTaggerFile.Size = new System.Drawing.Size(32, 24);
            this.btnTaggerFile.TabIndex = 8;
            this.toolTip.SetToolTip(this.btnTaggerFile, "Izberi model za označevanje...");
            this.btnTaggerFile.UseVisualStyleBackColor = true;
            this.btnTaggerFile.Click += new System.EventHandler(this.btnTaggerFile_Click);
            // 
            // txtLemmatizerFile
            // 
            this.txtLemmatizerFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLemmatizerFile.BackColor = System.Drawing.Color.White;
            this.txtLemmatizerFile.Location = new System.Drawing.Point(140, 35);
            this.txtLemmatizerFile.Margin = new System.Windows.Forms.Padding(0, 3, 1, 3);
            this.txtLemmatizerFile.Name = "txtLemmatizerFile";
            this.txtLemmatizerFile.Size = new System.Drawing.Size(394, 20);
            this.txtLemmatizerFile.TabIndex = 9;
            // 
            // btnLemmatizerFile
            // 
            this.btnLemmatizerFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLemmatizerFile.Image = global::PosTaggerTagGui.Properties.Resources.folder_page_white;
            this.btnLemmatizerFile.Location = new System.Drawing.Point(535, 33);
            this.btnLemmatizerFile.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.btnLemmatizerFile.Name = "btnLemmatizerFile";
            this.btnLemmatizerFile.Size = new System.Drawing.Size(32, 24);
            this.btnLemmatizerFile.TabIndex = 10;
            this.toolTip.SetToolTip(this.btnLemmatizerFile, "Izberi model za lematizacijo...");
            this.btnLemmatizerFile.UseVisualStyleBackColor = true;
            this.btnLemmatizerFile.Click += new System.EventHandler(this.btnLemmatizerFile_Click);
            // 
            // lblLemmatizerFile
            // 
            this.lblLemmatizerFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblLemmatizerFile.AutoSize = true;
            this.lblLemmatizerFile.Location = new System.Drawing.Point(3, 37);
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
            this.grpButtons.Location = new System.Drawing.Point(3, 266);
            this.grpButtons.Name = "grpButtons";
            this.grpButtons.Size = new System.Drawing.Size(582, 70);
            this.grpButtons.TabIndex = 2;
            this.grpButtons.TabStop = false;
            // 
            // pnlButtons
            // 
            this.pnlButtons.AutoSize = true;
            this.pnlButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlButtons.ColumnCount = 4;
            this.pnlButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 131F));
            this.pnlButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 372F));
            this.pnlButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.pnlButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.pnlButtons.Controls.Add(this.btnTag, 0, 0);
            this.pnlButtons.Controls.Add(this.btnCancel, 1, 0);
            this.pnlButtons.Controls.Add(this.btnExit, 3, 0);
            this.pnlButtons.Controls.Add(this.btnAbout, 2, 0);
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
            this.btnTag.Size = new System.Drawing.Size(125, 26);
            this.btnTag.TabIndex = 11;
            this.btnTag.Text = "Označi besedila";
            this.btnTag.UseVisualStyleBackColor = true;
            this.btnTag.Click += new System.EventHandler(this.btnTag_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnCancel.Enabled = false;
            this.btnCancel.Location = new System.Drawing.Point(131, 3);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(125, 26);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "Prekini označevanje";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.Image = global::PosTaggerTagGui.Properties.Resources.door_in;
            this.btnExit.Location = new System.Drawing.Point(535, 4);
            this.btnExit.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(32, 24);
            this.btnExit.TabIndex = 13;
            this.toolTip.SetToolTip(this.btnExit, "Izhod iz programa");
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnAbout
            // 
            this.btnAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAbout.Image = global::PosTaggerTagGui.Properties.Resources.help;
            this.btnAbout.Location = new System.Drawing.Point(503, 4);
            this.btnAbout.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.Size = new System.Drawing.Size(32, 24);
            this.btnAbout.TabIndex = 12;
            this.toolTip.SetToolTip(this.btnAbout, "O programu...");
            this.btnAbout.UseVisualStyleBackColor = true;
            this.btnAbout.Visible = false;
            // 
            // grpFiles
            // 
            this.grpFiles.AutoSize = true;
            this.grpFiles.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpFiles.Controls.Add(this.pnlFiles);
            this.grpFiles.Location = new System.Drawing.Point(3, 3);
            this.grpFiles.Name = "grpFiles";
            this.grpFiles.Size = new System.Drawing.Size(582, 153);
            this.grpFiles.TabIndex = 0;
            this.grpFiles.TabStop = false;
            this.grpFiles.Text = "Datoteke";
            // 
            // pnlFiles
            // 
            this.pnlFiles.AutoSize = true;
            this.pnlFiles.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlFiles.ColumnCount = 4;
            this.pnlFiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.pnlFiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 363F));
            this.pnlFiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.pnlFiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.pnlFiles.Controls.Add(this.btnInputFolder, 3, 0);
            this.pnlFiles.Controls.Add(this.btnInputFile, 2, 0);
            this.pnlFiles.Controls.Add(this.txtInput, 1, 0);
            this.pnlFiles.Controls.Add(this.lblInput, 0, 0);
            this.pnlFiles.Controls.Add(this.chkIncludeSubfolders, 1, 1);
            this.pnlFiles.Controls.Add(this.lblOutput, 0, 3);
            this.pnlFiles.Controls.Add(this.txtOutput, 1, 3);
            this.pnlFiles.Controls.Add(this.chkOverwriteFiles, 1, 4);
            this.pnlFiles.Controls.Add(this.btnOutputFolder, 3, 3);
            this.pnlFiles.Controls.Add(this.btnOutputFile, 2, 3);
            this.pnlFiles.Location = new System.Drawing.Point(6, 19);
            this.pnlFiles.Name = "pnlFiles";
            this.pnlFiles.RowCount = 5;
            this.pnlFiles.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlFiles.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlFiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 5F));
            this.pnlFiles.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlFiles.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlFiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.pnlFiles.Size = new System.Drawing.Size(570, 115);
            this.pnlFiles.TabIndex = 0;
            // 
            // btnInputFolder
            // 
            this.btnInputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInputFolder.Image = global::PosTaggerTagGui.Properties.Resources.folder;
            this.btnInputFolder.Location = new System.Drawing.Point(535, 3);
            this.btnInputFolder.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.btnInputFolder.Name = "btnInputFolder";
            this.btnInputFolder.Size = new System.Drawing.Size(32, 24);
            this.btnInputFolder.TabIndex = 2;
            this.toolTip.SetToolTip(this.btnInputFolder, "Izberi vhodno mapo...");
            this.btnInputFolder.UseVisualStyleBackColor = true;
            this.btnInputFolder.Click += new System.EventHandler(this.btnInputFolder_Click);
            // 
            // btnInputFile
            // 
            this.btnInputFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInputFile.Image = global::PosTaggerTagGui.Properties.Resources.folder_page_white;
            this.btnInputFile.Location = new System.Drawing.Point(503, 3);
            this.btnInputFile.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.btnInputFile.Name = "btnInputFile";
            this.btnInputFile.Size = new System.Drawing.Size(32, 24);
            this.btnInputFile.TabIndex = 1;
            this.toolTip.SetToolTip(this.btnInputFile, "Izberi vhodno datoteko...");
            this.btnInputFile.UseVisualStyleBackColor = true;
            this.btnInputFile.Click += new System.EventHandler(this.btnInputFile_Click);
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
            // chkOverwriteFiles
            // 
            this.chkOverwriteFiles.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkOverwriteFiles.AutoSize = true;
            this.chkOverwriteFiles.Location = new System.Drawing.Point(140, 93);
            this.chkOverwriteFiles.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.chkOverwriteFiles.Name = "chkOverwriteFiles";
            this.chkOverwriteFiles.Size = new System.Drawing.Size(161, 19);
            this.chkOverwriteFiles.TabIndex = 9;
            this.chkOverwriteFiles.Text = "Prepiši izhodne datoteke";
            this.chkOverwriteFiles.UseVisualStyleBackColor = true;
            // 
            // btnOutputFolder
            // 
            this.btnOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOutputFolder.Image = global::PosTaggerTagGui.Properties.Resources.folder;
            this.btnOutputFolder.Location = new System.Drawing.Point(535, 63);
            this.btnOutputFolder.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.btnOutputFolder.Name = "btnOutputFolder";
            this.btnOutputFolder.Size = new System.Drawing.Size(32, 24);
            this.btnOutputFolder.TabIndex = 6;
            this.toolTip.SetToolTip(this.btnOutputFolder, "Izberi izhodno mapo...");
            this.btnOutputFolder.UseVisualStyleBackColor = true;
            this.btnOutputFolder.Click += new System.EventHandler(this.btnOutputFolder_Click);
            // 
            // btnOutputFile
            // 
            this.btnOutputFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOutputFile.Image = global::PosTaggerTagGui.Properties.Resources.folder_page_white;
            this.btnOutputFile.Location = new System.Drawing.Point(503, 63);
            this.btnOutputFile.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.btnOutputFile.Name = "btnOutputFile";
            this.btnOutputFile.Size = new System.Drawing.Size(32, 24);
            this.btnOutputFile.TabIndex = 5;
            this.toolTip.SetToolTip(this.btnOutputFile, "Določi izhodno datoteko...");
            this.btnOutputFile.UseVisualStyleBackColor = true;
            this.btnOutputFile.Click += new System.EventHandler(this.btnOutputFile_Click);
            // 
            // dlgInputFolder
            // 
            this.dlgInputFolder.Description = "Izberi mapo z vhodnimi datotekami:";
            this.dlgInputFolder.ShowNewFolderButton = false;
            // 
            // dlgInputFile
            // 
            this.dlgInputFile.DefaultExt = "txt";
            this.dlgInputFile.Filter = "Tekstovne datoteke|*.txt|Datoteke XML-TEI|*.xml|Vse datoteke|*.*";
            this.dlgInputFile.Title = "Izberi vhodno datoteko";
            // 
            // dlgOutputFolder
            // 
            this.dlgOutputFolder.Description = "Izberi mapo za izhodne datoteke:";
            // 
            // dlgOutputFile
            // 
            this.dlgOutputFile.DefaultExt = "txt";
            this.dlgOutputFile.Filter = "Datoteke XML-TEI|*.xml|Vse datoteke|*.*";
            this.dlgOutputFile.Title = "Določi izhodno datoteko";
            // 
            // dlgTaggerFile
            // 
            this.dlgTaggerFile.DefaultExt = "bin";
            this.dlgTaggerFile.Filter = "Binarne datoteke|*.bin|Vse datoteke|*.*";
            this.dlgTaggerFile.Title = "Izberi model za označevanje";
            // 
            // dlgLemmatizerFile
            // 
            this.dlgLemmatizerFile.DefaultExt = "bin";
            this.dlgLemmatizerFile.Filter = "Binarne datoteke|*.bin|Vse datoteke|*.*";
            this.dlgLemmatizerFile.Title = "Izberi model za lematizacijo";
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
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "PosTaggerTagForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Obeliks [Oblikoslovni označevalnik za Slovenščino]";
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
            this.grpFiles.ResumeLayout(false);
            this.grpFiles.PerformLayout();
            this.pnlFiles.ResumeLayout(false);
            this.pnlFiles.PerformLayout();
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
        private System.Windows.Forms.Button btnTaggerFile;
        private System.Windows.Forms.TextBox txtLemmatizerFile;
        private System.Windows.Forms.Button btnLemmatizerFile;
        private System.Windows.Forms.Label lblLemmatizerFile;
        private System.Windows.Forms.GroupBox grpButtons;
        private System.Windows.Forms.TableLayoutPanel pnlButtons;
        private System.Windows.Forms.Button btnTag;
        private System.Windows.Forms.GroupBox grpFiles;
        private System.Windows.Forms.TableLayoutPanel pnlFiles;
        private System.Windows.Forms.Button btnInputFolder;
        private System.Windows.Forms.Button btnInputFile;
        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.Label lblInput;
        private System.Windows.Forms.FolderBrowserDialog dlgInputFolder;
        private System.Windows.Forms.OpenFileDialog dlgInputFile;
        private System.Windows.Forms.CheckBox chkIncludeSubfolders;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.Label lblOutput;
        private System.Windows.Forms.Button btnOutputFolder;
        private System.Windows.Forms.Button btnOutputFile;
        private System.Windows.Forms.FolderBrowserDialog dlgOutputFolder;
        private System.Windows.Forms.SaveFileDialog dlgOutputFile;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.OpenFileDialog dlgTaggerFile;
        private System.Windows.Forms.OpenFileDialog dlgLemmatizerFile;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnAbout;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.CheckBox chkOverwriteFiles;

    }
}

