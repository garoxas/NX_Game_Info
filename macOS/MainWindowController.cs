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

            List<string> messages;
            bool init = Process.initialize(out messages);

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

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
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

                List<string> filenames = openPanel.Urls.Select((arg) => arg.Path).ToList();
                filenames.Sort();

                Common.Settings.Default.InitialDirectory = Path.GetDirectoryName(filenames.First());
                Common.Settings.Default.Save();

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

                List<string> filenames = Directory.EnumerateFiles(openPanel.Urls.First().Path, "*.*", SearchOption.AllDirectories)
                    .Where(filename => filename.ToLower().EndsWith(".xci") || filename.ToLower().EndsWith(".nsp")).ToList();
                filenames.Sort();

                Common.Settings.Default.InitialDirectory = openPanel.Urls.First().Path;
                Common.Settings.Default.Save();

                backgroundWorker.RunWorkerAsync(filenames);
            }
        }

        void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
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

        void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            tableViewDataSource.Titles.AddRange((List<Title>)e.Result);

            tableView.ReloadData();
        }

    }

    public class TableViewDataSource : NSTableViewDataSource
    {
        List<Title> titles = new List<Title>();

        internal List<Title> Titles { get { return titles; } }

        public override nint GetRowCount(NSTableView tableView)
        {
            return titles.Count;
        }

        public override void SortDescriptorsChanged(NSTableView tableView, NSSortDescriptor[] oldDescriptors)
        {
            NSSortDescriptor sortDescriptor = tableView.SortDescriptors[0];
            if (sortDescriptor != null)
            {
                titles.Sort((x, y) =>
                {
                    if (sortDescriptor.Key == "titleID")
                    {
                        return string.Compare(x.titleID, y.titleID) * (sortDescriptor.Ascending ? 1 : -1);
                    }
                    else if (sortDescriptor.Key == "titleName")
                    {
                        return string.Compare(x.titleName, y.titleName) * (sortDescriptor.Ascending ? 1 : -1);
                    }
                    else if (sortDescriptor.Key == "displayVersion")
                    {
                        return string.Compare(x.displayVersion, y.displayVersion) * (sortDescriptor.Ascending ? 1 : -1);
                    }
                    else if (sortDescriptor.Key == "versionString")
                    {
                        return string.Compare(x.versionString, y.versionString) * (sortDescriptor.Ascending ? 1 : -1);
                    }
                    else if (sortDescriptor.Key == "latestVersionString")
                    {
                        return string.Compare(x.latestVersionString, y.latestVersionString) * (sortDescriptor.Ascending ? 1 : -1);
                    }
                    else if (sortDescriptor.Key == "firmware")
                    {
                        return string.Compare(x.firmware, y.firmware) * (sortDescriptor.Ascending ? 1 : -1);
                    }
                    else if (sortDescriptor.Key == "masterkeyString")
                    {
                        return string.Compare(x.masterkeyString, y.masterkeyString) * (sortDescriptor.Ascending ? 1 : -1);
                    }
                    else if (sortDescriptor.Key == "filename")
                    {
                        return string.Compare(x.filename, y.filename) * (sortDescriptor.Ascending ? 1 : -1);
                    }
                    else if (sortDescriptor.Key == "filesizeString")
                    {
                        return (int)((x.filesize - y.filesize) * (sortDescriptor.Ascending ? 1 : -1));
                    }
                    else if (sortDescriptor.Key == "typeString")
                    {
                        return string.Compare(x.typeString, y.typeString) * (sortDescriptor.Ascending ? 1 : -1);
                    }
                    else if (sortDescriptor.Key == "distribution")
                    {
                        return string.Compare(x.distribution.ToString(), y.distribution.ToString()) * (sortDescriptor.Ascending ? 1 : -1);
                    }
                    else if (sortDescriptor.Key == "structureString")
                    {
                        return string.Compare(x.structureString, y.structureString) * (sortDescriptor.Ascending ? 1 : -1);
                    }
                    else if (sortDescriptor.Key == "signatureString")
                    {
                        return string.Compare(x.signatureString, y.signatureString) * (sortDescriptor.Ascending ? 1 : -1);
                    }
                    else if (sortDescriptor.Key == "permissionString")
                    {
                        return string.Compare(x.permissionString, y.permissionString) * (sortDescriptor.Ascending ? 1 : -1);
                    }
                    else
                    {
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

            if (tableColumn.Identifier == "TitleID")
            {
                textField.StringValue = title.titleID ?? "";
            }
            else if (tableColumn.Identifier == "TitleName")
            {
                textField.StringValue = title.titleName ?? "";
            }
            else if (tableColumn.Identifier == "DisplayVersion")
            {
                textField.StringValue = title.displayVersion ?? "";
            }
            else if (tableColumn.Identifier == "Version")
            {
                textField.StringValue = title.versionString ?? "";
            }
            else if (tableColumn.Identifier == "LatestVersion")
            {
                textField.StringValue = title.latestVersionString ?? "";
            }
            else if (tableColumn.Identifier == "Firmware")
            {
                textField.StringValue = title.firmware ?? "";
            }
            else if (tableColumn.Identifier == "MasterKey")
            {
                textField.StringValue = title.masterkeyString ?? "";
            }
            else if (tableColumn.Identifier == "FileName")
            {
                textField.StringValue = title.filename ?? "";
            }
            else if (tableColumn.Identifier == "FileSize")
            {
                textField.StringValue = NSByteCountFormatter.Format(title.filesize, NSByteCountFormatterCountStyle.File) ?? "";
            }
            else if (tableColumn.Identifier == "Type")
            {
                textField.StringValue = title.typeString ?? "";
            }
            else if (tableColumn.Identifier == "Distribution")
            {
                textField.StringValue = title.distribution.ToString() ?? "";
            }
            else if (tableColumn.Identifier == "Structure")
            {
                textField.StringValue = title.structureString ?? "";
            }
            else if (tableColumn.Identifier == "Signature")
            {
                textField.StringValue = title.signatureString ?? "";
            }
            else if (tableColumn.Identifier == "Permission")
            {
                textField.StringValue = title.permissionString ?? "";
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
