using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Application = System.Windows.Forms.Application;
using BrightIdeasSoftware;
using Title = NX_Game_Info.Common.Title;

#pragma warning disable IDE1006 // Naming rule violation: These words must begin with upper case characters

namespace NX_Game_Info
{
    public partial class Main : Form
    {
        internal IProgressDialog progressDialog;

        public Main()
        {
            InitializeComponent();

            debugLogToolStripMenuItem.Checked = Properties.Settings.Default.DebugLog;

            aboutToolStripMenuItem.Text = String.Format("&About {0}", Application.ProductName);

            bool init = Process.initialize(out List<string> messages);

            foreach (var message in messages)
            {
                MessageBox.Show(message, Application.ProductName);
            }

            if (!init)
            {
                Environment.Exit(-1);
            }
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (backgroundWorkerProcess.IsBusy)
            {
                MessageBox.Show("Please wait until the current process is finished and try again.", Application.ProductName);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open NX Game Files";
            openFileDialog.Filter = "NX Game Files (*.xci;*.nsp)|*.xci;*.nsp|Gamecard Files (*.xci)|*.xci|Package Files (*.nsp)|*.nsp|All Files (*.*)|*.*";
            openFileDialog.Multiselect = true;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.InitialDirectory = Properties.Settings.Default.InitialDirectory;

            Process.log?.WriteLine("\nOpen File");

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                objectListView.Items.Clear();
                toolStripStatusLabel.Text = "";

                List<string> filenames = openFileDialog.FileNames.ToList();
                filenames.Sort();

                Properties.Settings.Default.InitialDirectory = Path.GetDirectoryName(filenames.First());
                Properties.Settings.Default.Save();

                Process.log?.WriteLine("{0} files selected", filenames.Count);

                progressDialog = (IProgressDialog)new ProgressDialog();
                progressDialog.StartProgressDialog(Handle, String.Format("Opening {0} files", filenames.Count));

                backgroundWorkerProcess.RunWorkerAsync(filenames);
            }
        }

        private void openDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (backgroundWorkerProcess.IsBusy)
            {
                MessageBox.Show("Please wait until the current process is finished and try again.", Application.ProductName);
                return;
            }

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = Properties.Settings.Default.InitialDirectory;

            Process.log?.WriteLine("\nOpen Directory");

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                objectListView.Items.Clear();
                toolStripStatusLabel.Text = "";

                List<string> filenames = Directory.EnumerateFiles(folderBrowserDialog.SelectedPath, "*.*", SearchOption.AllDirectories)
                    .Where(filename => filename.ToLower().EndsWith(".xci") || filename.ToLower().EndsWith(".nsp")).ToList();
                filenames.Sort();

                Properties.Settings.Default.InitialDirectory = folderBrowserDialog.SelectedPath;
                Properties.Settings.Default.Save();

                Process.log?.WriteLine("{0} files selected", filenames.Count);

                progressDialog = (IProgressDialog)new ProgressDialog();
                progressDialog.StartProgressDialog(Handle, String.Format("Opening {0} files from directory {1}", filenames.Count, folderBrowserDialog.SelectedPath));

