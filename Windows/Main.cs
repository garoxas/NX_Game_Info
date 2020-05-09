using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Application = System.Windows.Forms.Application;
using Bluegrams.Application;
using BrightIdeasSoftware;
using KPreisser.UI;
using LibHac;
using OfficeOpenXml;
using FsTitle = LibHac.Title;
using Title = NX_Game_Info.Common.Title;
using ArrayOfTitle = NX_Game_Info.Common.ArrayOfTitle;

#pragma warning disable IDE1006 // Naming rule violation: These words must begin with upper case characters

namespace NX_Game_Info
{
    public partial class Main : Form
    {
        internal AboutBox aboutBox;
        internal RenameBox renameBox;
        internal IProgressDialog progressDialog;

        public enum Worker
        {
            File,
            Directory,
            SDCard,
            Invalid = -1
        }

        private class CustomSorter : IComparer, IComparer<OLVListItem>
        {
            readonly OLVColumn column;
            readonly SortOrder order;

            public CustomSorter(OLVColumn column, SortOrder order)
            {
                this.column = column;
                this.order = order;
            }

            public int Compare(object x, object y)
            {
                if (x is OLVListItem && y is OLVListItem)
                    return Compare(x as OLVListItem, y as OLVListItem);
                return 0;
            }

            public int Compare(OLVListItem x, OLVListItem y)
            {
                switch (column.AspectName)
                {
                    case "versionString":
                        return ((Title)x.RowObject).version.CompareTo(((Title)y.RowObject).version) * (order == SortOrder.Ascending ? 1 : -1);
                    case "latestVersionString":
                        return ((Title)x.RowObject).latestVersion.CompareTo(((Title)y.RowObject).latestVersion) * (order == SortOrder.Ascending ? 1 : -1);
                    case "applicationVersionString":
                        return ((Title)x.RowObject).applicationVersion.CompareTo(((Title)y.RowObject).applicationVersion) * (order == SortOrder.Ascending ? 1 : -1);
                    case "masterkeyString":
                        return ((Title)x.RowObject).masterkey.CompareTo(((Title)y.RowObject).masterkey) * (order == SortOrder.Ascending ? 1 : -1);
                    case "filesizeString":
                        return ((Title)x.RowObject).filesize.CompareTo(((Title)y.RowObject).filesize) * (order == SortOrder.Ascending ? 1 : -1);
                    case "displayVersion":
                        if (!Version.TryParse(((Title)x.RowObject).displayVersion, out Version verx))
                        {
                            verx = new Version();
                        }
                        if (!Version.TryParse(((Title)y.RowObject).displayVersion, out Version very))
                        {
                            very = new Version();
                        }
                        return verx.CompareTo(very) * (order == SortOrder.Ascending ? 1 : -1);
                }

                return 0;
            }
        }

        internal List<Title> titles = new List<Title>();

        public Main()
        {
            InitializeComponent();

            PortableSettingsProvider.SettingsFileName = Common.USER_SETTINGS;
            PortableSettingsProviderBase.SettingsDirectory = Process.path_prefix;
            PortableSettingsProvider.ApplyProvider(Common.Settings.Default, Common.History.Default, Common.RecentDirectories.Default);

            Common.Settings.Default.Upgrade();
            Common.History.Default.Upgrade();
            Common.RecentDirectories.Default.Upgrade();

            debugLogToolStripMenuItem.Checked = Common.Settings.Default.DebugLog;

            aboutToolStripMenuItem.Text = String.Format("&About {0}", Application.ProductName);

            int index = 0;
            foreach (string property in Title.Properties)
            {
                ToolStripMenuItem menuItem = new ToolStripMenuItem
                {
                    Name = String.Format("property{0}ToolStripMenuItem", index++),
                    Text = property,
                };
                menuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
                copyToolStripMenuItem.DropDownItems.Add(menuItem);
            }

            bool init = Process.initialize(out List<string> messages);

            foreach (var message in messages)
            {
                MessageBox.Show(message, Application.ProductName);
            }

            if (!init)
            {
                Environment.Exit(-1);
            }

            Process.migrateSettings();
        }

