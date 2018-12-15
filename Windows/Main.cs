using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Application = System.Windows.Forms.Application;
using BrightIdeasSoftware;
using Title = NX_Game_Info.Common.Title;
using System.ComponentModel;

namespace NX_Game_Info
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();

            List<string> messages;
            bool init = Process.initialize(out messages);

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

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                objectListView.Items.Clear();

                List<string> filenames = openFileDialog.FileNames.ToList();
                filenames.Sort();

                Properties.Settings.Default.InitialDirectory = Path.GetDirectoryName(filenames.First());
                Properties.Settings.Default.Save();

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

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                objectListView.Items.Clear();

                List<string> filenames = Directory.EnumerateFiles(folderBrowserDialog.SelectedPath, "*.*", SearchOption.AllDirectories)
                    .Where(filename => filename.ToLower().EndsWith(".xci") || filename.ToLower().EndsWith(".nsp")).ToList();
                filenames.Sort();

                Properties.Settings.Default.InitialDirectory = folderBrowserDialog.SelectedPath;
                Properties.Settings.Default.Save();

                backgroundWorkerProcess.RunWorkerAsync(filenames);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(-1);
        }

        private void backgroundWorkerProcess_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            List<string> filenames = (List<string>)e.Argument;
            List<Title> titles = new List<Title>();

            int count = filenames.Count, index = 0;

            foreach (var filename in filenames)
            {
                Title title = Process.processFile(filename);
                if (title != null)
                {
                    titles.Add(title);
                }

                worker.ReportProgress(++index / count * 100);
            }

            e.Result = titles;
        }

        private void backgroundWorkerProcess_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

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
        }
    }
}
