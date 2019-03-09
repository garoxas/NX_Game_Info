using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Foundation;
using AppKit;
using FsTitle = LibHac.Title;
using Title = NX_Game_Info.Common.Title;

#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
#pragma warning disable RECS0061 // Warns when a culture-aware 'EndsWith' call is used by default.
#pragma warning disable RECS0063 // Warns when a culture-aware 'StartsWith' call is used by default.
#pragma warning disable RECS0117 // Local variable has the same name as a member and hides it

namespace NX_Game_Info
{
    public partial class MainWindowController : NSWindowController
    {
        private TableViewDataSource tableViewDataSource;
        private TableViewDelegate tableViewDelegate;

        private BackgroundWorker backgroundWorker;
        private bool userCancelled;

        public enum Worker
        {
            File,
            Directory,
            SDCard,
            Invalid = -1
        }

        private List<Title> titles = new List<Title>();

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

            NSMenuItem debugLog = Window.Menu?.ItemWithTitle("File")?.Submenu.ItemWithTitle("Debug Log");
            if (debugLog != null)
            {
                debugLog.State = Common.Settings.Default.DebugLog ? NSCellStateValue.On : NSCellStateValue.Off;
            }

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
            openPanel.AllowedFileTypes = new string[] { "xci", "nsp", "nro" };
            openPanel.DirectoryUrl = new NSUrl(!String.IsNullOrEmpty(Common.Settings.Default.InitialDirectory) && Directory.Exists(Common.Settings.Default.InitialDirectory) ? Common.Settings.Default.InitialDirectory : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            openPanel.Title = "Open NX Game Files";

            Process.log?.WriteLine("\nOpen File");

            openPanel.BeginSheet(Window, (nint result) =>
            {
                if (result == (int)NSModalResponse.OK)
                {
                    tableViewDataSource.Titles.Clear();
                    tableView.ReloadData();

                    Common.Settings.Default.InitialDirectory = Path.GetDirectoryName(openPanel.Urls.First().Path);
                    Common.Settings.Default.Save();

                    title.StringValue = String.Format("Opening files");
                    message.StringValue = "";
                    progress.DoubleValue = 0;

                    Window.BeginSheet(sheet, ProgressComplete);

                    backgroundWorker.RunWorkerAsync((Worker.File, openPanel.Urls.Select((arg) => arg.Path).ToList()));
                }
            });
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
            openPanel.DirectoryUrl = new NSUrl(!String.IsNullOrEmpty(Common.Settings.Default.InitialDirectory) && Directory.Exists(Common.Settings.Default.InitialDirectory) ? Common.Settings.Default.InitialDirectory : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            openPanel.Title = "Open NX Game Directory";

            Process.log?.WriteLine("\nOpen Directory");

            openPanel.BeginSheet(Window, (nint result) =>
            {
                if (result == (int)NSModalResponse.OK)
                {
                    tableViewDataSource.Titles.Clear();
                    tableView.ReloadData();

                    Common.Settings.Default.InitialDirectory = openPanel.Urls.First().Path;
                    Common.Settings.Default.Save();

                    title.StringValue = String.Format("Opening files from directory {0}", openPanel.Urls.First().Path);
                    message.StringValue = "";
                    progress.DoubleValue = 0;

                    Window.BeginSheet(sheet, ProgressComplete);

                    backgroundWorker.RunWorkerAsync((Worker.Directory, openPanel.Urls.First().Path));
                }
            });
        }

        [Export("save:")]
        public void OpenSDCard(NSMenuItem menuItem)
        {
            if (Process.keyset?.SdSeed?.All(b => b == 0) ?? true)
            {
                string error = "sd_seed is missing from Console Keys";
                Process.log?.WriteLine(error);

                var alert = new NSAlert()
                {
                    InformativeText = String.Format("{0}.\nOpen SD Card will not be available.", error),
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                };
                alert.RunModal();
                return;
            }

            if ((Process.keyset?.SdCardKekSource?.All(b => b == 0) ?? true) || (Process.keyset?.SdCardKeySources?[1]?.All(b => b == 0) ?? true))
            {
                Process.log?.WriteLine("Keyfile missing required keys");
                Process.log?.WriteLine(" - {0} ({1}exists)", "sd_card_kek_source", (bool)Process.keyset?.SdCardKekSource?.Any(b => b != 0) ? "" : "not ");
                Process.log?.WriteLine(" - {0} ({1}exists)", "sd_card_nca_key_source", (bool)Process.keyset?.SdCardKeySources?[1]?.Any(b => b != 0) ? "" : "not ");

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
            openPanel.DirectoryUrl = new NSUrl(!String.IsNullOrEmpty(Common.Settings.Default.SDCardDirectory) && Directory.Exists(Common.Settings.Default.SDCardDirectory) ? Common.Settings.Default.SDCardDirectory : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            openPanel.Title = "Open SD Card";

            Process.log?.WriteLine("\nOpen SD Card");

            openPanel.BeginSheet(Window, (nint result) =>
            {
                if (result == (int)NSModalResponse.OK)
                {
                    tableViewDataSource.Titles.Clear();
                    tableView.ReloadData();

                    Common.Settings.Default.SDCardDirectory = openPanel.Urls.First().Path;
                    Common.Settings.Default.Save();

                    title.StringValue = String.Format("Opening SD card on {0}", openPanel.Urls.First().Path);
                    message.StringValue = "";
                    progress.DoubleValue = 0;

                    Process.log?.WriteLine("SD card selected");

                    Window.BeginSheet(sheet, ProgressComplete);

                    backgroundWorker.RunWorkerAsync((Worker.SDCard, openPanel.Urls.First().Path));
                }
            });
        }

        [Export("debugLog:")]
        public void DebugLog(NSMenuItem menuItem)
        {
            menuItem.State = menuItem.State == NSCellStateValue.On ? NSCellStateValue.Off : NSCellStateValue.On;

            Common.Settings.Default.DebugLog = menuItem.State == NSCellStateValue.On;
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

        [Export("export:")]
        public void Export(NSMenuItem menuItem)
        {
            NSSavePanel savePanel = NSSavePanel.SavePanel;
            savePanel.AllowedFileTypes = new string[] { "txt" };
            savePanel.Title = "Export Titles";

            Process.log?.WriteLine("\nExport Titles");

            savePanel.BeginSheet(Window, (nint result) =>
            {
                if (result == (int)NSModalResponse.OK)
                {
                    using (var writer = new StreamWriter(savePanel.Url.Path))
                    {
                        Window.BeginSheet(sheet, ProgressComplete);
                        userCancelled = false;

                        writer.WriteLine("{0} {1}", NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleName").ToString(), NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString").ToString());
                        writer.WriteLine("--------------------------------------------------------------\n");

                        writer.WriteLine("Export titles starts at {0}\n", String.Format("{0:F}", DateTime.Now));

                        uint index = 0, count = (uint)titles.Count;

                        foreach (var title in titles)
                        {
                            if (userCancelled)
                            {
                                userCancelled = false;
                                break;
                            }

                            message.StringValue = title.titleName ?? "";
                            progress.DoubleValue = 100f * index++ / count;

                            writer.WriteLine("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}|{12}|{13}|{14}",
                                title.titleID,
                                title.titleName,
                                title.displayVersion,
                                title.versionString,
                                title.latestVersionString,
                                title.firmware,
                                title.masterkeyString,
                                title.filename,
                                title.filesizeString,
                                title.typeString,
                                title.distribution,
                                title.structureString,
                                title.signatureString,
                                title.permissionString,
                                title.error);
                        }

                        writer.WriteLine("\n{0} of {1} titles exported", index, titles.Count);

                        Process.log?.WriteLine("\n{0} of {1} titles exported", index, titles.Count);

                        Window.EndSheet(sheet);
                    }
                }
            });
        }

        void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            titles.Clear();

            if (e.Argument is ValueTuple<Worker, List<string>> argumentFile)
            {
                if (argumentFile.Item1 == Worker.File && argumentFile.Item2 is List<string> filenames)
                {
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
                        .Where(filename => filename.ToLower().EndsWith(".xci") || filename.ToLower().EndsWith(".nsp") || filename.ToLower().EndsWith(".nro")).ToList();
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

        void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == -1)
            {
                title.StringValue = e.UserState as string;
            }
            else
            {
                message.StringValue = e.UserState as string;
                progress.DoubleValue = e.ProgressPercentage;
            }
        }

        void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is List<Title> titles)
            {
                tableViewDataSource.Titles.AddRange(titles);

                tableView.ReloadData();

                Window.EndSheet(sheet);
            }
            else if (e.Result is string error)
            {
                Window.EndSheet(sheet);

                var alert = new NSAlert()
                {
                    InformativeText = String.Format("{0}.", error),
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                };
                alert.RunModal();
            }
        }

        [Export("cancelProgress:")]
        public void CancelProgress(NSObject sender)
        {
            message.StringValue = "Please wait until the current process is finished";

            if (backgroundWorker.IsBusy)
            {
                backgroundWorker.CancelAsync();
            }

            userCancelled = true;
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
                    textField.StringValue = title.filesizeString ?? "";
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