        public void reloadData()
        {
            uint index = 0, count = (uint)titles.Count;

            objectListView.SetObjects(titles);

            foreach (OLVListItem listItem in objectListView.Items)
            {
                Title title = listItem.RowObject as Title;

                progressDialog?.SetLine(2, title.titleName, true, IntPtr.Zero);
                progressDialog?.SetProgress(index++, count);

                string titleID = title.type == TitleType.AddOnContent ? title.titleID : title.baseTitleID ?? "";

                Process.latestVersions.TryGetValue(titleID, out uint latestVersion);
                Process.versionList.TryGetValue(titleID, out uint version);
                Process.titleVersions.TryGetValue(titleID, out uint titleVersion);

                if (latestVersion < version || latestVersion < titleVersion)
                {
                    listItem.BackColor = title.signature != true ? Color.OldLace : Color.LightYellow;
                }
                else if (title.signature != true)
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

        public void saveWindowState()
        {
            if (WindowState == FormWindowState.Normal)
            {
                Common.Settings.Default.WindowLocation = Location;
                Common.Settings.Default.WindowSize = Size;
            }
            else
            {
                Common.Settings.Default.WindowLocation = RestoreBounds.Location;
                Common.Settings.Default.WindowSize = RestoreBounds.Size;
            }

            Common.Settings.Default.Maximized = (WindowState == FormWindowState.Maximized ? true : false);

            Common.Settings.Default.Columns = objectListView.ColumnsInDisplayOrder.Select(x => x.AspectName).ToList();

            List<int> columnWidth = new List<int>();
            foreach (ColumnHeader column in objectListView.Columns)
            {
                columnWidth.Add(column.Width);
            }
            Common.Settings.Default.ColumnWidth = columnWidth;

            Common.Settings.Default.Save();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Location = Common.Settings.Default.WindowLocation;
            Size = Common.Settings.Default.WindowSize;

            if (Common.Settings.Default.Maximized)
            {
                WindowState = FormWindowState.Maximized;
            }

            if (Common.Settings.Default.Columns.Any())
            {
                foreach (var column in objectListView.AllColumns)
                {
                    if (!Common.Settings.Default.Columns.Union(new List<string> { "titleID", "titleName", "error" }).ToList().Contains(column.AspectName))
                    {
                        column.IsVisible = false;
                    }
                }
                objectListView.RebuildColumns();
            }

            foreach (var column in objectListView.Columns.Cast<ColumnHeader>().Zip(Common.Settings.Default.ColumnWidth, Tuple.Create))
            {
                column.Item1.Width = column.Item2;
            }

            objectListView.CustomSorter = delegate (OLVColumn column, SortOrder order)
            {
                switch (column.AspectName)
                {
                    case "versionString":
                    case "latestVersionString":
                    case "applicationVersionString":
                    case "filesizeString":
                        objectListView.ListViewItemSorter = new CustomSorter(column, order);
                        break;
                    case "displayVersion":
                        objectListView.ListViewItemSorter = new CustomSorter(column, order);
                        break;
                    case "titleID":
                    case "baseTitleID":
                    case "titleName":
                    case "systemUpdateString":
                    case "systemVersionString":
                    case "masterkeyString":
                    case "titleKey":
                    case "publisher":
                    case "languagesString":
                    case "filename":
                    case "typeString":
                    case "distribution":
                    case "structureString":
                    case "signatureString":
                    case "permissionString":
                    case "error":
                    default:
                        objectListView.ListViewItemSorter = new ColumnComparer(column, order);
                        break;
                }
            };

            int index = 0;
            foreach (ArrayOfTitle history in Common.History.Default.Titles)
            {
                ToolStripMenuItem menuItem = new ToolStripMenuItem
                {
                    Name = String.Format("history{0}ToolStripMenuItem", index++),
                    Text = String.Format("{0} ({1} files)", history.description, history.title.Count),
                    CheckOnClick = true,
                };
                menuItem.Click += new System.EventHandler(this.historyToolStripMenuItem_Click);
                historyToolStripMenuItem.DropDownItems.Add(menuItem);
            }

            if (index > 0)
                (historyToolStripMenuItem.DropDownItems[index - 1] as ToolStripMenuItem).Checked = true;

            titles = Process.processHistory();

            index = 0;
            foreach (ArrayOfTitle directories in Common.RecentDirectories.Default.Titles)
            {
                ToolStripMenuItemRemove menuItem = new ToolStripMenuItemRemove
                {
                    Name = directories.description,
                    Text = directories.description,
                };
                menuItem.Click += new System.EventHandler(this.openRecentDirectoryToolStripMenuItem_Click);
                openDirectoryToolStripMenuItem.DropDownItems.Add(menuItem);
            }

            titles = Process.processHistory();

            reloadData();

            toolStripStatusLabel.Text = String.Format("{0} files", titles.Count);
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            saveWindowState();
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
            openFileDialog.Filter = String.Format("NX Game Files (*.xci;*.nsp;{0}*.nro)|*.xci;*.nsp;{0}*.nro|Gamecard Files (*.xci{1})|*.xci{1}|Package Files (*.nsp{2})|*.nsp{2}|Homebrew Files (*.nro)|*.nro|All Files (*.*)|*.*",
                Common.Settings.Default.NszExtension ? "*.xcz;*.nsz;" : "", Common.Settings.Default.NszExtension ? ";*.xcz" : "", Common.Settings.Default.NszExtension ? ";*.nsz" : "");
            openFileDialog.Multiselect = true;
            openFileDialog.InitialDirectory = !String.IsNullOrEmpty(Common.Settings.Default.InitialDirectory) && Directory.Exists(Common.Settings.Default.InitialDirectory) ? Common.Settings.Default.InitialDirectory : Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());

            Process.log?.WriteLine("\nOpen File");

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                objectListView.Items.Clear();
                toolStripStatusLabel.Text = "";

                Common.Settings.Default.InitialDirectory = Path.GetDirectoryName(openFileDialog.FileNames.First());
                Common.Settings.Default.Save();

                progressDialog = (IProgressDialog)new ProgressDialog();
                progressDialog.StartProgressDialog(Handle, "Opening files");

                backgroundWorkerProcess.RunWorkerAsync((Worker.File, openFileDialog.FileNames));
            }
        }
        public class ToolStripMenuItemRemove : ToolStripMenuItem
        {
            public ToolStripMenuItemRemove() { }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                Rectangle rect = new Rectangle(this.Width - 20, 3, 16, 16);
                e.Graphics.DrawImage(Properties.Resources.Delete, rect);
            }

