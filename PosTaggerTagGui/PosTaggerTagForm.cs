using System;
using System.Configuration;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;
using System.Drawing;
using System.IO;
using PosTagger;
using Latino;
using System.Diagnostics;

namespace PosTaggerTagGui
{
    public partial class PosTaggerTagForm : Form
    {
        private Thread mThread
            = null;
        private Logger mLogger
            = Logger.GetRootLogger();

        public PosTaggerTagForm()
        {
            InitializeComponent();
            // read application configuration
            txtInput.Text = ConfigurationManager.AppSettings["input"];            
            if (ConfigurationManager.AppSettings["includeSubfolders"] != null)
            {
                chkIncludeSubfolders.Checked = Regex.Match(ConfigurationManager.AppSettings["includeSubfolders"],
                    "(true)|(1)|(yes)|(on)", RegexOptions.IgnoreCase).Success;
            }
            if (ConfigurationManager.AppSettings["overwriteFiles"] != null)
            {
                chkOverwriteFiles.Checked = Regex.Match(ConfigurationManager.AppSettings["overwriteFiles"],
                    "(true)|(1)|(yes)|(on)", RegexOptions.IgnoreCase).Success;            
            }
            txtOutput.Text = ConfigurationManager.AppSettings["output"];
            txtTaggerFile.Text = ConfigurationManager.AppSettings["taggerFile"];
            txtLemmatizerFile.Text = ConfigurationManager.AppSettings["lemmatizerFile"];
            long affinity = Convert.ToInt32(Utils.GetConfigValue("affinity", "-1"));
            if (affinity != -1) { Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)affinity; }
            // initialize LATINO logger
            mLogger = Logger.GetRootLogger();
            mLogger.LocalLevel = Logger.Level.Debug;
            mLogger.LocalOutputType = Logger.OutputType.Custom;
            mLogger.LocalProgressOutputType = Logger.ProgressOutputType.Custom;
            Logger.CustomOutput = new Logger.CustomOutputDelegate(delegate(string loggerName, Logger.Level level, string funcName, Exception e, string message, object[] msgArgs) 
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
            Logger.CustomProgressOutput = new Logger.CustomProgressOutputDelegate(delegate(string loggerName, object sender, int freq, string funcName, string message, int step, int numSteps, object[] args) 
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
                            if (!progressBar.Visible)
                            {
                                progressBar.Visible = true;
                                txtStatus.ScrollToCaret();
                            }
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
            foreach (Control ctrl in new Control[] { btnInputFile, btnInputFolder, btnOutputFile, btnOutputFolder,
                btnTaggerFile, btnLemmatizerFile, btnTag, chkIncludeSubfolders, chkOverwriteFiles })
            {
                ctrl.Enabled = false;
            }
            foreach (TextBox ctrl in new TextBox[] { txtInput, txtOutput, txtTaggerFile, txtLemmatizerFile })
            {
                ctrl.ReadOnly = true;
                ctrl.BackColor = SystemColors.Control;
            }
        }

        private void EnableForm()
        {
            progressBar.Visible = false;
            btnTag.Enabled = true;
            btnTag.Focus();
            btnCancel.Enabled = false;
            foreach (Control ctrl in new Control[] { btnInputFile, btnInputFolder, btnOutputFile, btnOutputFolder, 
                btnTaggerFile, btnLemmatizerFile, chkIncludeSubfolders, chkOverwriteFiles })
            {
                ctrl.Enabled = true;
            }
            foreach (TextBox ctrl in new TextBox[] { txtInput, txtOutput, txtTaggerFile, txtLemmatizerFile })
            {
                ctrl.ReadOnly = false;
                ctrl.BackColor = Color.FromKnownColor(KnownColor.White);
            }
        }

        private void PosTaggerTagForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (mThread.IsAlive) { mLogger.Info(null, "Počakajte ..."); }
                mLogger.LocalLevel = Logger.Level.Off;
                ThreadHandler.Abort(mThread, /*timeoutMs=*/3000);
            }
            catch { }
        }