                backgroundWorkerProcess.RunWorkerAsync(filenames);
            }
        }

        private void openSDCardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Process.keyset?.SdSeed?.All(b => b == 0) ?? true)
            {
                string error = "sd_seed is missing from Console Keys";
                Process.log?.WriteLine(error);

                MessageBox.Show(String.Format("{0}.\nOpen SD Card will not be available.", error), Application.ProductName);
                return;
            }

            if ((Process.keyset?.SdCardKekSource?.All(b => b == 0) ?? true) || (Process.keyset?.SdCardKeySources?[1]?.All(b => b == 0) ?? true))
            {
                Process.log?.WriteLine("Keyfile missing required keys");
                Process.log?.WriteLine(" - {0} ({1}exists)", "sd_card_kek_source", (bool)Process.keyset?.SdCardKekSource?.Any(b => b != 0) ? "" : "not ");
                Process.log?.WriteLine(" - {0} ({1}exists)", "sd_card_nca_key_source", (bool)Process.keyset?.SdCardKeySources?[1]?.Any(b => b != 0) ? "" : "not ");

                MessageBox.Show("sd_card_kek_source and sd_card_nca_key_source are missing from Keyfile.\nOpen SD Card will not be available.", Application.ProductName);
                return;
            }

            if (backgroundWorkerProcess.IsBusy)
            {
                MessageBox.Show("Please wait until the current process is finished and try again.", Application.ProductName);
                return;
            }

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = String.IsNullOrEmpty(Properties.Settings.Default.SDCardDirectory) ? Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()) : Properties.Settings.Default.SDCardDirectory;

            Process.log?.WriteLine("\nOpen SD Card");

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                objectListView.Items.Clear();
                toolStripStatusLabel.Text = "";

                Properties.Settings.Default.SDCardDirectory = folderBrowserDialog.SelectedPath;
                Properties.Settings.Default.Save();

                Process.log?.WriteLine("SD card selected");

                progressDialog = (IProgressDialog)new ProgressDialog();
                progressDialog.StartProgressDialog(Handle, "Opening SD card on " + folderBrowserDialog.SelectedPath);

                backgroundWorkerProcess.RunWorkerAsync(folderBrowserDialog.SelectedPath);
            }
        }

        private void debugLogToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.DebugLog = debugLogToolStripMenuItem.Checked;
            Properties.Settings.Default.Save();

            if (Properties.Settings.Default.DebugLog)
            {
                try
                {
                    Process.log = File.AppendText(Process.path_prefix + Common.LOG_FILE);
                    Process.log.AutoFlush = true;
                }
                catch { }
            }
            else
            {
                Process.log?.Close();
                Process.log = null;
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(-1);
        }

        private void backgroundWorkerProcess_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            List<Title> titles = new List<Title>();

            if (e.Argument is List<string> filenames)
            {
                int count = filenames.Count, index = 0;

                foreach (var filename in filenames)
                {
                    if (worker.CancellationPending) break;

                    worker.ReportProgress(100 * index++ / count, filename);

                    Title title = Process.processFile(filename);
                    if (title != null)
                    {
                        titles.Add(title);
                    }
                }

                if (!worker.CancellationPending)
                {
                    worker.ReportProgress(100, "");
                }

                Process.log?.WriteLine("\n{0} titles processed", titles.Count);
            }
            else if (e.Argument is string sdpath)
            {
                List<LibHac.Title> sdtitles = Process.processSd(sdpath);
                int count = sdtitles.Count, index = 0;

                foreach (var sdtitle in sdtitles)
                {
                    if (worker.CancellationPending) break;

                    worker.ReportProgress(100 * index++ / count, sdtitle.MainNca?.Filename);

                    Title title = Process.processTitle(sdtitle);
                    if (title != null)
                    {
                        titles.Add(title);
                    }
                }

                if (!worker.CancellationPending)
                {
                    worker.ReportProgress(100, "");
                }

                Process.log?.WriteLine("\n{0} titles processed", titles.Count);
            }

            e.Result = titles;
        }

        private void backgroundWorkerProcess_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (progressDialog.HasUserCancelled())
            {
                backgroundWorkerProcess.CancelAsync();
            }

            progressDialog.SetLine(2, e.UserState as string, true, IntPtr.Zero);
            progressDialog.SetProgress((uint)e.ProgressPercentage, 100);
        }

        private void backgroundWorkerProcess_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            List<Title> titles = (List<Title>)e.Result;

            objectListView.SetObjects(titles);

            foreach (OLVListItem listItem in objectListView.Items)
            {
                Title title = listItem.RowObject as Title;

                if (title.signature != true)
                {
                    listItem.BackColor = Color.WhiteSmoke;
                }

                if (title.permission == Title.Permission.Dangerous)
                {
                    listItem.ForeColor = Color.DarkRed;
                }
                else if (title.permission == Title.Permission.Unsafe)
                {
                    listItem.ForeColor = Color.Indigo;
                }
            }

            toolStripStatusLabel.Text = titles.Count + " files";

            progressDialog.StopProgressDialog();
            Activate();
        }
    }

    // IProgressDialog Credits to Alex J https://stackoverflow.com/a/37393363
    [Flags]
    public enum IPD_Flags : uint
    {
        Normal = 0x00000000,
        Modal = 0x00000001,
        AutoTime = 0x00000002,
        NoTime = 0x00000004,
        NoMinimize = 0x00000008,
        NoProgressBar = 0x00000010,
    }

    [Flags]
    public enum IPDTIMER_Flags : uint
    {
        Reset = 0x00000001,
        Pause = 0x00000002,
        Resume = 0x00000003,
    }

    [ComImport]
    [Guid("F8383852-FCD3-11d1-A6B9-006097DF5BD4")]
    internal class ProgressDialog
    {

    }

    [ComImport]
    [Guid("EBBC7C04-315E-11d2-B62F-006097DF5BD4")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IProgressDialog
    {
        [PreserveSig]
        void StartProgressDialog(IntPtr hwndParent
        , [MarshalAs(UnmanagedType.IUnknown)] object punkEnableModless
        , uint dwFlags
        , IntPtr pvResevered);

        [PreserveSig]
        void StopProgressDialog();

        [PreserveSig]
        void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pwzTitle);

        [PreserveSig]
        void SetAnimation(IntPtr hInstAnimation, ushort idAnimation);

        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Bool)]
        bool HasUserCancelled();

        [PreserveSig]
        void SetProgress(uint dwCompleted, uint dwTotal);

        [PreserveSig]
        void SetProgress64(ulong ullCompleted, ulong ullTotal);

        [PreserveSig]
        void SetLine(uint dwLineNum
            , [MarshalAs(UnmanagedType.LPWStr)] string pwzString
            , [MarshalAs(UnmanagedType.VariantBool)] bool fCompactPath
            , IntPtr pvResevered);

        [PreserveSig]
        void SetCancelMsg([MarshalAs(UnmanagedType.LPWStr)]string pwzCancelMsg, object pvResevered);

        [PreserveSig]
        void Timer(uint dwTimerAction, object pvResevered);
    }

    public static class ProgressDialogExtension
    {
        internal static void StartProgressDialog(this IProgressDialog progressDialog, IntPtr hwndParent, string pwzString)
        {
            progressDialog.SetTitle(Application.ProductName);
            progressDialog.SetCancelMsg("Please wait until the current process is finished", IntPtr.Zero);
            progressDialog.SetLine(1, pwzString, false, IntPtr.Zero);

            progressDialog.StartProgressDialog(hwndParent, null, (uint)(IPD_Flags.Modal | IPD_Flags.AutoTime | IPD_Flags.NoMinimize), IntPtr.Zero);
            progressDialog.SetProgress(0, 100);
        }
    }
}
