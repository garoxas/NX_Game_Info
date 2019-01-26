namespace NX_Game_Info
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openSDCardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.debugLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backgroundWorkerProcess = new System.ComponentModel.BackgroundWorker();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.objectListView = new BrightIdeasSoftware.ObjectListView();
            this.olvColumnTitleID = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnTitleName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnDisplayVersion = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnVersion = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnLatestVersion = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnFirmware = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnMasterKey = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnFileName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnFileSize = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnType = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnDistribution = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnStructure = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnSignature = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnPermission = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnError = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.objectListView)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(778, 33);
            this.menuStrip.TabIndex = 1;
            this.menuStrip.Text = "menuStrip";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFileToolStripMenuItem,
            this.openDirectoryToolStripMenuItem,
            this.openSDCardToolStripMenuItem,
            this.toolStripSeparator1,
            this.debugLogToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(50, 29);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openFileToolStripMenuItem
            // 
            this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            this.openFileToolStripMenuItem.Size = new System.Drawing.Size(217, 30);
            this.openFileToolStripMenuItem.Text = "&Open File";
            this.openFileToolStripMenuItem.Click += new System.EventHandler(this.openFileToolStripMenuItem_Click);
            // 
            // openDirectoryToolStripMenuItem
            // 
            this.openDirectoryToolStripMenuItem.Name = "openDirectoryToolStripMenuItem";
            this.openDirectoryToolStripMenuItem.Size = new System.Drawing.Size(217, 30);
            this.openDirectoryToolStripMenuItem.Text = "Open &Directory";
            this.openDirectoryToolStripMenuItem.Click += new System.EventHandler(this.openDirectoryToolStripMenuItem_Click);
            // 
            // openSDCardToolStripMenuItem
            // 
            this.openSDCardToolStripMenuItem.Name = "openSDCardToolStripMenuItem";
            this.openSDCardToolStripMenuItem.Size = new System.Drawing.Size(217, 30);
            this.openSDCardToolStripMenuItem.Text = "Open &SD Card";
            this.openSDCardToolStripMenuItem.Click += new System.EventHandler(this.openSDCardToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(214, 6);
            // 
            // debugLogToolStripMenuItem
            // 
            this.debugLogToolStripMenuItem.CheckOnClick = true;
            this.debugLogToolStripMenuItem.Name = "debugLogToolStripMenuItem";
            this.debugLogToolStripMenuItem.Size = new System.Drawing.Size(217, 30);
            this.debugLogToolStripMenuItem.Text = "Debug &Log";
            this.debugLogToolStripMenuItem.CheckedChanged += new System.EventHandler(this.debugLogToolStripMenuItem_CheckedChanged);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(214, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(217, 30);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(61, 29);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(146, 30);
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // backgroundWorkerProcess
            // 
            this.backgroundWorkerProcess.WorkerReportsProgress = true;
            this.backgroundWorkerProcess.WorkerSupportsCancellation = true;
            this.backgroundWorkerProcess.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerProcess_DoWork);
            this.backgroundWorkerProcess.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerProcess_ProgressChanged);
            this.backgroundWorkerProcess.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerProcess_RunWorkerCompleted);
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 522);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(778, 22);
            this.statusStrip.TabIndex = 2;
            this.statusStrip.Text = "statusStrip";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // objectListView
            // 
            this.objectListView.AllColumns.Add(this.olvColumnTitleID);
            this.objectListView.AllColumns.Add(this.olvColumnTitleName);
            this.objectListView.AllColumns.Add(this.olvColumnDisplayVersion);
            this.objectListView.AllColumns.Add(this.olvColumnVersion);
            this.objectListView.AllColumns.Add(this.olvColumnLatestVersion);
            this.objectListView.AllColumns.Add(this.olvColumnFirmware);
            this.objectListView.AllColumns.Add(this.olvColumnMasterKey);
            this.objectListView.AllColumns.Add(this.olvColumnFileName);
            this.objectListView.AllColumns.Add(this.olvColumnFileSize);
            this.objectListView.AllColumns.Add(this.olvColumnType);
            this.objectListView.AllColumns.Add(this.olvColumnDistribution);
            this.objectListView.AllColumns.Add(this.olvColumnStructure);
            this.objectListView.AllColumns.Add(this.olvColumnSignature);
            this.objectListView.AllColumns.Add(this.olvColumnPermission);
            this.objectListView.AllColumns.Add(this.olvColumnError);
            this.objectListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumnTitleID,
            this.olvColumnTitleName,
            this.olvColumnDisplayVersion,
            this.olvColumnVersion,
            this.olvColumnLatestVersion,
            this.olvColumnFirmware,
            this.olvColumnMasterKey,
            this.olvColumnFileName,
            this.olvColumnFileSize,
            this.olvColumnType,
            this.olvColumnDistribution,
            this.olvColumnStructure,
            this.olvColumnSignature,
            this.olvColumnPermission,
            this.olvColumnError});
            this.objectListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.objectListView.FullRowSelect = true;
            this.objectListView.GridLines = true;
            this.objectListView.Location = new System.Drawing.Point(0, 33);
            this.objectListView.Name = "objectListView";
            this.objectListView.ShowGroups = false;
            this.objectListView.Size = new System.Drawing.Size(778, 489);
            this.objectListView.TabIndex = 3;
            this.objectListView.UseCompatibleStateImageBehavior = false;
            this.objectListView.View = System.Windows.Forms.View.Details;
            // 
            // olvColumnTitleID
            // 
            this.olvColumnTitleID.AspectName = "titleID";
            this.olvColumnTitleID.CellPadding = null;
            this.olvColumnTitleID.Text = "Title ID";
            this.olvColumnTitleID.Width = 120;
            // 
            // olvColumnTitleName
            // 
            this.olvColumnTitleName.AspectName = "titleName";
            this.olvColumnTitleName.CellPadding = null;
            this.olvColumnTitleName.Text = "Title Name";
            this.olvColumnTitleName.Width = 200;
            // 
            // olvColumnDisplayVersion
            // 
            this.olvColumnDisplayVersion.AspectName = "displayVersion";
            this.olvColumnDisplayVersion.CellPadding = null;
            this.olvColumnDisplayVersion.Text = "Display Version";
            this.olvColumnDisplayVersion.Width = 100;
            // 
            // olvColumnVersion
            // 
            this.olvColumnVersion.AspectName = "versionString";
            this.olvColumnVersion.CellPadding = null;
            this.olvColumnVersion.Text = "Version";
            this.olvColumnVersion.Width = 100;
            // 
            // olvColumnLatestVersion
            // 
            this.olvColumnLatestVersion.AspectName = "latestVersionString";
            this.olvColumnLatestVersion.CellPadding = null;
            this.olvColumnLatestVersion.Text = "Latest Version";
            this.olvColumnLatestVersion.Width = 100;
            // 
            // olvColumnFirmware
            // 
            this.olvColumnFirmware.AspectName = "firmware";
            this.olvColumnFirmware.CellPadding = null;
            this.olvColumnFirmware.Text = "Firmware";
            // 
            // olvColumnMasterKey
            // 
            this.olvColumnMasterKey.AspectName = "masterkeyString";
            this.olvColumnMasterKey.CellPadding = null;
            this.olvColumnMasterKey.Text = "MasterKey";
            this.olvColumnMasterKey.Width = 100;
            // 
            // olvColumnFileName
            // 
            this.olvColumnFileName.AspectName = "filename";
            this.olvColumnFileName.CellPadding = null;
            this.olvColumnFileName.Text = "File Name";
            this.olvColumnFileName.Width = 360;
            // 
            // olvColumnFileSize
            // 
            this.olvColumnFileSize.AspectName = "filesizeString";
            this.olvColumnFileSize.CellPadding = null;
            this.olvColumnFileSize.Text = "File Size";
            // 
            // olvColumnType
            // 
            this.olvColumnType.AspectName = "typeString";
            this.olvColumnType.CellPadding = null;
            this.olvColumnType.Text = "Type";
            // 
            // olvColumnDistribution
            // 
            this.olvColumnDistribution.AspectName = "distribution";
            this.olvColumnDistribution.CellPadding = null;
            this.olvColumnDistribution.Text = "Distribution";
            this.olvColumnDistribution.Width = 80;
            // 
            // olvColumnStructure
            // 
            this.olvColumnStructure.AspectName = "structureString";
            this.olvColumnStructure.CellPadding = null;
            this.olvColumnStructure.Text = "Structure";
            this.olvColumnStructure.Width = 80;
            // 
            // olvColumnSignature
            // 
            this.olvColumnSignature.AspectName = "signatureString";
            this.olvColumnSignature.CellPadding = null;
            this.olvColumnSignature.Text = "Signature";
            // 
            // olvColumnPermission
            // 
            this.olvColumnPermission.AspectName = "permissionString";
            this.olvColumnPermission.CellPadding = null;
            this.olvColumnPermission.Text = "Permission";
            // 
            // olvColumnError
            // 
            this.olvColumnError.AspectName = "error";
            this.olvColumnError.CellPadding = null;
            this.olvColumnError.Text = "";
            this.olvColumnError.Width = 260;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(778, 544);
            this.Controls.Add(this.objectListView);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Name = "Main";
            this.Text = "NX Game Info";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.objectListView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openSDCardToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem debugLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker backgroundWorkerProcess;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private BrightIdeasSoftware.ObjectListView objectListView;
        private BrightIdeasSoftware.OLVColumn olvColumnTitleID;
        private BrightIdeasSoftware.OLVColumn olvColumnTitleName;
        private BrightIdeasSoftware.OLVColumn olvColumnDisplayVersion;
        private BrightIdeasSoftware.OLVColumn olvColumnVersion;
        private BrightIdeasSoftware.OLVColumn olvColumnLatestVersion;
        private BrightIdeasSoftware.OLVColumn olvColumnFirmware;
        private BrightIdeasSoftware.OLVColumn olvColumnMasterKey;
        private BrightIdeasSoftware.OLVColumn olvColumnFileName;
        private BrightIdeasSoftware.OLVColumn olvColumnFileSize;
        private BrightIdeasSoftware.OLVColumn olvColumnType;
        private BrightIdeasSoftware.OLVColumn olvColumnDistribution;
        private BrightIdeasSoftware.OLVColumn olvColumnStructure;
        private BrightIdeasSoftware.OLVColumn olvColumnSignature;
        private BrightIdeasSoftware.OLVColumn olvColumnPermission;
        private BrightIdeasSoftware.OLVColumn olvColumnError;
    }
}

