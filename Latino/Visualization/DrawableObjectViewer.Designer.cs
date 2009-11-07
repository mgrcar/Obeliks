namespace Latino.Visualization
{
    partial class DrawableObjectViewer
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.picBoxCanvas = new System.Windows.Forms.PictureBox();
            this.FpsInfo = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxCanvas)).BeginInit();
            this.SuspendLayout();
            // 
            // picBoxCanvas
            // 
            this.picBoxCanvas.BackColor = System.Drawing.Color.White;
            this.picBoxCanvas.ErrorImage = null;
            this.picBoxCanvas.InitialImage = null;
            this.picBoxCanvas.Location = new System.Drawing.Point(0, 0);
            this.picBoxCanvas.Name = "picBoxCanvas";
            this.picBoxCanvas.Size = new System.Drawing.Size(800, 600);
            this.picBoxCanvas.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picBoxCanvas.TabIndex = 0;
            this.picBoxCanvas.TabStop = false;
            this.picBoxCanvas.MouseLeave += new System.EventHandler(this.DrawableObjectViewer_MouseLeave);
            this.picBoxCanvas.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DrawableObjectViewer_MouseMove);
            this.picBoxCanvas.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.DrawableObjectViewer_MouseDoubleClick);
            this.picBoxCanvas.MouseClick += new System.Windows.Forms.MouseEventHandler(this.DrawableObjectViewer_MouseClick);
            this.picBoxCanvas.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DrawableObjectViewer_MouseDown);
            this.picBoxCanvas.MouseHover += new System.EventHandler(this.DrawableObjectViewer_MouseHover);
            this.picBoxCanvas.MouseUp += new System.Windows.Forms.MouseEventHandler(this.DrawableObjectViewer_MouseUp);
            // 
            // FpsInfo
            // 
            this.FpsInfo.AutoSize = true;
            this.FpsInfo.Location = new System.Drawing.Point(0, 0);
            this.FpsInfo.Name = "FpsInfo";
            this.FpsInfo.Size = new System.Drawing.Size(78, 13);
            this.FpsInfo.TabIndex = 1;
            this.FpsInfo.Text = "0.00 ms / draw";
            this.FpsInfo.Click += new System.EventHandler(this.FpsInfo_Click);
            // 
            // DrawableObjectViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.FpsInfo);
            this.Controls.Add(this.picBoxCanvas);
            this.Name = "DrawableObjectViewer";
            this.Size = new System.Drawing.Size(99, 99);
            ((System.ComponentModel.ISupportInitialize)(this.picBoxCanvas)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picBoxCanvas;
        private System.Windows.Forms.Label FpsInfo;
    }
}
