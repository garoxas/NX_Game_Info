using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Foundation;
using AppKit;
using Title = NX_Game_Info.Common.Title;

#pragma warning disable RECS0061 // Warns when a culture-aware 'EndsWith' call is used by default.
#pragma warning disable RECS0063 // Warns when a culture-aware 'StartsWith' call is used by default.

namespace NX_Game_Info
{
    public partial class MainWindowController : NSWindowController
    {
        private TableViewDataSource tableViewDataSource;
        private TableViewDelegate tableViewDelegate;

        private BackgroundWorker backgroundWorker;

        public MainWindowController(IntPtr handle) : base(handle)
        {
        }

        [Export("initWithCoder:")]
        public MainWindowController(NSCoder coder) : base(coder)
        {
        }

        public MainWindowController() : base("MainWindow")
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
        }

        public new MainWindow Window
        {
            get { return (MainWindow)base.Window; }
        }

        public override void WindowDidLoad()
        {
            base.WindowDidLoad();

            tableViewDataSource = new TableViewDataSource();
            tableViewDelegate = new TableViewDelegate(tableViewDataSource);

            tableView.DataSource = tableViewDataSource;
            tableView.Delegate = tableViewDelegate;

            bool init = Process.initialize(out List<string> messages);

            foreach (var message in messages)
            {
                var alert = new NSAlert()
                {
                    InformativeText = message,
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                };
                alert.RunModal();
            }

            if (!init)
            {
                Environment.Exit(-1);
            }

            backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
        }

        [Export("openDocument:")]
        public void OpenFile(NSMenuItem menuItem)
        {
            if (backgroundWorker.IsBusy)
            {
                var alert = new NSAlert()
                {
                    InformativeText = "Please wait until the current process is finished and try again.",
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                };
                alert.RunModal();
                return;
            }

            NSOpenPanel openPanel = NSOpenPanel.OpenPanel;
            openPanel.CanChooseFiles = true;
            openPanel.CanChooseDirectories = false;
            openPanel.AllowsMultipleSelection = true;
            openPanel.AllowedFileTypes = new string[] { "xci", "nsp" };
            openPanel.DirectoryUrl = new NSUrl(Common.Settings.Default.InitialDirectory ?? "");

            if (openPanel.RunModal() == (int)NSModalResponse.OK)
            {
                tableViewDataSource.Titles.Clear();
                tableView.ReloadData();

                List<string> filenames = openPanel.Urls.Select((arg) => arg.Path).ToList();
                filenames.Sort();

                Common.Settings.Default.InitialDirectory = Path.GetDirectoryName(filenames.First());
                Common.Settings.Default.Save();

                message.StringValue = "";
                progress.DoubleValue = 0;

                Window.BeginSheet(sheet, ProgressComplete);

                backgroundWorker.RunWorkerAsync(filenames);
            }
        }

        [Export("open:")]
        public void OpenDirectory(NSMenuItem menuItem)
        {
            if (backgroundWorker.IsBusy)
            {
                var alert = new NSAlert()
                {
                    InformativeText = "Please wait until the current process is finished and try again.",
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                };
                alert.RunModal();
                return;
            }

            NSOpenPanel openPanel = NSOpenPanel.OpenPanel;
            openPanel.CanChooseFiles = false;
            openPanel.CanChooseDirectories = true;
            openPanel.DirectoryUrl = new NSUrl(Common.Settings.Default.InitialDirectory ?? "");

            if (openPanel.RunModal() == (int)NSModalResponse.OK)
            {
                tableViewDataSource.Titles.Clear();
                tableView.ReloadData();

                List<string> filenames = Directory.EnumerateFiles(openPanel.Urls.First().Path, "*.*", SearchOption.AllDirectories)
                    .Where(filename => filename.ToLower().EndsWith(".xci") || filename.ToLower().EndsWith(".nsp")).ToList();
                filenames.Sort();

                Common.Settings.Default.InitialDirectory = openPanel.Urls.First().Path;
                Common.Settings.Default.Save();

                message.StringValue = "";
                progress.DoubleValue = 0;

                Window.BeginSheet(sheet, ProgressComplete);

                backgroundWorker.RunWorkerAsync(filenames);
            }
        }

