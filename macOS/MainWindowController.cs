using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Foundation;
using AppKit;
using Bluegrams.Application;
using LibHac;
using OfficeOpenXml;
using FsTitle = LibHac.Title;
using Title = NX_Game_Info.Common.Title;
using ArrayOfTitle = NX_Game_Info.Common.ArrayOfTitle;

#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
#pragma warning disable RECS0061 // Warns when a culture-aware 'EndsWith' call is used by default.
#pragma warning disable RECS0063 // Warns when a culture-aware 'StartsWith' call is used by default.
#pragma warning disable RECS0064 // Warns when a culture-aware 'string.CompareTo' call is used by default
#pragma warning disable RECS0117 // Local variable has the same name as a member and hides it

namespace NX_Game_Info
{
    public partial class MainWindowController : NSWindowController
    {
        private TableViewDataSource tableViewDataSource;
        private TableViewDelegate tableViewDelegate;

        private NSMenu historyMenu;

        private BackgroundWorker backgroundWorker;
        private bool userCancelled;

        public enum Worker
        {
            File,
            Directory,
            SDCard,
            Invalid = -1
        }

        internal List<Title> titles = new List<Title>();

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

            PortableSettingsProvider.SettingsFileName = Common.USER_SETTINGS;
            PortableSettingsProviderBase.SettingsDirectory = Process.path_prefix;
            PortableSettingsProvider.ApplyProvider(Common.Settings.Default, Common.History.Default);

            Common.Settings.Default.Upgrade();
            Common.History.Default.Upgrade();

            NSMenuItem debugLog = Window.Menu?.ItemWithTitle("File")?.Submenu.ItemWithTitle("Debug Log");
            if (debugLog != null)
            {
                debugLog.State = Common.Settings.Default.DebugLog ? NSCellStateValue.On : NSCellStateValue.Off;
            }

            historyMenu = Window.Menu?.ItemWithTitle("History")?.Submenu;

            InitContextMenu(contextMenu.ItemWithTitle("Copy")?.Submenu);

            bool init = Process.initialize(out List<string> messages);

            foreach (var message in messages)
            {
                new NSAlert()
                {
                    InformativeText = message,
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                }
                .RunModal();
            }

            if (!init)
            {
                Environment.Exit(-1);
            }

            Process.migrateSettings();

            backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

            int index = 0;
            foreach (ArrayOfTitle history in Common.History.Default.Titles)
            {
                NSMenuItem menuItem = new NSMenuItem(String.Format("{0} ({1} files)", history.description, history.title.Count), new System.EventHandler(History));
                historyMenu.AddItem(menuItem);

                index++;
            }

            if (index > 0)
                historyMenu.Items[index - 1].State = NSCellStateValue.On;

            titles = Process.processHistory();

            tableViewDataSource.Titles.AddRange(titles);

            tableView.ReloadData();

            tableView.RegisterForDraggedTypes(new string[] { "NSFilenamesPboardType" });
            tableViewDataSource.viewController = this;
        }