            public bool DeleteClicked { get; set; }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                DeleteClicked = e.X > (this.Width - 20);
            }
        }

        private void openDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (backgroundWorkerProcess.IsBusy)
            {
                MessageBox.Show("Please wait until the current process is finished and try again.", Application.ProductName);
                return;
            }

            fileToolStripMenuItem.HideDropDown();

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = !String.IsNullOrEmpty(Common.Settings.Default.InitialDirectory) && Directory.Exists(Common.Settings.Default.InitialDirectory) ? Common.Settings.Default.InitialDirectory : Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());

            Process.log?.WriteLine("\nOpen Directory");

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                ArrayOfTitle findtitle = Common.RecentDirectories.Default.Titles.Find(x => x.description == folderBrowserDialog.SelectedPath);
                Common.RecentDirectories.Default.Titles.Remove(findtitle);
                Common.RecentDirectories.Default.Titles.Insert(0, new ArrayOfTitle { description = folderBrowserDialog.SelectedPath });

                if (Common.RecentDirectories.Default.Titles.Count > Common.HISTORY_SIZE)
                {
                    Common.RecentDirectories.Default.Titles.RemoveRange(Common.HISTORY_SIZE, Common.RecentDirectories.Default.Titles.Count - Common.HISTORY_SIZE);
                }
                Common.RecentDirectories.Default.Save();

                ToolStripMenuItemRemove menuItem = new ToolStripMenuItemRemove
                {
                    Name = folderBrowserDialog.SelectedPath,
                    Text = folderBrowserDialog.SelectedPath,
                };
                menuItem.Click += new System.EventHandler(this.openRecentDirectoryToolStripMenuItem_Click);

                ToolStripItem[] finditem = openDirectoryToolStripMenuItem.DropDownItems.Find(folderBrowserDialog.SelectedPath, false);
                if (finditem.Length > 0)
                {
                    openDirectoryToolStripMenuItem.DropDownItems.Remove(finditem[0]);
                }

                openDirectoryToolStripMenuItem.DropDownItems.Insert(0, menuItem);

                foreach (ToolStripMenuItem item in openDirectoryToolStripMenuItem.DropDownItems)
                {
                    item.Checked = item == menuItem;
                }

                while (openDirectoryToolStripMenuItem.DropDownItems.Count > Common.HISTORY_SIZE)
                {
                    openDirectoryToolStripMenuItem.DropDownItems.RemoveAt(Common.HISTORY_SIZE);
                }

                objectListView.Items.Clear();
                toolStripStatusLabel.Text = "";

                Common.Settings.Default.InitialDirectory = folderBrowserDialog.SelectedPath;
                Common.Settings.Default.Save();

                progressDialog = (IProgressDialog)new ProgressDialog();
                progressDialog.StartProgressDialog(Handle, String.Format("Opening files from directory {0}", folderBrowserDialog.SelectedPath));

                backgroundWorkerProcess.RunWorkerAsync((Worker.Directory, folderBrowserDialog.SelectedPath));
            }
        }

        private void openRecentDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (backgroundWorkerProcess.IsBusy)
            {
                MessageBox.Show("Please wait until the current process is finished and try again.", Application.ProductName);
                return;
            }

            string directorypath = ((ToolStripMenuItem)sender).Text;
            ArrayOfTitle findtitle = Common.RecentDirectories.Default.Titles.Find(x => x.description == directorypath);
            ToolStripItem[] finditem = openDirectoryToolStripMenuItem.DropDownItems.Find(directorypath, false);

            if (((ToolStripMenuItemRemove)sender).DeleteClicked)
            {
                Common.RecentDirectories.Default.Titles.Remove(findtitle);
                Common.RecentDirectories.Default.Save();
                openDirectoryToolStripMenuItem.DropDownItems.Remove(finditem[0]);
            }
            else
            {
                if (Directory.Exists(directorypath)){
                    Common.RecentDirectories.Default.Titles.Remove(findtitle);
                    Common.RecentDirectories.Default.Titles.Insert(0, findtitle);
                    Common.RecentDirectories.Default.Save();

                    if (finditem.Length > 0)
                    {
                        openDirectoryToolStripMenuItem.DropDownItems.Remove(finditem[0]);
                        openDirectoryToolStripMenuItem.DropDownItems.Insert(0, finditem[0]);
                    }

                    foreach (ToolStripMenuItem item in openDirectoryToolStripMenuItem.DropDownItems)
                    {
                        item.Checked = item == finditem[0];
                    }

                    Process.log?.WriteLine("\nOpen Recent Directory");

                    objectListView.Items.Clear();
                    toolStripStatusLabel.Text = "";

                    Common.Settings.Default.InitialDirectory = directorypath;
                    Common.Settings.Default.Save();

                    progressDialog = (IProgressDialog)new ProgressDialog();
                    progressDialog.StartProgressDialog(Handle, String.Format("Opening files from directory {0}", directorypath));

                    backgroundWorkerProcess.RunWorkerAsync((Worker.Directory, directorypath));
                }
                else
                {
                    DialogResult dialogResult = MessageBox.Show("Directory \"" + directorypath + "\" doesn't exist, remove from recents ?", Application.ProductName, MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        Common.RecentDirectories.Default.Titles.Remove(findtitle);
                        Common.RecentDirectories.Default.Save();
                        openDirectoryToolStripMenuItem.DropDownItems.Remove(finditem[0]);
                    }
                }
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
            folderBrowserDialog.SelectedPath = !String.IsNullOrEmpty(Common.Settings.Default.SDCardDirectory) && Directory.Exists(Common.Settings.Default.SDCardDirectory) ? Common.Settings.Default.SDCardDirectory : Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());

            Process.log?.WriteLine("\nOpen SD Card");

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                objectListView.Items.Clear();
                toolStripStatusLabel.Text = "";

                Common.Settings.Default.SDCardDirectory = folderBrowserDialog.SelectedPath;
                Common.Settings.Default.Save();

                Process.log?.WriteLine("SD card selected");

                progressDialog = (IProgressDialog)new ProgressDialog();
                progressDialog.StartProgressDialog(Handle, String.Format("Opening SD card on {0}", folderBrowserDialog.SelectedPath));

                backgroundWorkerProcess.RunWorkerAsync((Worker.SDCard, folderBrowserDialog.SelectedPath));
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Export Titles";
            saveFileDialog.Filter = "CSV File (*.csv)|*.csv|Excel Workbook (*.xlsx)|*.xlsx";

            Process.log?.WriteLine("\nExport Titles");

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filename = saveFileDialog.FileName;

                if (filename.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    using (var writer = new StreamWriter(filename))
                    {
                        progressDialog = (IProgressDialog)new ProgressDialog();
                        progressDialog.StartProgressDialog(Handle, "Exporting titles");

                        char separator = Common.Settings.Default.CsvSeparator;
                        if (separator != '\0')
                        {
                            writer.WriteLine("sep={0}", separator);
                        }
                        else
                        {
                            separator = ',';
                        }

                        writer.WriteLine("# publisher {0} {1}", Application.ProductName, Application.ProductVersion);
                        writer.WriteLine("# updated {0}", String.Format("{0:F}", DateTime.Now));

                        writer.WriteLine(String.Join(separator.ToString(), Common.Title.Properties));

                        uint index = 0, count = (uint)titles.Count;

                        foreach (var title in titles)
                        {
                            if (progressDialog.HasUserCancelled())
                            {
                                break;
                            }

                            progressDialog.SetLine(2, title.titleName, true, IntPtr.Zero);
                            progressDialog.SetProgress(index++, count);

                            writer.WriteLine(String.Join(separator.ToString(), new string[] {
                                title.titleID.Quote(separator),
                                title.baseTitleID.Quote(separator),
                                title.titleName.Quote(separator),
                                title.displayVersion.Quote(separator),
                                title.versionString.Quote(separator),
                                title.latestVersionString.Quote(separator),
                                title.systemUpdateString.Quote(separator),
                                title.systemVersionString.Quote(separator),
                                title.applicationVersionString.Quote(separator),
                                title.masterkeyString.Quote(separator),
                                title.titleKey.Quote(separator),
                                title.publisher.Quote(separator),
                                title.languagesString.Quote(separator),
                                title.filename.Quote(separator),
                                title.filesizeString.Quote(separator),
                                title.typeString.Quote(separator),
                                title.distribution.ToString().Quote(separator),
                                title.structureString.Quote(separator),
                                title.signatureString.Quote(separator),
                                title.permissionString.Quote(separator),
                                title.error.Quote(separator),
                            }));
                        }

                        Process.log?.WriteLine("\n{0} of {1} titles exported", index, titles.Count);

                        progressDialog.StopProgressDialog();
                        Activate();

                        MessageBox.Show(String.Format("{0} of {1} titles exported", index, titles.Count), Application.ProductName);
                    }
                }
                else if (filename.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    using (ExcelPackage excel = new ExcelPackage())
                    {
                        progressDialog = (IProgressDialog)new ProgressDialog();
                        progressDialog.StartProgressDialog(Handle, "Exporting titles");

                        ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add(Common.History.Default.Titles.LastOrDefault().description ?? Application.ProductName);

                        worksheet.Cells[1, 1, 1, Title.Properties.Count()].LoadFromArrays(new List<string[]> { Title.Properties });
                        worksheet.Cells["1:1"].Style.Font.Bold = true;
                        worksheet.Cells["1:1"].Style.Font.Color.SetColor(Color.White);
                        worksheet.Cells["1:1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        worksheet.Cells["1:1"].Style.Fill.BackgroundColor.SetColor(Color.MidnightBlue);

                        uint index = 0, count = (uint)titles.Count;

                        foreach (var title in titles)
                        {
                            if (progressDialog.HasUserCancelled())
                            {
                                break;
                            }

                            progressDialog.SetLine(2, title.titleName, true, IntPtr.Zero);
                            progressDialog.SetProgress(index++, count);

                            var data = new List<string[]>
                            {
                                new string[] {
                                    title.titleID,
                                    title.baseTitleID,
                                    title.titleName,
                                    title.displayVersion,
                                    title.versionString,
                                    title.latestVersionString,
                                    title.systemUpdateString,
                                    title.systemVersionString,
                                    title.applicationVersionString,
                                    title.masterkeyString,
                                    title.titleKey,
                                    title.publisher,
                                    title.languagesString,
                                    title.filename,
                                    title.filesizeString,
                                    title.typeString,
                                    title.distribution.ToString(),
                                    title.structureString,
                                    title.signatureString,
                                    title.permissionString,
                                    title.error,
                                }
                            };

                            worksheet.Cells[(int)index + 1, 1].LoadFromArrays(data);

                            string titleID = title.type == TitleType.AddOnContent ? title.titleID : title.baseTitleID ?? "";

                            Process.latestVersions.TryGetValue(titleID, out uint latestVersion);
                            Process.versionList.TryGetValue(titleID, out uint version);
                            Process.titleVersions.TryGetValue(titleID, out uint titleVersion);

                            if (latestVersion < version || latestVersion < titleVersion)
                            {
                                worksheet.Cells[(int)index + 1, 1, (int)index + 1, Title.Properties.Count()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                worksheet.Cells[(int)index + 1, 1, (int)index + 1, Title.Properties.Count()].Style.Fill.BackgroundColor.SetColor(title.signature != true ? Color.OldLace : Color.LightYellow);
                            }
                            else if (title.signature != true)
                            {
                                worksheet.Cells[(int)index + 1, 1, (int)index + 1, Title.Properties.Count()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                worksheet.Cells[(int)index + 1, 1, (int)index + 1, Title.Properties.Count()].Style.Fill.BackgroundColor.SetColor(Color.WhiteSmoke);
                            }

                            if (title.permission == Title.Permission.Dangerous)
                            {
                                worksheet.Cells[(int)index + 1, 1, (int)index + 1, Title.Properties.Count()].Style.Font.Color.SetColor(Color.DarkRed);
                            }
                            else if (title.permission == Title.Permission.Unsafe)
                            {
                                worksheet.Cells[(int)index + 1, 1, (int)index + 1, Title.Properties.Count()].Style.Font.Color.SetColor(Color.Indigo);
                            }
                        }

                        ExcelRange range = worksheet.Cells[1, 1, (int)count + 1, Title.Properties.Count()];
                        range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                        worksheet.Column(1).Width = 18;
                        worksheet.Column(2).Width = 18;
                        worksheet.Column(3).AutoFit();
                        worksheet.Column(3).Width = Math.Max(worksheet.Column(3).Width, 30);
                        worksheet.Column(4).Width = 16;
                        worksheet.Column(5).Width = 16;
                        worksheet.Column(6).Width = 16;
                        worksheet.Column(7).Width = 16;
                        worksheet.Column(8).Width = 16;
                        worksheet.Column(9).Width = 16;
                        worksheet.Column(10).Width = 16;
                        worksheet.Column(11).AutoFit();
                        worksheet.Column(11).Width = Math.Max(worksheet.Column(11).Width, 36);
                        worksheet.Column(12).AutoFit();
                        worksheet.Column(12).Width = Math.Max(worksheet.Column(12).Width, 30);
                        worksheet.Column(13).Width = 18;
                        worksheet.Column(14).AutoFit();
                        worksheet.Column(14).Width = Math.Max(worksheet.Column(14).Width, 54);
                        worksheet.Column(15).Width = 10;
                        worksheet.Column(16).Width = 10;
                        worksheet.Column(17).Width = 12;
                        worksheet.Column(18).Width = 12;
                        worksheet.Column(19).Width = 10;
                        worksheet.Column(20).Width = 10;
                        worksheet.Column(21).Width = 40;

                        excel.SaveAs(new FileInfo(filename));

                        Process.log?.WriteLine("\n{0} of {1} titles exported", index, titles.Count);

                        progressDialog.StopProgressDialog();
                        Activate();

                        MessageBox.Show(String.Format("{0} of {1} titles exported", index, titles.Count), Application.ProductName);
                    }
                }
                else
                {
                    MessageBox.Show(String.Format("This file type is not supported {0}", Path.GetExtension(filename)), Application.ProductName);
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveWindowState();

            Environment.Exit(-1);
        }

        private void updateTitleKeysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            progressDialog = (IProgressDialog)new ProgressDialog();
            progressDialog.StartProgressDialog(Handle, "Downloading title keys");

            progressDialog.SetLine(2, String.Format("Downloading from {0}", Common.TITLE_KEYS_URI), true, IntPtr.Zero);

            int count = Process.keyset?.TitleKeys?.Count ?? 0;

            if (Process.updateTitleKeys())
            {
                Process.log?.WriteLine("\nFound {0} updated title keys", (Process.keyset?.TitleKeys?.Count ?? 0) - count);

                progressDialog.StopProgressDialog();
                Activate();

                MessageBox.Show(String.Format("Found {0} updated title keys", (Process.keyset?.TitleKeys?.Count ?? 0) - count), Application.ProductName);
            }
            else
            {
                progressDialog.StopProgressDialog();
                Activate();

                MessageBox.Show("Failed to download title keys", Application.ProductName);
            }
        }

        private void updateVersionListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            progressDialog = (IProgressDialog)new ProgressDialog();
            progressDialog.StartProgressDialog(Handle, "Downloading version list");

            progressDialog.SetLine(2, String.Format("Downloading from {0}", Common.HAC_VERSIONLIST_URI), true, IntPtr.Zero);

            if (Process.updateVersionList())
            {
                uint count = 0;

                foreach (var title in titles)
                {
                    if (title.type == TitleType.Application || title.type == TitleType.Patch)
                    {
                        if (Process.versionList.TryGetValue(title.baseTitleID, out uint version))
                        {
                            if (title.latestVersion == unchecked((uint)-1) || version > title.latestVersion)
                            {
                                title.latestVersion = version;
                                count++;
                            }
                        }
                    }
                }

                if (count != 0)
                {
                    reloadData();

                    ArrayOfTitle history = new ArrayOfTitle
                    {
                        description = DateTime.Now.ToString("dd MMMM yyyy HH:mm:ss"),
                        title = titles.ToList(),
                    };
                    Common.History.Default.Titles.Add(history);
                    if (Common.History.Default.Titles.Count > Common.HISTORY_SIZE)
                    {
                        Common.History.Default.Titles.RemoveRange(0, Common.History.Default.Titles.Count - Common.HISTORY_SIZE);
                    }
                    Common.History.Default.Save();

                    while (historyToolStripMenuItem.DropDownItems.Count > Common.HISTORY_SIZE)
                    {
                        historyToolStripMenuItem.DropDownItems.RemoveAt(0);
                    }
                }

                Process.log?.WriteLine("\n{0} titles have updated version", count);

                progressDialog.StopProgressDialog();
                Activate();

                MessageBox.Show(String.Format("{0} titles have updated version", count), Application.ProductName);
            }
            else
            {
                progressDialog.StopProgressDialog();
                Activate();

                MessageBox.Show("Failed to download version list", Application.ProductName);
            }
        }

        private void debugLogToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            Common.Settings.Default.DebugLog = debugLogToolStripMenuItem.Checked;
            Common.Settings.Default.Save();

            if (Common.Settings.Default.DebugLog)
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

        private void historyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.latestVersions.Clear();

            int index = 0;
            foreach (ToolStripMenuItem item in historyToolStripMenuItem.DropDownItems)
            {
                item.Checked = item == sender;

                if (item == sender)
                {
                    titles = Process.processHistory(index);

                    reloadData();

                    toolStripStatusLabel.Text = String.Format("{0} files", titles.Count);
                }

                index++;
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private void objectListView_CellRightClick(object sender, CellRightClickEventArgs e)
        {
            if (e.Model != null)
            {
                e.MenuStrip = contextMenuStrip;
                contextMenuStrip.Tag = e.Model as Title;
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (contextMenuStrip.Tag is Title title)
            {
                List<string> text = new List<string>();

                string property = (sender as ToolStripMenuItem).Text.Replace("&", "");
                bool allColumns = property == "All Columns";

                foreach (OLVListItem item in objectListView.SelectedItems)
                {
                    title = (Title)item.RowObject;

                    if (allColumns || property == "Title ID")
                    {
                        text.Add(title.titleID);
                    }
                    if (allColumns || property == "Base Title ID")
                    {
                        text.Add(title.baseTitleID);
                    }
                    if (allColumns || property == "Title Name")
                    {
                        text.Add(title.titleName);
                    }
                    if (allColumns || property == "Display Version")
                    {
                        text.Add(title.displayVersion);
                    }
                    if (allColumns || property == "Version")
                    {
                        text.Add(title.versionString);
                    }
                    if (allColumns || property == "Latest Version")
                    {
                        text.Add(title.latestVersionString);
                    }
                    if (allColumns || property == "System Update")
                    {
                        text.Add(title.systemUpdateString);
                    }
                    if (allColumns || property == "System Version")
                    {
                        text.Add(title.systemVersionString);
                    }
                    if (allColumns || property == "Application Version")
                    {
                        text.Add(title.applicationVersionString);
                    }
                    if (allColumns || property == "Masterkey")
                    {
                        text.Add(title.masterkeyString);
                    }
                    if (allColumns || property == "Title Key")
                    {
                        text.Add(title.titleKey);
                    }
                    if (allColumns || property == "Publisher")
                    {
                        text.Add(title.publisher);
                    }
                    if (allColumns || property == "Languages")
                    {
                        text.Add(title.languagesString);
                    }
                    if (allColumns || property == "Filename")
                    {
                        text.Add(title.filename);
                    }
                    if (allColumns || property == "Filesize")
                    {
                        text.Add(title.filesizeString);
                    }
                    if (allColumns || property == "Type")
                    {
                        text.Add(title.typeString);
                    }
                    if (allColumns || property == "Distribution")
                    {
                        text.Add(title.distribution.ToString("G"));
                    }
                    if (allColumns || property == "Structure")
                    {
                        text.Add(title.structureString);
                    }
                    if (allColumns || property == "Signature")
                    {
                        text.Add(title.signatureString);
                    }
                    if (allColumns || property == "Permission")
                    {
                        text.Add(title.permissionString);
                    }
                    if (allColumns || property == "Error")
                    {
                        text.Add(title.error);
                    }
                }

                if (allColumns)
                {
                    text = text.Select((x, i) => new { x, i }).GroupBy(x => x.i / Title.Properties.Count()).Select(x => String.Join("\t", x.Select(t => t.x))).ToList();
                }

                if (text.Any() && !text.All(x => String.IsNullOrEmpty(x)))
                {
                    Clipboard.SetDataObject(new DataObject(DataFormats.Text, String.Join("\n", text)));
                }
                else
                {
                    MessageBox.Show(String.Format("{0} is empty", property), Application.ProductName);
                }
            }

            contextMenuStrip.Tag = null;
        }

        private enum SkipReason
        {
            Duplicate,
            Existing,
            Missing,
        }

        private void renameFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (contextMenuStrip.Tag is Title title)
            {
                List<Tuple<string, string>> renameList = new List<Tuple<string, string>>();
                List<Tuple<string, string, SkipReason>> skippedList = new List<Tuple<string, string, SkipReason>>();

                int selectedCount = objectListView.SelectedItems.Count;

                foreach (OLVListItem item in objectListView.SelectedItems)
                {
                    title = (Title)item.RowObject;

                    string filename = Path.GetFullPath(title.filename);
                    string newname = Path.Combine(Path.GetDirectoryName(filename), Regex.Replace(Common.Settings.Default.RenameFormat
                        .Replace("{n}", title.titleName)
                        .Replace("{i}", title.titleID)
                        .Replace("{j}", title.baseTitleID)
                        .Replace("{v}", title.version.ToString())
                        .Replace("{w}", (title.version >= 65536 ? title.version / 65536 : 0).ToString())
                        .Replace("{d}", title.displayVersion),
                        String.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))), "") + Path.GetExtension(filename));

                    if (filename == newname)
                    {
                        skippedList.Add(Tuple.Create(filename, newname, SkipReason.Duplicate));
                        Process.log?.WriteLine("Skipping file \"{0}\": The source and destination file names are the same", filename);
                    }
                    else
                    {
                        if (File.Exists(filename))
                        {
                            if (File.Exists(newname))
                            {
                                skippedList.Add(Tuple.Create(filename, newname, SkipReason.Existing));
                                Process.log?.WriteLine("Skipping file \"{0}\": There is already a file with the same name", filename);
                            }
                            else
                            {
                                renameList.Add(Tuple.Create(filename, newname));
                            }
                        }
                        else
                        {
                            skippedList.Add(Tuple.Create(filename, newname, SkipReason.Missing));
                            Process.log?.WriteLine("Skipping file \"{0}\": The source file could not be found", filename);
                        }
                    }
                }

                if (skippedList.Count == selectedCount)
                {
                    string message = "";

                    if (skippedList.Select(x => x.Item3).Any(x => x == SkipReason.Duplicate))
                    {
                        message += String.Format("Files do not need renaming:\n{0}",
                            String.Join("\n", skippedList.Where(x => x.Item3 == SkipReason.Duplicate).Select(x => String.Format("• \"{0}\"", x.Item1))));
                    }
                    if (skippedList.Select(x => x.Item3).Any(x => x == SkipReason.Existing))
                    {
                        if (message.Length > 0)
                        {
                            message += "\n\n";
                        }
                        message += String.Format("Files of the same names already exist:\n{0}",
                            String.Join("\n", skippedList.Where(x => x.Item3 == SkipReason.Existing).Select(x => String.Format("• \"{0}\" to \"{1}\"", x.Item1, Path.GetFileName(x.Item2)))));
                    }
                    if (skippedList.Select(x => x.Item3).Any(x => x == SkipReason.Missing))
                    {
                        if (message.Length > 0)
                        {
                            message += "\n\n";
                        }
                        message += String.Format("Files could not be found:\n{0}",
                            String.Join("\n", skippedList.Where(x => x.Item3 == SkipReason.Missing).Select(x => String.Format("• \"{0}\"", x.Item1))));
                    }

                    TaskDialogPage page = new TaskDialogPage()
                    {
                        Title = Application.ProductName,
                        Text = String.Format("Skip renaming {0} files", skippedList.Count),
                        Expander =
                        {
                            Text = message,
                            ExpandFooterArea = true,
                        },
                        Width = 350,
                    };

                    new TaskDialog(page).Show(Handle);
                }
                else
                {
                    List<Tuple<string, string>> failedList = new List<Tuple<string, string>>();

                    TaskDialogPage page = new TaskDialogPage()
                    {
                        Title = Application.ProductName,
                        Text = String.Format("{0} of {1} files will be renamed. Do you wish to continue renaming?", renameList.Count, selectedCount),
                        Expander =
                        {
                            Text = String.Join("\n", renameList.Select(x => String.Format("• \"{0}\" to \"{1}\"", x.Item1, Path.GetFileName(x.Item2)))),
                            ExpandFooterArea = true,
                        },
                        StandardButtons =
                        {
                            new TaskDialogStandardButton(TaskDialogResult.OK)
                            {
                                DefaultButton = true,
                            },
                            new TaskDialogStandardButton(TaskDialogResult.Cancel),
                        },
                        Width = 350,
                    };

                    if (((TaskDialogStandardButton)new TaskDialog(page).Show(Handle)).Result == TaskDialogResult.OK)
                    {
                        int index = historyToolStripMenuItem.DropDownItems.OfType<ToolStripMenuItem>().Select((item, i) => new { item, i }).FirstOrDefault(x => x.item.Checked)?.i ?? -1;

                        foreach (Tuple<string, string> rename in renameList)
                        {
                            string filename = rename.Item1;
                            string newname = rename.Item2;

                            Process.log?.WriteLine("Renaming file \"{0}\" to \"{1}\"", filename, newname);

                            try
                            {
                                new FileInfo(filename).MoveTo(newname);

                                titles.Where(x => x.filename == filename).ToList().ForEach(x => x.filename = newname);

                                if (index != -1)
                                {
                                    Common.History.Default.Titles[index].title.Where(x => x.filename == filename).ToList().ForEach(x => x.filename = newname);
                                }
                            }
                            catch (SystemException ex) when (ex is NotSupportedException || ex is UnauthorizedAccessException || ex is IOException)
                            {
                                failedList.Add(Tuple.Create(filename, newname));

                                Process.log?.WriteLine(ex.StackTrace);
                                Process.log?.WriteLine("Failed to rename file \"{0}\"", filename);
                            }
                        }

                        Common.History.Default.Save();

                        reloadData();

                        string message = "";

                        if (failedList.Any())
                        {
                            message += String.Format("Failed to rename files:\n{0}",
                                String.Join("\n", failedList.Select(x => String.Format("\"{0}\" to \"{1}\"", x.Item1, Path.GetFileName(x.Item2)))));
                        }
                        if (skippedList.Select(x => x.Item3).Any(x => x == SkipReason.Duplicate))
                        {
                            if (message.Length > 0)
                            {
                                message += "\n\n";
                            }
                            message += String.Format("Files do not need renaming:\n{0}",
                                String.Join("\n", skippedList.Where(x => x.Item3 == SkipReason.Duplicate).Select(x => String.Format("• \"{0}\"", x.Item1))));
                        }
                        if (skippedList.Select(x => x.Item3).Any(x => x == SkipReason.Existing))
                        {
                            if (message.Length > 0)
                            {
                                message += "\n\n";
                            }
                            message += String.Format("Files of the same names already exist:\n{0}",
                                String.Join("\n", skippedList.Where(x => x.Item3 == SkipReason.Existing).Select(x => String.Format("• \"{0}\" to \"{1}\"", x.Item1, Path.GetFileName(x.Item2)))));
                        }
                        if (skippedList.Select(x => x.Item3).Any(x => x == SkipReason.Missing))
                        {
                            if (message.Length > 0)
                            {
                                message += "\n\n";
                            }
                            message += String.Format("Files could not be found:\n{0}",
                                String.Join("\n", skippedList.Where(x => x.Item3 == SkipReason.Missing).Select(x => String.Format("• \"{0}\"", x.Item1))));
                        }

                        TaskDialogPage dialogPage = new TaskDialogPage()
                        {
                            Title = Application.ProductName,
                            Text = String.Format("{0} of {1} files renamed" + (failedList.Count > 0 ? ". {2} files failed to rename" : ""),
                                renameList.Count - failedList.Count, selectedCount, failedList.Count),
                            Expander =
                            {
                                Text = message,
                                ExpandFooterArea = true,
                            },
                            Width = 350,
                        };

                        new TaskDialog(dialogPage).Show(Handle);
                    }
                }
            }

            contextMenuStrip.Tag = null;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr ILCreateFromPathW(string pszPath);

        [DllImport("shell32.dll")]
        private static extern void ILFree(IntPtr pidl);

        [DllImport("shell32.dll")]
        private static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, int cild, [In, Optional, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, int dwFlags);

        private void openFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (contextMenuStrip.Tag is Title title)
            {
                List<string> files = new List<string>();

                foreach (OLVListItem item in objectListView.SelectedItems)
                {
                    title = (Title)item.RowObject;

                    if (File.Exists(title.filename))
                    {
                        files.Add(title.filename);
                    }
                }

                if (files.Any())
                {
                    string path = files.Select(x => Path.GetDirectoryName(x)).Aggregate(files.FirstOrDefault(), (x, y) => String.Join("", x.Substring(0, Math.Min(x.Length, y.Length)).TakeWhile((c, i) => c == y[i])), r => r);

                    files.ForEach(x =>
                    {
                        while (!x.Contains(path + "\\"))
                        {
                            path = Path.GetDirectoryName(path);
                        }
                    });

                    if (Directory.Exists(path))
                    {
                        IntPtr pidl = ILCreateFromPathW(path);
                        List<IntPtr> filesPidl = new List<IntPtr>();
                        foreach (var file in files)
                        {
                            filesPidl.Add(ILCreateFromPathW(file));
                        }

                        try
                        {
                            SHOpenFolderAndSelectItems(pidl, filesPidl.Count(), filesPidl.ToArray(), 0);
                        }
                        finally
                        {
                            ILFree(pidl);
                            filesPidl.ForEach(x => ILFree(x));
                        }
                    }
                    else
                    {
                        MessageBox.Show(String.Format("{0} is not a valid directory", path), Application.ProductName);
                    }
                }
                else if (objectListView.SelectedItems.Count > 0)
                {
                    MessageBox.Show("The selected files could not be found", Application.ProductName);
                }
                else
                {
                    MessageBox.Show("There is no title selected", Application.ProductName);
                }
            }

            contextMenuStrip.Tag = null;
        }

        private void objectListView_Freezing(object sender, FreezeEventArgs e)
        {
            if (e.FreezeLevel == 0)
            {
                reloadData();
            }
        }

        private void backgroundWorkerProcess_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            titles.Clear();
            Process.latestVersions.Clear();

            if (e.Argument is ValueTuple<Worker, string[]> argumentFile)
            {
                if (argumentFile.Item1 == Worker.File && argumentFile.Item2 is string[] files)
                {
                    List<string> filenames = files.ToList();
                    filenames.Sort();

                    Process.log?.WriteLine("{0} files selected", filenames.Count);

                    worker.ReportProgress(-1, String.Format("Opening {0} files", filenames.Count));

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
            }
            else if (e.Argument is ValueTuple<Worker, string> argumentPath)
            {
                if (argumentPath.Item1 == Worker.Directory && argumentPath.Item2 is string path)
                {
                    List<string> filenames = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                        .Where(filename => filename.ToLower().EndsWith(".xci") || filename.ToLower().EndsWith(".nsp") || filename.ToLower().EndsWith(".nro") ||
                        (Common.Settings.Default.NszExtension && (filename.ToLower().EndsWith(".xcz") || filename.ToLower().EndsWith(".nsz")))).ToList();
                    filenames.Sort();

                    Process.log?.WriteLine("{0} files selected", filenames.Count);

                    worker.ReportProgress(-1, String.Format("Opening {0} files from directory {1}", filenames.Count, path));

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
                else if (argumentPath.Item1 == Worker.SDCard && argumentPath.Item2 is string pathSd)
                {
                    List<FsTitle> fsTitles = Process.processSd(pathSd);

                    if (fsTitles != null)
                    {
                        int count = fsTitles.Count, index = 0;

                        foreach (var fsTitle in fsTitles)
                        {
                            if (worker.CancellationPending) break;

                            worker.ReportProgress(100 * index++ / count, fsTitle.MainNca?.Filename);

                            Title title = Process.processTitle(fsTitle);
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
                    else
                    {
                        worker.ReportProgress(0, "");

                        string error = "SD card \"Contents\" directory could not be found";
                        Process.log?.WriteLine(error);

                        e.Result = error;
                        return;
                    }
                }
            }

            e.Result = titles;
        }

        private void backgroundWorkerProcess_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (progressDialog.HasUserCancelled())
            {
                if (backgroundWorkerProcess.IsBusy)
                {
                    backgroundWorkerProcess.CancelAsync();
                }
            }

            if (e.ProgressPercentage == -1)
            {
                progressDialog.SetLine(1, e.UserState as string, false, IntPtr.Zero);
            }
            else
            {
                progressDialog.SetLine(2, e.UserState as string, true, IntPtr.Zero);
                progressDialog.SetProgress((uint)e.ProgressPercentage, 100);
            }
        }

        private void backgroundWorkerProcess_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Result is List<Title> titles)
            {
                reloadData();

                ArrayOfTitle history = new ArrayOfTitle
                {
                    description = DateTime.Now.ToString("dd MMMM yyyy HH:mm:ss"),
                    title = titles.ToList(),
                };
                Common.History.Default.Titles.Add(history);
                if (Common.History.Default.Titles.Count > Common.HISTORY_SIZE)
                {
                    Common.History.Default.Titles.RemoveRange(0, Common.History.Default.Titles.Count - Common.HISTORY_SIZE);
                }
                Common.History.Default.Save();

                foreach (ToolStripMenuItem item in historyToolStripMenuItem.DropDownItems)
                {
                    item.Checked = false;
                }

                ToolStripMenuItem menuItem = new ToolStripMenuItem
                {
                    Name = String.Format("history{0}ToolStripMenuItem", Common.History.Default.Titles.Count - 1),
                    Text = String.Format("{0} ({1} files)", history.description, history.title.Count),
                    CheckOnClick = true,
                };
                menuItem.Click += new System.EventHandler(this.historyToolStripMenuItem_Click);
                menuItem.Checked = true;
                historyToolStripMenuItem.DropDownItems.Add(menuItem);

                while (historyToolStripMenuItem.DropDownItems.Count > Common.HISTORY_SIZE)
                {
                    historyToolStripMenuItem.DropDownItems.RemoveAt(0);
                }

                toolStripStatusLabel.Text = String.Format("{0} files", titles.Count);

                progressDialog.StopProgressDialog();
                Activate();
            }
            else if (e.Result is string error)
            {
                progressDialog.StopProgressDialog();

                MessageBox.Show(String.Format("{0}.", error), Application.ProductName);
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            renameBox = new RenameBox();
            renameBox.ShowDialog();
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