        [Export("newDocument:")]
        public void OpenSDCard(NSMenuItem menuItem)
        {
            if (Process.keyset?.SdSeed.All(b => b == 0) ?? true)
            {
                var alert = new NSAlert()
                {
                    InformativeText = "sd_seed is missing from Console Keys.\nOpen SD Card will not be available.",
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                };
                alert.RunModal();
                return;
            }

            if ((Process.keyset?.SdCardKekSource.All(b => b == 0) ?? true) || (Process.keyset?.SdCardKeySources[1].All(b => b == 0) ?? true))
            {
                var alert = new NSAlert()
                {
                    InformativeText = "sd_card_kek_source and sd_card_nca_key_source are missing from Keyfile.\nOpen SD Card will not be available.",
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                };
                alert.RunModal();
                return;
            }

            if (backgroundWorker.IsBusy)
            {
                var alert = new NSAlert()
                {
                    InformativeText = "Please wait until the current process is finished and try again.",
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                };
                alert.RunModal();
                return;
            }

            NSOpenPanel openPanel = NSOpenPanel.OpenPanel;
            openPanel.CanChooseFiles = false;
            openPanel.CanChooseDirectories = true;
            openPanel.DirectoryUrl = new NSUrl(Common.Settings.Default.InitialDirectory ?? "");

            if (openPanel.RunModal() == (int)NSModalResponse.OK)
            {
                tableViewDataSource.Titles.Clear();
                tableView.ReloadData();

                Common.Settings.Default.InitialDirectory = openPanel.Urls.First().Path;
                Common.Settings.Default.Save();

                message.StringValue = "";
                progress.DoubleValue = 0;

                Window.BeginSheet(sheet, ProgressComplete);

                backgroundWorker.RunWorkerAsync(openPanel.Urls.First().Path);
            }
        }