        private void btnTag_Click(object sender, EventArgs e)
        {
            ThreadHandler.Reset();
            // save current configuration
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("input");
            config.AppSettings.Settings.Add("input", txtInput.Text);
            config.AppSettings.Settings.Remove("includeSubfolders");
            config.AppSettings.Settings.Add("includeSubfolders", chkIncludeSubfolders.Checked ? "true" : "false");
            config.AppSettings.Settings.Remove("overwriteFiles");
            config.AppSettings.Settings.Add("overwriteFiles", chkOverwriteFiles.Checked ? "true" : "false");
            config.AppSettings.Settings.Remove("output");
            config.AppSettings.Settings.Add("output", txtOutput.Text);
            config.AppSettings.Settings.Remove("taggerFile");
            config.AppSettings.Settings.Add("taggerFile", txtTaggerFile.Text);
            config.AppSettings.Settings.Remove("lemmatizerFile");
            config.AppSettings.Settings.Add("lemmatizerFile", txtLemmatizerFile.Text);
            config.Save(ConfigurationSaveMode.Modified);
            // prepare form
            DisableForm();
            txtStatus.Clear();
            // invoke tagger
            mThread = new Thread(new ThreadStart(delegate()
            {
                ArrayList<string> settings = new ArrayList<string>(new string[]{ "-v", "-t" });
                if (chkOverwriteFiles.Checked) { settings.Add("-o"); }
                if (chkIncludeSubfolders.Checked) { settings.Add("-s"); }
                if (txtLemmatizerFile.Text.Trim() != "") { settings.Add("-lem:" + txtLemmatizerFile.Text); }
                settings.AddRange(new string[] { txtInput.Text, txtTaggerFile.Text, txtOutput.Text });
                PosTaggerTag.Tag(settings.ToArray());
                GC.Collect(); // this closes all open files by invoking finalizers on readers and writers
                Invoke(new ThreadStart(delegate() { EnableForm(); }));
            }));
            mThread.Start();
        }

        private string Locate(string path)
        {
            try { path = new DirectoryInfo(path).FullName; }
            catch { return null; }
            path = path.TrimEnd('\\');
            string[] parts = path.Split('\\');
            int tailLen = 0;
            for (int i = parts.Length - 1; i >= 0; i--)
            {
                string folder = path.Substring(0, path.Length - tailLen) + "\\";
                if (Utils.VerifyFolderName(folder, /*mustExist=*/true)) { return folder; }
                tailLen += parts[i].Length + 1;
            }
            return null;
        }

        private void btnInputFolder_Click(object sender, EventArgs e)
        {
            string folder = Locate(txtInput.Text);
            if (folder != null) { dlgInputFolder.SelectedPath = folder; }
            if (dlgInputFolder.ShowDialog() == DialogResult.OK)
            {
                folder = dlgInputFolder.SelectedPath;
                if (!folder.EndsWith("\\")) { folder += "\\"; }
                txtInput.Text = folder + "*.*";
            }
        }

        private void btnInputFile_Click(object sender, EventArgs e)
        {
            string folder = Locate(txtInput.Text);
            if (folder != null) { dlgInputFile.InitialDirectory = folder; }
            if (dlgInputFile.ShowDialog() == DialogResult.OK)
            {
                txtInput.Text = dlgInputFile.FileName;
            }
        }

        private void btnOutputFolder_Click(object sender, EventArgs e)
        {
            string folder = Locate(txtOutput.Text);
            if (folder != null) { dlgOutputFolder.SelectedPath = folder; }
            if (dlgOutputFolder.ShowDialog() == DialogResult.OK)
            {
                txtOutput.Text = dlgOutputFolder.SelectedPath;
            }
        }

        private void btnOutputFile_Click(object sender, EventArgs e)
        {
            string folder = Locate(txtOutput.Text);
            if (folder != null) { dlgOutputFile.InitialDirectory = folder; }
            if (dlgOutputFile.ShowDialog() == DialogResult.OK)
            { 
                txtOutput.Text = dlgOutputFile.FileName;
            }
        }

        private void btnTaggerFile_Click(object sender, EventArgs e)
        {
            string folder = Locate(txtTaggerFile.Text);
            if (folder != null) { dlgTaggerFile.InitialDirectory = folder; }
            if (dlgTaggerFile.ShowDialog() == DialogResult.OK)
            {
                txtTaggerFile.Text = dlgTaggerFile.FileName;
            }
        }

        private void btnLemmatizerFile_Click(object sender, EventArgs e)
        {
            string folder = Locate(txtLemmatizerFile.Text);
            if (folder != null) { dlgLemmatizerFile.InitialDirectory = folder; }
            if (dlgLemmatizerFile.ShowDialog() == DialogResult.OK)
            {
                txtLemmatizerFile.Text = dlgLemmatizerFile.FileName;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            btnCancel.Enabled = false;
            if (mThread.IsAlive) { mLogger.Info(null, "Počakajte ..."); } 
            mLogger.LocalLevel = Logger.Level.Off;
            try 
            {
                ThreadHandler.Abort(mThread, /*timeoutMs=*/3000);
                while (mThread.IsAlive) { Thread.Sleep(100); }
            } 
            catch { }
            GC.Collect(); // this closes all open files by invoking finalizers on readers and writers
            EnableForm();
            mLogger.LocalLevel = Logger.Level.Debug;
            mLogger.Info(null, "Označevanje prekinjeno.");
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}