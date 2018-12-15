using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Foundation;
using AppKit;
using Title = NX_Game_Info.Common.Title;

namespace NX_Game_Info
{
    public partial class MainWindowController : NSWindowController
    {
        private TableViewDataSource tableViewDataSource;
        private TableViewDelegate tableViewDelegate;

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
        }

        [Export("openDocument:")]
        public void OpenFile(NSMenuItem menuItem)
        {
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

                foreach (var filename in filenames)
                {
                    Title title = Process.processFile(filename);
                    if (title != null)
                    {
                        tableViewDataSource.Titles.Add(title);
                    }
                }

                tableView.ReloadData();
            }
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
                    else
                    {
                        return string.Compare(x.titleID, y.titleID) * (sortDescriptor.Ascending ? 1 : -1);
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
                textField.StringValue = title.titleID;
            }

            return textField;
        }
    }
}
