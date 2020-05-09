namespace NX_Game_Info
{
    partial class RenameBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            this.labelCustom = new System.Windows.Forms.Label();
            this.labelTextJ = new System.Windows.Forms.Label();
            this.labelJ = new System.Windows.Forms.Label();
            this.labelTextI = new System.Windows.Forms.Label();
            this.labelFormat = new System.Windows.Forms.Label();
            this.labelI = new System.Windows.Forms.Label();
            this.labelPreview = new System.Windows.Forms.Label();
            this.labelTextV = new System.Windows.Forms.Label();
            this.labelV = new System.Windows.Forms.Label();
            this.labelTextW = new System.Windows.Forms.Label();
            this.labelW = new System.Windows.Forms.Label();
            this.labelTextD = new System.Windows.Forms.Label();
            this.labelD = new System.Windows.Forms.Label();
            this.labelTextN = new System.Windows.Forms.Label();
            this.labelN = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.richTextBoxDefault = new System.Windows.Forms.RichTextBox();
            this.labelDefault = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.textBoxFormat = new System.Windows.Forms.RichTextBox();
            this.textBoxPreview = new System.Windows.Forms.RichTextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelCustom
            // 
            this.labelCustom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelCustom.AutoSize = true;
            this.labelCustom.Location = new System.Drawing.Point(3, 119);
            this.labelCustom.Name = "labelCustom";
            this.labelCustom.Size = new System.Drawing.Size(93, 13);
            this.labelCustom.TabIndex = 44;
            this.labelCustom.Text = "Customize Format:";
            // 
            // labelTextJ
            // 
            this.labelTextJ.AutoSize = true;
            this.labelTextJ.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelTextJ.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline);
            this.labelTextJ.Location = new System.Drawing.Point(40, 23);
            this.labelTextJ.Margin = new System.Windows.Forms.Padding(2, 3, 0, 0);
            this.labelTextJ.Name = "labelTextJ";
            this.labelTextJ.Size = new System.Drawing.Size(68, 13);
            this.labelTextJ.TabIndex = 31;
            this.labelTextJ.Text = "Base Title ID";
            this.labelTextJ.Click += new System.EventHandler(this.addToFormat);
            // 
            // labelJ
            // 
            this.labelJ.AutoSize = true;
            this.labelJ.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.labelJ.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelJ.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold);
            this.labelJ.Location = new System.Drawing.Point(6, 21);
            this.labelJ.Margin = new System.Windows.Forms.Padding(2, 1, 0, 0);
            this.labelJ.Name = "labelJ";
            this.labelJ.Padding = new System.Windows.Forms.Padding(2, 2, 0, 3);
            this.labelJ.Size = new System.Drawing.Size(30, 19);
            this.labelJ.TabIndex = 30;
            this.labelJ.Text = "{j}";
            this.labelJ.Click += new System.EventHandler(this.addToFormat);
            // 
            // labelTextI
            // 
            this.labelTextI.AutoSize = true;
            this.labelTextI.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelTextI.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline);
            this.labelTextI.Location = new System.Drawing.Point(40, 3);
            this.labelTextI.Margin = new System.Windows.Forms.Padding(2, 3, 0, 0);
            this.labelTextI.Name = "labelTextI";
            this.labelTextI.Size = new System.Drawing.Size(41, 13);
            this.labelTextI.TabIndex = 29;
            this.labelTextI.Text = "Title ID";
            this.labelTextI.Click += new System.EventHandler(this.addToFormat);
            // 
            // labelFormat
            // 
            this.labelFormat.AutoSize = true;
            this.labelFormat.Location = new System.Drawing.Point(3, 3);
            this.labelFormat.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.labelFormat.Name = "labelFormat";
            this.labelFormat.Size = new System.Drawing.Size(42, 13);
            this.labelFormat.TabIndex = 26;
            this.labelFormat.Text = "Format:";
            // 
            // labelI
            // 
            this.labelI.AutoSize = true;
            this.labelI.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.labelI.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelI.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold);
            this.labelI.Location = new System.Drawing.Point(6, 1);
            this.labelI.Margin = new System.Windows.Forms.Padding(2, 1, 0, 0);
            this.labelI.Name = "labelI";
            this.labelI.Padding = new System.Windows.Forms.Padding(2, 2, 0, 3);
            this.labelI.Size = new System.Drawing.Size(30, 19);
            this.labelI.TabIndex = 27;
            this.labelI.Text = "{i}";
            this.labelI.Click += new System.EventHandler(this.addToFormat);
            // 
            // labelPreview
            // 
            this.labelPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelPreview.AutoSize = true;
            this.labelPreview.Location = new System.Drawing.Point(3, 169);
            this.labelPreview.Name = "labelPreview";
            this.labelPreview.Size = new System.Drawing.Size(48, 13);
            this.labelPreview.TabIndex = 41;
            this.labelPreview.Text = "Preview:";
            // 
            // labelTextV
            // 
            this.labelTextV.AutoSize = true;
            this.labelTextV.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelTextV.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline);
            this.labelTextV.Location = new System.Drawing.Point(332, 3);
            this.labelTextV.Margin = new System.Windows.Forms.Padding(2, 3, 0, 0);
            this.labelTextV.Name = "labelTextV";
            this.labelTextV.Size = new System.Drawing.Size(42, 13);
            this.labelTextV.TabIndex = 37;
            this.labelTextV.Text = "Version";
            this.labelTextV.Click += new System.EventHandler(this.addToFormat);
            // 
            // labelV
            // 
            this.labelV.AutoSize = true;
            this.labelV.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.labelV.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelV.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold);
            this.labelV.Location = new System.Drawing.Point(298, 1);
            this.labelV.Margin = new System.Windows.Forms.Padding(2, 1, 0, 0);
            this.labelV.Name = "labelV";
            this.labelV.Padding = new System.Windows.Forms.Padding(2, 2, 0, 3);
            this.labelV.Size = new System.Drawing.Size(30, 19);
            this.labelV.TabIndex = 38;
            this.labelV.Text = "{v}";
            this.labelV.Click += new System.EventHandler(this.addToFormat);
            // 
            // labelTextW
            // 
            this.labelTextW.AutoSize = true;
            this.labelTextW.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelTextW.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline);
            this.labelTextW.Location = new System.Drawing.Point(332, 23);
            this.labelTextW.Margin = new System.Windows.Forms.Padding(2, 3, 0, 0);
            this.labelTextW.Name = "labelTextW";
            this.labelTextW.Size = new System.Drawing.Size(76, 13);
            this.labelTextW.TabIndex = 35;
            this.labelTextW.Text = "Simple Version";
            this.labelTextW.Click += new System.EventHandler(this.addToFormat);
            // 
            // labelW
            // 
            this.labelW.AutoSize = true;
            this.labelW.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.labelW.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelW.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold);
            this.labelW.Location = new System.Drawing.Point(298, 21);
            this.labelW.Margin = new System.Windows.Forms.Padding(2, 1, 0, 0);
            this.labelW.Name = "labelW";
            this.labelW.Padding = new System.Windows.Forms.Padding(2, 2, 0, 3);
            this.labelW.Size = new System.Drawing.Size(30, 19);
            this.labelW.TabIndex = 36;
            this.labelW.Text = "{w}";
            this.labelW.Click += new System.EventHandler(this.addToFormat);
            // 
            // labelTextD
            // 
            this.labelTextD.AutoSize = true;
            this.labelTextD.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelTextD.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline);
            this.labelTextD.Location = new System.Drawing.Point(186, 23);
            this.labelTextD.Margin = new System.Windows.Forms.Padding(2, 3, 0, 0);
            this.labelTextD.Name = "labelTextD";
            this.labelTextD.Size = new System.Drawing.Size(79, 13);
            this.labelTextD.TabIndex = 43;
            this.labelTextD.Text = "Display Version";
            this.labelTextD.Click += new System.EventHandler(this.addToFormat);
            // 
            // labelD
            // 
            this.labelD.AutoSize = true;
            this.labelD.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.labelD.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelD.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold);
            this.labelD.Location = new System.Drawing.Point(152, 21);
            this.labelD.Margin = new System.Windows.Forms.Padding(2, 1, 0, 0);
            this.labelD.Name = "labelD";
            this.labelD.Padding = new System.Windows.Forms.Padding(2, 2, 0, 3);
            this.labelD.Size = new System.Drawing.Size(30, 19);
            this.labelD.TabIndex = 42;
            this.labelD.Text = "{d}";
            this.labelD.Click += new System.EventHandler(this.addToFormat);
            // 
            // labelTextN
            // 
            this.labelTextN.AutoSize = true;
            this.labelTextN.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelTextN.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline);
            this.labelTextN.Location = new System.Drawing.Point(186, 3);
            this.labelTextN.Margin = new System.Windows.Forms.Padding(2, 3, 0, 0);
            this.labelTextN.Name = "labelTextN";
            this.labelTextN.Size = new System.Drawing.Size(58, 13);
            this.labelTextN.TabIndex = 33;
            this.labelTextN.Text = "Title Name";
            this.labelTextN.Click += new System.EventHandler(this.addToFormat);
            // 
            // labelN
            // 
            this.labelN.AutoSize = true;
            this.labelN.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.labelN.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelN.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold);
            this.labelN.Location = new System.Drawing.Point(152, 1);
            this.labelN.Margin = new System.Windows.Forms.Padding(2, 1, 0, 0);
            this.labelN.Name = "labelN";
            this.labelN.Padding = new System.Windows.Forms.Padding(2, 2, 0, 3);
            this.labelN.Size = new System.Drawing.Size(30, 19);
            this.labelN.TabIndex = 32;
            this.labelN.Text = "{n}";
            this.labelN.Click += new System.EventHandler(this.addToFormat);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.richTextBoxDefault);
            this.panel1.Controls.Add(this.labelDefault);
            this.panel1.Location = new System.Drawing.Point(0, 78);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(450, 39);
            this.panel1.TabIndex = 46;
            // 
            // richTextBoxDefault
            // 
            this.richTextBoxDefault.BackColor = System.Drawing.SystemColors.Control;
            this.richTextBoxDefault.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxDefault.Cursor = System.Windows.Forms.Cursors.Hand;
            this.richTextBoxDefault.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold);
            this.richTextBoxDefault.Location = new System.Drawing.Point(79, 2);
            this.richTextBoxDefault.Name = "richTextBoxDefault";
            this.richTextBoxDefault.Size = new System.Drawing.Size(295, 16);
            this.richTextBoxDefault.TabIndex = 46;
            this.richTextBoxDefault.Text = "";
            this.richTextBoxDefault.Click += new System.EventHandler(this.labelDefault_Click);
            // 
            // labelDefault
            // 
            this.labelDefault.AutoSize = true;
            this.labelDefault.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelDefault.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline);
            this.labelDefault.Location = new System.Drawing.Point(3, 2);
            this.labelDefault.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.labelDefault.Name = "labelDefault";
            this.labelDefault.Size = new System.Drawing.Size(76, 13);
            this.labelDefault.TabIndex = 39;
            this.labelDefault.Text = "Default format:";
            this.labelDefault.Click += new System.EventHandler(this.labelDefault_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(293, 229);
            this.button1.Margin = new System.Windows.Forms.Padding(0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 24);
            this.button1.TabIndex = 25;
            this.button1.Text = "&Cancel";
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.okButton.Location = new System.Drawing.Point(377, 229);
            this.okButton.Margin = new System.Windows.Forms.Padding(0);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 24);
            this.okButton.TabIndex = 24;
            this.okButton.Text = "&OK";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // textBoxFormat
            // 
            this.textBoxFormat.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxFormat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxFormat.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold);
            this.textBoxFormat.Location = new System.Drawing.Point(4, 4);
            this.textBoxFormat.Multiline = false;
            this.textBoxFormat.Name = "textBoxFormat";
            this.textBoxFormat.Size = new System.Drawing.Size(428, 18);
            this.textBoxFormat.TabIndex = 26;
            this.textBoxFormat.Text = "";
            this.textBoxFormat.TextChanged += new System.EventHandler(this.textBoxFormatInput_TextChanged);
            // 
            // textBoxPreview
            // 
            this.textBoxPreview.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxPreview.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold);
            this.textBoxPreview.Location = new System.Drawing.Point(4, 4);
            this.textBoxPreview.Multiline = false;
            this.textBoxPreview.Name = "textBoxPreview";
            this.textBoxPreview.ReadOnly = true;
            this.textBoxPreview.Size = new System.Drawing.Size(428, 18);
            this.textBoxPreview.TabIndex = 48;
            this.textBoxPreview.Text = "";
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.White;
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.textBoxFormat);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(6, 135);
            this.panel3.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.panel3.Size = new System.Drawing.Size(438, 24);
            this.panel3.TabIndex = 26;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 126F));
            this.tableLayoutPanel1.Controls.Add(this.labelI, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelTextJ, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.labelTextI, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelW, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.labelTextW, 5, 1);
            this.tableLayoutPanel1.Controls.Add(this.labelJ, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.labelN, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelTextD, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.labelTextN, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelD, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.labelV, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelTextV, 5, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 18);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(450, 60);
            this.tableLayoutPanel1.TabIndex = 27;
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel.ColumnCount = 1;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Controls.Add(this.panel3, 0, 4);
            this.tableLayoutPanel.Controls.Add(this.labelCustom, 0, 3);
            this.tableLayoutPanel.Controls.Add(this.labelFormat, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.tableLayoutPanel1, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.panel1, 0, 2);
            this.tableLayoutPanel.Controls.Add(this.labelPreview, 0, 5);
            this.tableLayoutPanel.Controls.Add(this.panel4, 0, 6);
            this.tableLayoutPanel.Location = new System.Drawing.Point(7, 8);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 7;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(450, 212);
            this.tableLayoutPanel.TabIndex = 1;
            // 
            // panel4
            // 
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.textBoxPreview);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(6, 185);
            this.panel4.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.panel4.Name = "panel4";
            this.panel4.Padding = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.panel4.Size = new System.Drawing.Size(438, 24);
            this.panel4.TabIndex = 47;
            // 
            // RenameBox
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 265);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tableLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RenameBox";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "File Rename Format";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label labelFormat;
        private System.Windows.Forms.Label labelN;
        private System.Windows.Forms.Label labelTextJ;
        private System.Windows.Forms.Label labelJ;
        private System.Windows.Forms.Label labelTextI;
        private System.Windows.Forms.Label labelI;
        private System.Windows.Forms.Label labelV;
        private System.Windows.Forms.Label labelTextV;
        private System.Windows.Forms.Label labelW;
        private System.Windows.Forms.Label labelTextW;
        private System.Windows.Forms.Label labelTextN;
        private System.Windows.Forms.Label labelPreview;
        private System.Windows.Forms.Label labelTextD;
        private System.Windows.Forms.Label labelD;
        private System.Windows.Forms.Label labelCustom;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelDefault;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RichTextBox textBoxFormat;
        private System.Windows.Forms.RichTextBox textBoxPreview;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.RichTextBox richTextBoxDefault;
    }
}
