using System;
using System.Windows.Forms;
using System.Threading;
using Latino;
using PosTagger;
using System.Drawing;

namespace PosTaggerTagGui
{
    public partial class PosTaggerTagForm : Form
    {
        private Thread mThread
            = null;

        public PosTaggerTagForm()
        {
            InitializeComponent();
            // initialize LATINO logger
            Logger mLogger = Logger.GetRootLogger();
            mLogger.LocalLevel = Logger.Level.Debug;
            mLogger.LocalOutputType = Logger.OutputType.Custom;
            mLogger.LocalProgressOutputType = Logger.ProgressOutputType.Custom;
            mLogger.CustomOutput = new Logger.CustomOutputDelegate(delegate(string loggerName, Logger.Level level, string funcName, Exception e, string message, object[] msgArgs) 
            {
                try
                {
                    Invoke(new ThreadStart(delegate()
                    {
                        if (txtStatus.Text != "") { txtStatus.AppendText("\r\n"); }
                        txtStatus.AppendText(string.Format(message, msgArgs));
                        txtStatus.SelectionStart = txtStatus.TextLength;
                        txtStatus.ScrollToCaret();
                    }));
                }
                catch { }
            });
            mLogger.CustomProgressOutput = new Logger.CustomProgressOutputDelegate(delegate(string loggerName, object sender, int freq, string funcName, string message, int step, int numSteps, object[] args) 
            {
                try
                {
                    Invoke(new ThreadStart(delegate()
                    {
                        if (numSteps == step)
                        {
                            progressBar.Visible = false;
                        }
                        else
                        {                            
                            progressBar.Maximum = numSteps;
                            progressBar.Value = step;
                            progressBar.Visible = true;
                        }
                    }));
                } 
                catch { }
            });
        }

        private void DisableForm()
        {
            btnCancel.Enabled = true;
            btnCancel.Focus();
            foreach (Control ctrl in new Control[] { btnBrowseTextFile, btnBrowseTaggerFile, btnBrowseLemmatizerFile, btnBrowseOutputFile, btnTag })
            {
                ctrl.Enabled = false;
            }
            foreach (TextBox ctrl in new TextBox[] { txtTextFile, txtTaggerFile, txtLemmatizerFile, txtOutputFile })
            {
                ctrl.ReadOnly = true;
                ctrl.BackColor = SystemColors.Control;
            }
        }

        private void EnableForm()
        {
            btnTag.Enabled = true;
            btnTag.Focus();
            btnCancel.Enabled = false;
            foreach (Control ctrl in new Control[] { btnBrowseTextFile, btnBrowseTaggerFile, btnBrowseLemmatizerFile, btnBrowseOutputFile })
            {
                ctrl.Enabled = true;
            }
            foreach (TextBox ctrl in new TextBox[] { txtTextFile, txtTaggerFile, txtLemmatizerFile })
            {
                ctrl.ReadOnly = false;
                ctrl.BackColor = Color.FromKnownColor(KnownColor.White);
            }
            txtOutputFile.BackColor = Color.FromKnownColor(KnownColor.LightYellow);
        }

        private void PosTaggerTagForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                mThread.Abort();
            }
            catch { }
        }

        private void btnTag_Click(object sender, EventArgs e)
        {
            DisableForm();
            txtStatus.Clear();
            mThread = new Thread(new ThreadStart(delegate()
            {
                PosTaggerTag.Tag(new string[] { "-v", "-k", "-xml", @"-lem:C:\Work\PosTagger\Data\lemmatizer.bin", @"C:\Work\PosTagger\Data\jos100k-test.xml",
                    @"C:\Work\PosTagger\Data\jos100k-train.bin", @"C:\Work\PosTagger\Data\output.xml" });
                Invoke(new ThreadStart(delegate() { progressBar.Visible = false; EnableForm(); }));
            }));
            mThread.Start();
        }
    }
}
