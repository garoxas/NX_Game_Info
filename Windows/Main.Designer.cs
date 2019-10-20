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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openSDCardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateTitleKeysToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateVersionListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.debugLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.historyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.openFileLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.objectListView = new BrightIdeasSoftware.ObjectListView();
            this.olvColumnTitleID = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnBaseTitleID = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnTitleName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnDisplayVersion = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnVersion = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnLatestVersion = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnSystemUpdate = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnSystemVersion = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnApplicationVersion = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnMasterKey = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnTitleKey = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnPublisher = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnLanguages = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnFileName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnFileSize = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnType = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnDistribution = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnStructure = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnSignature = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnPermission = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumnError = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.backgroundWorkerProcess = new System.ComponentModel.BackgroundWorker();
            this.menuStrip.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.objectListView)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.historyToolStripMenuItem,
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
            this.exportToolStripMenuItem,
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
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(217, 30);
            this.exportToolStripMenuItem.Text = "&Export";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
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
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.updateTitleKeysToolStripMenuItem,
            this.updateVersionListToolStripMenuItem,
            this.toolStripSeparator3,
            this.debugLogToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(88, 29);
            this.optionsToolStripMenuItem.Text = "&Options";
            // 
            // updateTitleKeysToolStripMenuItem
            // 
            this.updateTitleKeysToolStripMenuItem.Name = "updateTitleKeysToolStripMenuItem";
            this.updateTitleKeysToolStripMenuItem.Size = new System.Drawing.Size(252, 30);
            this.updateTitleKeysToolStripMenuItem.Text = "Update &Title Keys";
            this.updateTitleKeysToolStripMenuItem.Click += new System.EventHandler(this.updateTitleKeysToolStripMenuItem_Click);
            // 
            // updateVersionListToolStripMenuItem
            // 
            this.updateVersionListToolStripMenuItem.Name = "updateVersionListToolStripMenuItem";
            this.updateVersionListToolStripMenuItem.Size = new System.Drawing.Size(248, 30);
            this.updateVersionListToolStripMenuItem.Text = "Update &Version List";
            this.updateVersionListToolStripMenuItem.Click += new System.EventHandler(this.updateVersionListToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(245, 6);
            // 
            // debugLogToolStripMenuItem
            // 
            this.debugLogToolStripMenuItem.CheckOnClick = true;
            this.debugLogToolStripMenuItem.Name = "debugLogToolStripMenuItem";
            this.debugLogToolStripMenuItem.Size = new System.Drawing.Size(248, 30);
            this.debugLogToolStripMenuItem.Text = "Debug &Log";
            this.debugLogToolStripMenuItem.CheckedChanged += new System.EventHandler(this.debugLogToolStripMenuItem_CheckedChanged);
            // 
            // historyToolStripMenuItem
            // 
            this.historyToolStripMenuItem.Name = "historyToolStripMenuItem";
            this.historyToolStripMenuItem.Size = new System.Drawing.Size(81, 29);
            this.historyToolStripMenuItem.Text = "His&tory";
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
            // contextMenuStrip
            // 
            this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.toolStripSeparator4,
            this.openFileLocationToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(225, 70);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(224, 30);
            this.copyToolStripMenuItem.Text = "&Copy";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(221, 6);
            // 
            // openFileLocationToolStripMenuItem
            // 
            this.openFileLocationToolStripMenuItem.Name = "openFileLocationToolStripMenuItem";
            this.openFileLocationToolStripMenuItem.Size = new System.Drawing.Size(224, 30);
            this.openFileLocationToolStripMenuItem.Text = "Open file locat&ion";
            this.openFileLocationToolStripMenuItem.Click += new System.EventHandler(this.openFileLocationToolStripMenuItem_Click);
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
            this.objectListView.AllColumns.Add(this.olvColumnBaseTitleID);
            this.objectListView.AllColumns.Add(this.olvColumnTitleName);
            this.objectListView.AllColumns.Add(this.olvColumnDisplayVersion);
            this.objectListView.AllColumns.Add(this.olvColumnVersion);
            this.objectListView.AllColumns.Add(this.olvColumnLatestVersion);
            this.objectListView.AllColumns.Add(this.olvColumnSystemUpdate);
            this.objectListView.AllColumns.Add(this.olvColumnSystemVersion);
            this.objectListView.AllColumns.Add(this.olvColumnApplicationVersion);
            this.objectListView.AllColumns.Add(this.olvColumnMasterKey);
            this.objectListView.AllColumns.Add(this.olvColumnTitleKey);
            this.objectListView.AllColumns.Add(this.olvColumnPublisher);
            this.objectListView.AllColumns.Add(this.olvColumnLanguages);
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
            this.olvColumnBaseTitleID,
            this.olvColumnTitleName,
            this.olvColumnDisplayVersion,
            this.olvColumnVersion,
            this.olvColumnLatestVersion,
            this.olvColumnSystemUpdate,
            this.olvColumnSystemVersion,
            this.olvColumnApplicationVersion,
            this.olvColumnMasterKey,
            this.olvColumnTitleKey,
            this.olvColumnPublisher,
            this.olvColumnLanguages,
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
            this.objectListView.HideSelection = false;
            this.objectListView.Location = new System.Drawing.Point(0, 33);
            this.objectListView.Name = "objectListView";
            this.objectListView.ShowGroups = false;
            this.objectListView.Size = new System.Drawing.Size(778, 489);
            this.objectListView.TabIndex = 3;
            this.objectListView.UseCompatibleStateImageBehavior = false;
            this.objectListView.View = System.Windows.Forms.View.Details;
            this.objectListView.CellRightClick += new System.EventHandler<BrightIdeasSoftware.CellRightClickEventArgs>(this.objectListView_CellRightClick);
            this.objectListView.Freezing += new System.EventHandler<BrightIdeasSoftware.FreezeEventArgs>(this.objectListView_Freezing);
            // 
            // olvColumnTitleID
            // 
            this.olvColumnTitleID.AspectName = "titleID";
            this.olvColumnTitleID.CellPadding = null;
            this.olvColumnTitleID.Hideable = false;
            this.olvColumnTitleID.Text = "Title ID";
            this.olvColumnTitleID.Width = 120;
            // 
            // olvColumnBaseTitleID
            // 
            this.olvColumnBaseTitleID.AspectName = "baseTitleID";
            this.olvColumnBaseTitleID.CellPadding = null;
            this.olvColumnBaseTitleID.Text = "Base Title ID";
            this.olvColumnBaseTitleID.Width = 120;
            // 
            // olvColumnTitleName
            // 
            this.olvColumnTitleName.AspectName = "titleName";
            this.olvColumnTitleName.CellPadding = null;
            this.olvColumnTitleName.Hideable = false;
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
            // olvColumnSystemUpdate
            // 
            this.olvColumnSystemUpdate.AspectName = "systemUpdateString";
            this.olvColumnSystemUpdate.CellPadding = null;
            this.olvColumnSystemUpdate.Text = "System Update";
            this.olvColumnSystemUpdate.Width = 100;
            // 
            // olvColumnSystemVersion
            // 
            this.olvColumnSystemVersion.AspectName = "systemVersionString";
            this.olvColumnSystemVersion.CellPadding = null;
            this.olvColumnSystemVersion.Text = "System Version";
            this.olvColumnSystemVersion.Width = 100;
            // 
            // olvColumnApplicationVersion
            // 
            this.olvColumnApplicationVersion.AspectName = "applicationVersionString";
            this.olvColumnApplicationVersion.CellPadding = null;
            this.olvColumnApplicationVersion.Text = "Application Version";
            this.olvColumnApplicationVersion.Width = 100;
            // 
            // olvColumnMasterKey
            // 
            this.olvColumnMasterKey.AspectName = "masterkeyString";
            this.olvColumnMasterKey.CellPadding = null;
            this.olvColumnMasterKey.Text = "MasterKey";
            this.olvColumnMasterKey.Width = 100;
            // 
            // olvColumnTitleKey
            // 
            this.olvColumnTitleKey.AspectName = "titleKey";
            this.olvColumnTitleKey.CellPadding = null;
            this.olvColumnTitleKey.Text = "Title Key";
            this.olvColumnTitleKey.Width = 240;
            // 
            // olvColumnPublisher
            // 
            this.olvColumnPublisher.AspectName = "publisher";
            this.olvColumnPublisher.CellPadding = null;
            this.olvColumnPublisher.Text = "Publisher";
            this.olvColumnPublisher.Width = 200;
            // 
            // olvColumnLanguages
            // 
            this.olvColumnLanguages.AspectName = "languagesString";
            this.olvColumnLanguages.CellPadding = null;
            this.olvColumnLanguages.Text = "Languages";
            this.olvColumnLanguages.Width = 120;
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
            this.olvColumnError.Hideable = false;
            this.olvColumnError.Text = "";
            this.olvColumnError.Width = 260;
            // 
            // backgroundWorkerProcess
            // 
            this.backgroundWorkerProcess.WorkerReportsProgress = true;
            this.backgroundWorkerProcess.WorkerSupportsCancellation = true;
            this.backgroundWorkerProcess.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerProcess_DoWork);
            this.backgroundWorkerProcess.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerProcess_ProgressChanged);
            this.backgroundWorkerProcess.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerProcess_RunWorkerCompleted);
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
            this.contextMenuStrip.ResumeLayout(false);
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
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateTitleKeysToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateVersionListToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem debugLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem historyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem openFileLocationToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private BrightIdeasSoftware.ObjectListView objectListView;
        private BrightIdeasSoftware.OLVColumn olvColumnTitleID;
        private BrightIdeasSoftware.OLVColumn olvColumnBaseTitleID;
        private BrightIdeasSoftware.OLVColumn olvColumnTitleName;
        private BrightIdeasSoftware.OLVColumn olvColumnDisplayVersion;
        private BrightIdeasSoftware.OLVColumn olvColumnVersion;
        private BrightIdeasSoftware.OLVColumn olvColumnLatestVersion;
        private BrightIdeasSoftware.OLVColumn olvColumnSystemUpdate;
        private BrightIdeasSoftware.OLVColumn olvColumnSystemVersion;
        private BrightIdeasSoftware.OLVColumn olvColumnApplicationVersion;
        private BrightIdeasSoftware.OLVColumn olvColumnMasterKey;
        private BrightIdeasSoftware.OLVColumn olvColumnTitleKey;
        private BrightIdeasSoftware.OLVColumn olvColumnPublisher;
        private BrightIdeasSoftware.OLVColumn olvColumnLanguages;
        private BrightIdeasSoftware.OLVColumn olvColumnFileName;
        private BrightIdeasSoftware.OLVColumn olvColumnFileSize;
        private BrightIdeasSoftware.OLVColumn olvColumnType;
        private BrightIdeasSoftware.OLVColumn olvColumnDistribution;
        private BrightIdeasSoftware.OLVColumn olvColumnStructure;
        private BrightIdeasSoftware.OLVColumn olvColumnSignature;
        private BrightIdeasSoftware.OLVColumn olvColumnPermission;
        private BrightIdeasSoftware.OLVColumn olvColumnError;
        private System.ComponentModel.BackgroundWorker backgroundWorkerProcess;
    }
}