        void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
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
            }

            e.Result = titles;
        }

        void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            message.StringValue = e.UserState as string;
            progress.DoubleValue = e.ProgressPercentage;
        }

        void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            tableViewDataSource.Titles.AddRange((List<Title>)e.Result);

            tableView.ReloadData();

            Window.EndSheet(sheet);
        }

        [Export("cancelProgress:")]
        public void CancelProgress(NSObject sender)
        {
            message.StringValue = "Please wait until the current process is finished";

            backgroundWorker.CancelAsync();
        }

        void ProgressComplete(nint obj)
        {

        }
    }

    public class TableViewDataSource : NSTableViewDataSource
    {
        internal List<Title> Titles { get; } = new List<Title>();

        public override nint GetRowCount(NSTableView tableView)
        {
            return Titles.Count;
        }

        public override void SortDescriptorsChanged(NSTableView tableView, NSSortDescriptor[] oldDescriptors)
        {
            NSSortDescriptor sortDescriptor = tableView.SortDescriptors[0];
            if (sortDescriptor != null)
            {
                Titles.Sort((x, y) =>
                {
                    switch (sortDescriptor.Key)
                    {
                        case "titleID":
                            return string.Compare(x.titleID, y.titleID) * (sortDescriptor.Ascending ? 1 : -1);
                        case "titleName":
                            return string.Compare(x.titleName, y.titleName) * (sortDescriptor.Ascending ? 1 : -1);
                        case "displayVersion":
                            return string.Compare(x.displayVersion, y.displayVersion) * (sortDescriptor.Ascending ? 1 : -1);
                        case "versionString":
                            return string.Compare(x.versionString, y.versionString) * (sortDescriptor.Ascending ? 1 : -1);
                        case "latestVersionString":
                            return string.Compare(x.latestVersionString, y.latestVersionString) * (sortDescriptor.Ascending ? 1 : -1);
                        case "firmware":
                            return string.Compare(x.firmware, y.firmware) * (sortDescriptor.Ascending ? 1 : -1);
                        case "masterkeyString":
                            return string.Compare(x.masterkeyString, y.masterkeyString) * (sortDescriptor.Ascending ? 1 : -1);
                        case "filename":
                            return string.Compare(x.filename, y.filename) * (sortDescriptor.Ascending ? 1 : -1);
                        case "filesizeString":
                            return (int)((x.filesize - y.filesize) * (sortDescriptor.Ascending ? 1 : -1));
                        case "typeString":
                            return string.Compare(x.typeString, y.typeString) * (sortDescriptor.Ascending ? 1 : -1);
                        case "distribution":
                            return string.Compare(x.distribution.ToString(), y.distribution.ToString()) * (sortDescriptor.Ascending ? 1 : -1);
                        case "structureString":
                            return string.Compare(x.structureString, y.structureString) * (sortDescriptor.Ascending ? 1 : -1);
                        case "signatureString":
                            return string.Compare(x.signatureString, y.signatureString) * (sortDescriptor.Ascending ? 1 : -1);
                        case "permissionString":
                            return string.Compare(x.permissionString, y.permissionString) * (sortDescriptor.Ascending ? 1 : -1);
                        case "error":
                            return string.Compare(x.error, y.error) * (sortDescriptor.Ascending ? 1 : -1);
                        default:
                            return 0;
                    }
                });

                tableView.ReloadData();
            }
        }
    }

    public class TableViewDelegate : NSTableViewDelegate
    {
        TableViewDataSource dataSource;

        public TableViewDelegate(TableViewDataSource dataSource)
        {
            this.dataSource = dataSource;
        }

        public override NSView GetViewForItem(NSTableView tableView, NSTableColumn tableColumn, nint row)
        {
            NSTextField textField = (NSTextField)tableView.MakeView("TextField", null);
            if (textField == null)
            {
                textField = new NSTextField
                {
                    BackgroundColor = NSColor.Clear,
                    Bordered = false,
                    Editable = false,
                };
            }

            Title title = dataSource.Titles[(int)row];
            switch (tableColumn.Identifier)
            {
                case "TitleID":
                    textField.StringValue = title.titleID ?? "";
                    break;
                case "TitleName":
                    textField.StringValue = title.titleName ?? "";
                    break;
                case "DisplayVersion":
                    textField.StringValue = title.displayVersion ?? "";
                    break;
                case "Version":
                    textField.StringValue = title.versionString ?? "";
                    break;
                case "LatestVersion":
                    textField.StringValue = title.latestVersionString ?? "";
                    break;
                case "Firmware":
                    textField.StringValue = title.firmware ?? "";
                    break;
                case "MasterKey":
                    textField.StringValue = title.masterkeyString ?? "";
                    break;
                case "FileName":
                    textField.StringValue = title.filename ?? "";
                    break;
                case "FileSize":
                    textField.StringValue = NSByteCountFormatter.Format(title.filesize, NSByteCountFormatterCountStyle.File) ?? "";
                    break;
                case "Type":
                    textField.StringValue = title.typeString ?? "";
                    break;
                case "Distribution":
                    textField.StringValue = title.distribution.ToString() ?? "";
                    break;
                case "Structure":
                    textField.StringValue = title.structureString ?? "";
                    break;
                case "Signature":
                    textField.StringValue = title.signatureString ?? "";
                    break;
                case "Permission":
                    textField.StringValue = title.permissionString ?? "";
                    break;
                case "Error":
                    textField.StringValue = title.error ?? "";
                    break;
            }

            if (title.signature != true)
            {
                textField.BackgroundColor = NSColor.Gray.ColorWithAlphaComponent((nfloat)0.1);
            }

            if (title.permission == Title.Permission.Dangerous)
            {
                textField.TextColor = NSColor.Red;
            }
            else if (title.permission == Title.Permission.Unsafe)
            {
                textField.TextColor = NSColor.Purple;
            }

            textField.Cell.LineBreakMode = NSLineBreakMode.CharWrapping;

            return textField;
        }
    }
}
