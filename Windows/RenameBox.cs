using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;

#pragma warning disable IDE1006 // Naming rule violation: These words must begin with upper case characters

namespace NX_Game_Info
{
    partial class RenameBox : Form
    {
        string[] formatSearch = { "{d}", "{i}", "{j}", "{n}", "{v}", "{w}" };
        string[] formatReplace = { "1.0.1", "0100001001234800", "0100001001234000", "Game Title Name", "65536", "1" };

        public RenameBox()
        {
            InitializeComponent();
            richTextBoxDefault.Text = Common.Settings.Default.Properties["RenameFormat"].DefaultValue.ToString();
            richTextApplyColor(richTextBoxDefault, false);
            textBoxFormat.SelectionBackColor = Color.Aqua;
            textBoxFormat.SelectedText = Common.Settings.Default.RenameFormat;
            textBoxFormat.Select(textBoxFormat.Text.Length, 0);
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

        private void okButton_Click(object sender, EventArgs e)
        {
            Common.Settings.Default.RenameFormat = textBoxFormat.Text;
            Common.Settings.Default.Save();
            Close();
        }

        private void addToFormat(object sender, EventArgs e)
        {
            Label lbl = sender as Label;
            Label lbl_tag = this.Controls.Find(lbl.Name.Replace("Text", ""), true).FirstOrDefault() as Label;

            //textBoxFormatInput.SelectionBackColor = lbl_tag.BackColor;
            textBoxFormat.SelectedText = lbl_tag.Text;
            //textBoxFormatInput.SelectionBackColor = Color.White;
        }

        private void labelDefault_Click(object sender, EventArgs e)
        {
            labelDefault.Focus();
            textBoxFormat.Text = Common.Settings.Default.Properties["RenameFormat"].DefaultValue.ToString();
            textBoxFormat.Select(textBoxFormat.Text.Length, 0);
        }

        private void richTextApplyColor (RichTextBox target, bool replace)
        {

            for (int key = 0; key < formatSearch.Length; ++key)
            {
                Label lbl_tag = this.Controls.Find("label" + formatSearch[key][1].ToString().ToUpper(), true).FirstOrDefault() as Label;
                MatchCollection matches = Regex.Matches(target.Text, formatSearch[key]);

                // build the preview string reversly so matched index are unchanged
                for (int k = matches.Count - 1; k >= 0; k--)
                {
                    Match match = matches[k];

                    target.Select(match.Index, match.Length);
                    target.SelectionBackColor = lbl_tag.BackColor;
                    if(replace) target.SelectedText = formatReplace[key];
                }
            }
        }

        private void textBoxFormatInput_TextChanged(object sender, EventArgs e)
        {
            int cursor = textBoxFormat.SelectionStart;

            // reset selection colors before re-apply to new string
            textBoxFormat.Select(0, textBoxFormat.Text.Length);
            textBoxFormat.SelectionBackColor = Color.White;

            // escape template with user input so we can replace and change colors later
            textBoxPreview.Text = Regex.Replace(textBoxFormat.Text, String.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))), "") + ".ext";

            richTextApplyColor(textBoxFormat, false);
            richTextApplyColor(textBoxPreview, true);

            // reset cursor to where it was
            textBoxFormat.SelectionStart = cursor;
            textBoxFormat.SelectionLength = 0;
        }
    }
}