        [Export("openDocument:")]
        public void OpenFile(NSMenuItem menuItem)
        {
            if (backgroundWorker.IsBusy)
            {
                new NSAlert()
                {
                    InformativeText = "Please wait until the current process is finished and try again.",
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                }
                .RunModal();
                return;
            }

            NSOpenPanel openPanel = NSOpenPanel.OpenPanel;
            openPanel.CanChooseFiles = true;
            openPanel.CanChooseDirectories = false;
            openPanel.AllowsMultipleSelection = true;
            openPanel.AllowedFileTypes = Common.Settings.Default.NszExtension ? new string[] { "xci", "nsp", "xcz", "nsz", "nro" } : new string[] { "xci", "nsp", "nro" };
            openPanel.DirectoryUrl = NSUrl.FromFilename(!String.IsNullOrEmpty(Common.Settings.Default.InitialDirectory) && Directory.Exists(Common.Settings.Default.InitialDirectory) ? Common.Settings.Default.InitialDirectory : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            openPanel.Title = "Open NX Game Files";

            Process.log?.WriteLine("\nOpen File");

            openPanel.BeginSheet(Window, (nint result) =>
            {
                if (result == (int)NSModalResponse.OK)
                {
                    Common.Settings.Default.InitialDirectory = Path.GetDirectoryName(openPanel.Urls.First().Path);
                    Common.Settings.Default.Save();

                    OpenFile(openPanel.Urls.Select((arg) => arg.Path).ToList());
                }
            });
        }

        public void OpenFile(List<string> filenames)
        {
            tableViewDataSource.Titles.Clear();
            tableView.ReloadData();

            title.StringValue = String.Format("Opening files");
            message.StringValue = "";
            progress.DoubleValue = 0;

            Window.BeginSheet(sheet, ProgressComplete);

            backgroundWorker.RunWorkerAsync((Worker.File, filenames));
        }

        [Export("open:")]
        public void OpenDirectory(NSMenuItem menuItem)
        {
            if (backgroundWorker.IsBusy)
            {
                new NSAlert()
                {
                    InformativeText = "Please wait until the current process is finished and try again.",
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                }
                .RunModal();
                return;
            }

            NSOpenPanel openPanel = NSOpenPanel.OpenPanel;
            openPanel.CanChooseFiles = false;
            openPanel.CanChooseDirectories = true;
            openPanel.DirectoryUrl = NSUrl.FromFilename(!String.IsNullOrEmpty(Common.Settings.Default.InitialDirectory) && Directory.Exists(Common.Settings.Default.InitialDirectory) ? Common.Settings.Default.InitialDirectory : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            openPanel.Title = "Open NX Game Directory";

            Process.log?.WriteLine("\nOpen Directory");

            openPanel.BeginSheet(Window, (nint result) =>
            {
                if (result == (int)NSModalResponse.OK)
                {
                    Common.Settings.Default.InitialDirectory = openPanel.Urls.First().Path;
                    Common.Settings.Default.Save();

                    OpenDirectory(openPanel.Urls.First().Path);
                }
            });
        }

        public void OpenDirectory(string path)
        {
            tableViewDataSource.Titles.Clear();
            tableView.ReloadData();

            title.StringValue = String.Format("Opening files from directory {0}", path);
            message.StringValue = "";
            progress.DoubleValue = 0;

            Window.BeginSheet(sheet, ProgressComplete);

            backgroundWorker.RunWorkerAsync((Worker.Directory, path));
        }

        [Export("save:")]
        public void OpenSDCard(NSMenuItem menuItem)
        {
            if (Process.keyset?.SdSeed?.All(b => b == 0) ?? true)
            {
                string error = "sd_seed is missing from Console Keys";
                Process.log?.WriteLine(error);

                new NSAlert()
                {
                    InformativeText = String.Format("{0}.\nOpen SD Card will not be available.", error),
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                }
                .RunModal();
                return;
            }

            if ((Process.keyset?.SdCardKekSource?.All(b => b == 0) ?? true) || (Process.keyset?.SdCardKeySources?[1]?.All(b => b == 0) ?? true))
            {
                Process.log?.WriteLine("Keyfile missing required keys");
                Process.log?.WriteLine(" - {0} ({1}exists)", "sd_card_kek_source", (bool)Process.keyset?.SdCardKekSource?.Any(b => b != 0) ? "" : "not ");
                Process.log?.WriteLine(" - {0} ({1}exists)", "sd_card_nca_key_source", (bool)Process.keyset?.SdCardKeySources?[1]?.Any(b => b != 0) ? "" : "not ");

                new NSAlert()
                {
                    InformativeText = "sd_card_kek_source and sd_card_nca_key_source are missing from Keyfile.\nOpen SD Card will not be available.",
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                }
                .RunModal();
                return;
            }

            if (backgroundWorker.IsBusy)
            {
                new NSAlert()
                {
                    InformativeText = "Please wait until the current process is finished and try again.",
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                }
                .RunModal();
                return;
            }

            NSOpenPanel openPanel = NSOpenPanel.OpenPanel;
            openPanel.CanChooseFiles = false;
            openPanel.CanChooseDirectories = true;
            openPanel.DirectoryUrl = NSUrl.FromFilename(!String.IsNullOrEmpty(Common.Settings.Default.SDCardDirectory) && Directory.Exists(Common.Settings.Default.SDCardDirectory) ? Common.Settings.Default.SDCardDirectory : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
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

        [Export("export:")]
        public void Export(NSMenuItem menuItem)
        {
            NSSavePanel savePanel = NSSavePanel.SavePanel;
            savePanel.AllowedFileTypes = new string[] { "csv", "xlsx" };
            savePanel.Title = "Export Titles";

            Process.log?.WriteLine("\nExport Titles");

            savePanel.BeginSheet(Window, (nint result) =>
            {
                if (result == (int)NSModalResponse.OK)
                {
                    string filename = savePanel.Url.Path;

                    if (filename.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var writer = new StreamWriter(filename))
                        {
                            Window.BeginSheet(sheet, ProgressComplete);
                            userCancelled = false;

                            char separator = Common.Settings.Default.CsvSeparator;
                            if (separator != '\0')
                            {
                                writer.WriteLine("sep={0}", separator);
                            }
                            else
                            {
                                separator = ',';
                            }

                            writer.WriteLine("# publisher {0} {1}", NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleName").ToString(), NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString").ToString());
                            writer.WriteLine("# updated {0}", String.Format("{0:F}", DateTime.Now));

                            writer.WriteLine(String.Join(separator.ToString(), Common.Title.Properties));

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

                            Window.EndSheet(sheet);

                            new NSAlert()
                            {
                                InformativeText = String.Format("{0} of {1} titles exported", index, titles.Count),
                                MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                            }
                            .RunModal();
                        }
                    }
                    else if (filename.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    {
                        using (ExcelPackage excel = new ExcelPackage())
                        {
                            Window.BeginSheet(sheet, ProgressComplete);
                            userCancelled = false;

                            ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add(Common.History.Default.Titles.LastOrDefault().description ?? NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString());

                            worksheet.Cells[1, 1, 1, Title.Properties.Count()].LoadFromArrays(new List<string[]> { Title.Properties });
                            worksheet.Cells["1:1"].Style.Font.Bold = true;
                            worksheet.Cells["1:1"].Style.Font.Color.SetColor(NSColor.White);
                            worksheet.Cells["1:1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            worksheet.Cells["1:1"].Style.Fill.BackgroundColor.SetColor(NSColor.Blue.ShadowWithLevel((nfloat)0.6));

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
                                    worksheet.Cells[(int)index + 1, 1, (int)index + 1, Title.Properties.Count()].Style.Fill.BackgroundColor.SetColor(title.signature != true ? NSColor.Orange.ColorWithAlphaComponent((nfloat)0.1) : NSColor.Yellow.ColorWithAlphaComponent((nfloat)0.1));
                                }
                                else if (title.signature != true)
                                {
                                    worksheet.Cells[(int)index + 1, 1, (int)index + 1, Title.Properties.Count()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    worksheet.Cells[(int)index + 1, 1, (int)index + 1, Title.Properties.Count()].Style.Fill.BackgroundColor.SetColor(NSColor.Gray.ColorWithAlphaComponent((nfloat)0.1));
                                }

                                if (title.permission == Title.Permission.Dangerous)
                                {
                                    worksheet.Cells[(int)index + 1, 1, (int)index + 1, Title.Properties.Count()].Style.Font.Color.SetColor(NSColor.Red);
                                }
                                else if (title.permission == Title.Permission.Unsafe)
                                {
                                    worksheet.Cells[(int)index + 1, 1, (int)index + 1, Title.Properties.Count()].Style.Font.Color.SetColor(NSColor.Purple);
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

                            Window.EndSheet(sheet);

                            new NSAlert()
                            {
                                InformativeText = String.Format("{0} of {1} titles exported", index, titles.Count),
                                MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                            }
                            .RunModal();
                        }
                    }
                    else
                    {
                        new NSAlert()
                        {
                            InformativeText = String.Format("This file type is not supported {0}", Path.GetExtension(filename)),
                            MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                        }
                        .RunModal();
                    }
                }
            });
        }

        [Export("updateTitleKeys:")]
        public void UpdateTitleKeys(NSMenuItem menuItem)
        {
            Window.BeginSheet(sheet, ProgressComplete);

            title.StringValue = "";
            message.StringValue = String.Format("Downloading from {0}", Common.TITLE_KEYS_URI);
            progress.DoubleValue = 0;

            int count = Process.keyset?.TitleKeys?.Count ?? 0;

            if (Process.updateTitleKeys())
            {
                Process.log?.WriteLine("\nFound {0} updated title keys", (Process.keyset?.TitleKeys?.Count ?? 0) - count);

                Window.EndSheet(sheet);

                new NSAlert()
                {
                    InformativeText = String.Format("Found {0} updated title keys", (Process.keyset?.TitleKeys?.Count ?? 0) - count),
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                }
                .RunModal();
            }
            else
            {
                Window.EndSheet(sheet);

                new NSAlert()
                {
                    InformativeText = "Failed to download title keys",
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                }
                .RunModal();
            }
        }

        [Export("updateVersionList:")]
        public void UpdateVersionList(NSMenuItem menuItem)
        {
            Window.BeginSheet(sheet, ProgressComplete);

            title.StringValue = "";
            message.StringValue = String.Format("Downloading from {0}", Common.HAC_VERSIONLIST_URI);
            progress.DoubleValue = 0;

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
                    tableView.ReloadData();

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

                    while (historyMenu.Items.Length > Common.HISTORY_SIZE)
                    {
                        historyMenu.RemoveItemAt(0);
                    }
                }

                Process.log?.WriteLine("\n{0} titles have updated version", count);

                Window.EndSheet(sheet);

                new NSAlert()
                {
                    InformativeText = String.Format("{0} titles have updated version", count),
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                }
                .RunModal();
            }
            else
            {
                Window.EndSheet(sheet);

                new NSAlert()
                {
                    InformativeText = "Failed to download version list",
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                }
                .RunModal();
            }
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

        void History(object sender, EventArgs e)
        {
            tableViewDataSource.Titles.Clear();
            Process.latestVersions.Clear();

            int index = 0;
            foreach (NSMenuItem item in historyMenu.Items)
            {
                item.State = item == sender ? NSCellStateValue.On : NSCellStateValue.Off;

                if (item == sender)
                {
                    titles = Process.processHistory(index);

                    tableViewDataSource.Titles.AddRange(titles);
                    tableView.ReloadData();
                }

                index++;
            }
        }

        void InitContextMenu(NSMenu menu)
        {
            foreach (string property in Title.Properties)
            {
                NSMenuItem menuItem = new NSMenuItem(property, new System.EventHandler(Copy));
                menu.AddItem(menuItem);
            }
        }

        [Export("copy:")]
        public void Copy(NSMenuItem menuItem)
        {
            Copy(menuItem, new EventArgs());
        }

        void Copy(object sender, EventArgs e)
        {
            List<string> text = new List<string>();

            string property = (sender as NSMenuItem).Title;
            bool allColumns = property == "All Columns";

            tableView.SelectedRows.EnumerateIndexes((nuint idx, ref bool stop) =>
            {
                Title title = tableViewDataSource.Titles?[(int)idx];
                if (title != null)
                {
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
            });

            if (allColumns)
            {
                text = text.Select((x, i) => new { x, i }).GroupBy(x => x.i / Title.Properties.Count()).Select(x => String.Join("\t", x.Select(t => t.x))).ToList();
            }

            if (text.Any() && !text.All(x => String.IsNullOrEmpty(x)))
            {
                var pboard = NSPasteboard.GeneralPasteboard;
                pboard.DeclareTypes(new string[] { NSPasteboard.NSPasteboardTypeString }, null);
                pboard.SetStringForType(String.Join("\n", text), NSPasteboard.NSPasteboardTypeString);
            }
            else
            {
                new NSAlert()
                {
                    InformativeText = String.Format("{0} is empty", property),
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                }
                .RunModal();
            }
        }

        [Export("rename:")]
        public void Rename(NSMenuItem menuItem)
        {
            int duplicateCount = 0, existingCount = 0, missingCount = 0;

            List<Tuple<string, string>> renameList = new List<Tuple<string, string>>();

            tableView.SelectedRows.EnumerateIndexes((nuint idx, ref bool stop) =>
            {
                Title title = tableViewDataSource.Titles?[(int)idx];
                if (title != null)
                {
                    string filename = Path.GetFullPath(title.filename);
                    string newname = Path.Combine(Path.GetDirectoryName(filename), Regex.Replace(title.titleName,
                        String.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))), "") + " [" + title.titleID + "][v" + title.version + "]" + Path.GetExtension(filename));

                    if (filename == newname)
                    {
                        duplicateCount++;
                        Process.log?.WriteLine("Skipping file \"{0}\": The source and destination file names are the same", filename);
                    }
                    else
                    {
                        if (File.Exists(filename))
                        {
                            if (File.Exists(newname))
                            {
                                existingCount++;
                                Process.log?.WriteLine("Skipping file \"{0}\": There is already a file with the same name", filename);
                            }
                            else
                            {
                                renameList.Add(Tuple.Create(filename, newname));
                            }
                        }
                        else
                        {
                            missingCount++;
                            Process.log?.WriteLine("Skipping file \"{0}\": The source file could not be found", filename);
                        }
                    }
                }
            });

            int selectedCount = (int)tableView.SelectedRowCount;

            if (duplicateCount + existingCount + missingCount == selectedCount)
            {
                string message;

                if (duplicateCount == selectedCount)
                {
                    message = "The selected files do not need renaming";
                }
                else if (existingCount == selectedCount)
                {
                    message = "The files of the same names already exist";
                }
                else if (missingCount == selectedCount)
                {
                    message = "The selected files could not be found";
                }
                else
                {
                    message = String.Join("\n", new string[]
                    {
                            duplicateCount > 0 ? String.Format("{0} files do not need renaming", duplicateCount) : "",
                            existingCount > 0 ? String.Format("{0} files of the same names already exist", existingCount) : "",
                            missingCount > 0 ? String.Format("{0} files could not be found", missingCount) : "",
                    }
                    .Where(x => !String.IsNullOrEmpty(x)));
                }

                new NSAlert()
                {
                    InformativeText = message,
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                }
                .RunModal();
            }
            else
            {
                int renameCount = renameList.Count();
                bool confirm = true;

                int index = historyMenu.Items.Select((item, i) => new { item, i }).FirstOrDefault(x => x.item.State == NSCellStateValue.On)?.i ?? -1;

                foreach (Tuple<string, string> rename in renameList)
                {
                    string filename = rename.Item1;
                    string newname = rename.Item2;

                    if (confirm)
                    {
                        if (renameCount > 1)
                        {
                            var alert = new NSAlert()
                            {
                                InformativeText = String.Format("{0} files will be renamed following this naming convention\n\n\"{1}\" to \"{2}\"\n\nDo you wish to continue renaming?", renameCount, filename, newname),
                                MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                            };
                            alert.AddButton("OK");
                            alert.AddButton("Cancel");
                            if (alert.RunModal() != (int)NSAlertButtonReturn.First)
                            {
                                return;
                            }
                        }
                        else
                        {
                            var alert = new NSAlert()
                            {
                                InformativeText = String.Format("\"{0}\" will be renamed to \"{1}\"\n\nDo you wish to continue renaming?", filename, newname),
                                MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                            };
                            alert.AddButton("OK");
                            alert.AddButton("Cancel");
                            if (alert.RunModal() != (int)NSAlertButtonReturn.First)
                            {
                                return;
                            }
                        }
                    }

                    confirm = false;

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
                        renameCount--;

                        Process.log?.WriteLine(ex.StackTrace);
                        Process.log?.WriteLine("Failed to rename file \"{0}\"", filename);
                    }
                }

                Common.History.Default.Save();

                tableView.ReloadData();

                string message = String.Join("\n", new string[]
                {
                    String.Format("{0} of {1} files renamed", renameCount, selectedCount),
                    duplicateCount > 0 ? String.Format("{0} files do not need renaming", duplicateCount) : "",
                    existingCount > 0 ? String.Format("{0} files of the same names already exist", existingCount) : "",
                    missingCount > 0 ? String.Format("{0} files could not be found", missingCount) : "",
                }
                .Where(x => !String.IsNullOrEmpty(x)));

                new NSAlert()
                {
                    InformativeText = message,
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                }
                .RunModal();
            }
        }

        [Export("showInFinder:")]
        public void ShowInFinder(NSMenuItem menuItem)
        {
            List<NSUrl> files = new List<NSUrl>();

            tableView.SelectedRows.EnumerateIndexes((nuint idx, ref bool stop) =>
            {
                Title title = tableViewDataSource.Titles?[(int)idx];
                if (title != null)
                {
                    if (File.Exists(title.filename))
                    {
                        files.Add(NSUrl.FromFilename(title.filename));
                    }
                }
            });

            if (files.Any())
            {
                NSWorkspace.SharedWorkspace.ActivateFileViewer(files.ToArray());
            }
            else if (tableView.SelectedRows.Any())
            {
                new NSAlert()
                {
                    InformativeText = String.Format("The selected files could not be found"),
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                }
                .RunModal();
            }
            else
            {
                new NSAlert()
                {
                    InformativeText = String.Format("There is no title selected"),
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                }
                .RunModal();
            }
        }

        void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            titles.Clear();
            Process.latestVersions.Clear();

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

                foreach (NSMenuItem item in historyMenu.Items)
                {
                    item.State = NSCellStateValue.Off;
                }

                NSMenuItem menuItem = new NSMenuItem(String.Format("{0} ({1} files)", history.description, history.title.Count), new System.EventHandler(History))
                {
                    State = NSCellStateValue.On,
                };
                historyMenu.AddItem(menuItem);

                while (historyMenu.Items.Length > Common.HISTORY_SIZE)
                {
                    historyMenu.RemoveItemAt(0);
                }

                Window.EndSheet(sheet);
            }
            else if (e.Result is string error)
            {
                Window.EndSheet(sheet);

                new NSAlert()
                {
                    InformativeText = String.Format("{0}.", error),
                    MessageText = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleExecutable").ToString(),
                }
                .RunModal();
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
        internal MainWindowController viewController;

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
                        case "baseTitleID":
                            return string.Compare(x.baseTitleID, y.baseTitleID) * (sortDescriptor.Ascending ? 1 : -1);
                        case "titleName":
                            return string.Compare(x.titleName, y.titleName) * (sortDescriptor.Ascending ? 1 : -1);
                        case "displayVersion":
                            if (!Version.TryParse(x.displayVersion, out Version verx))
                            {
                                verx = new Version();
                            }
                            if (!Version.TryParse(y.displayVersion, out Version very))
                            {
                                very = new Version();
                            }
                            return verx.CompareTo(very) * (sortDescriptor.Ascending ? 1 : -1);
                        case "version":
                            return x.version.CompareTo(y.version) * (sortDescriptor.Ascending ? 1 : -1);
                        case "latestVersion":
                            return x.latestVersion.CompareTo(y.latestVersion) * (sortDescriptor.Ascending ? 1 : -1);
                        case "systemUpdate":
                            return x.systemUpdate.CompareTo(y.systemUpdate) * (sortDescriptor.Ascending ? 1 : -1);
                        case "systemVersion":
                            return x.systemVersion.CompareTo(y.systemVersion) * (sortDescriptor.Ascending ? 1 : -1);
                        case "applicationVersion":
                            return x.applicationVersion.CompareTo(y.applicationVersion) * (sortDescriptor.Ascending ? 1 : -1);
                        case "masterkey":
                            return x.masterkey.CompareTo(y.masterkey) * (sortDescriptor.Ascending ? 1 : -1);
                        case "titleKey":
                            return string.Compare(x.titleKey, y.titleKey) * (sortDescriptor.Ascending ? 1 : -1);
                        case "publisher":
                            return string.Compare(x.publisher, y.publisher) * (sortDescriptor.Ascending ? 1 : -1);
                        case "languagesString":
                            return string.Compare(x.languagesString, y.languagesString) * (sortDescriptor.Ascending ? 1 : -1);
                        case "filename":
                            return string.Compare(x.filename, y.filename) * (sortDescriptor.Ascending ? 1 : -1);
                        case "filesize":
                            return x.filesize.CompareTo(y.filesize) * (sortDescriptor.Ascending ? 1 : -1);
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

        [Export("tableView:validateDrop:proposedRow:proposedDropOperation:")]
        public override NSDragOperation ValidateDrop(NSTableView tableView, NSDraggingInfo info, nint row, NSTableViewDropOperation dropOperation)
        {
            return NSDragOperation.Move;
        }

        [Export("tableView:acceptDrop:row:dropOperation:")]
        public override bool AcceptDrop(NSTableView tableView, NSDraggingInfo info, nint row, NSTableViewDropOperation dropOperation)
        {
            NSPasteboard pasteboard = info.DraggingPasteboard;
            if (Array.IndexOf(pasteboard.Types, "NSFilenamesPboardType") >= 0)
            {
                NSPasteboardItem[] pasteboardItems = pasteboard.PasteboardItems;
                string[] files = pasteboardItems.Select(x => new NSUrl(x.GetStringForType("public.file-url")).Path).ToArray();

                if (files.Count() == 1 && Directory.Exists(files[0]))
                {
                    viewController.OpenDirectory(files[0]);
                }
                else
                {
                    viewController.OpenFile(files.Where(x => File.Exists(x)).ToList());
                }

                return true;
            }

            return false;
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
                case "BaseTitleID":
                    textField.StringValue = title.baseTitleID ?? "";
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
                case "SystemUpdate":
                    textField.StringValue = title.systemUpdateString ?? "";
                    break;
                case "SystemVersion":
                    textField.StringValue = title.systemVersionString ?? "";
                    break;
                case "ApplicationVersion":
                    textField.StringValue = title.applicationVersionString ?? "";
                    break;
                case "MasterKey":
                    textField.StringValue = title.masterkeyString ?? "";
                    break;
                case "TitleKey":
                    textField.StringValue = title.titleKey ?? "";
                    break;
                case "Publisher":
                    textField.StringValue = title.publisher ?? "";
                    break;
                case "Languages":
                    textField.StringValue = title.languagesString ?? "";
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

            string titleID = title.type == TitleType.AddOnContent ? title.titleID : title.baseTitleID ?? "";

            Process.latestVersions.TryGetValue(titleID, out uint latestVersion);
            Process.versionList.TryGetValue(titleID, out uint version);
            Process.titleVersions.TryGetValue(titleID, out uint titleVersion);

            if (latestVersion < version || latestVersion < titleVersion)
            {
                textField.BackgroundColor = title.signature != true ? NSColor.Orange.ColorWithAlphaComponent((nfloat)0.1) : NSColor.Yellow.ColorWithAlphaComponent((nfloat)0.1);
            }
            else if (title.signature != true)
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

    public static class ExcelColorExtension
    {
        public static void SetColor(this OfficeOpenXml.Style.ExcelColor excelColor, NSColor color)
        {
            NSColor rgb = color.UsingColorSpace(NSColorSpace.DeviceRGB);
            nfloat alpha = rgb.AlphaComponent;
            excelColor.SetColor(255, (int)((1 + alpha * (rgb.RedComponent - 1)) * 255), (int)((1 + alpha * (rgb.GreenComponent - 1)) * 255), (int)((1 + alpha * (rgb.BlueComponent - 1)) * 255));
        }
    }
}
