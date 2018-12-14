using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using System.Windows.Forms;
using Application = System.Windows.Forms.Application;
using BrightIdeasSoftware;
using LibHac;
using LibHac.IO;
using Newtonsoft.Json;

namespace NX_Game_Info
{
    public partial class Main : Form
    {
        private const string PROD_KEYS = "prod.keys";
        private const string TITLE_KEYS = "title.keys";
        private const string HAC_VERSIONLIST = "hac_versionlist.json";

        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern Int32 StrFormatByteSize(
            long fileSize,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer,
            int bufferSize);

        private class Title
        {
            public enum Distribution
            {
                Digital,
                Cartridge,
                Invalid = -1
            }

            public enum Structure
            {
                CnmtXml,
                CnmtNca,
                Cert,
                Tik,
                LegalinfoXml,
                NacpXml,
                PrograminfoXml,
                CardspecXml,
                AuthoringtoolinfoXml,
                RootPartition,
                UpdatePartition,
                NormalPartition,
                SecurePartition,
                LogoPartition,
                Invalid = -1
            }

            public enum Permission
            {
                Safe,
                Unsafe,
                Dangerous,
                Invalid = -1
            }

            public string titleID { get; set; }
            public string titleIDApplication { get { return String.IsNullOrEmpty(titleID) ? "" : titleID.Substring(0, Math.Min(titleID.Length, 13)) + "000"; } }
            public string titleName { get; set; }
            public string displayVersion { get; set; }
            public uint version { get; set; } = unchecked((uint)-1);
            public string versionString { get { return version != unchecked((uint)-1) ? version.ToString() : ""; } }
            public uint latestVersion { get; set; } = unchecked((uint)-1);
            public string latestVersionString { get { return latestVersion != unchecked((uint)-1) ? latestVersion.ToString() : ""; } }
            public string firmware { get; set; }
            public uint masterkey { get; set; } = 0;
            public string masterkeyString
            {
                get
                {
                    switch (masterkey)
                    {
                        case 0:
                            return masterkey.ToString() + " (1.0.0-2.3.0)";
                        case 1:
                            return masterkey.ToString() + " (3.0.0)";
                        case 2:
                            return masterkey.ToString() + " (3.0.1-3.0.2)";
                        case 3:
                            return masterkey.ToString() + " (4.0.0-4.1.0)";
                        case 4:
                            return masterkey.ToString() + " (5.0.0-5.1.0)";
                        case 5:
                            return masterkey.ToString() + " (6.0.0-6.1.0)";
                        case 6:
                            return masterkey.ToString() + " (6.2.0)";
                        default:
                            return masterkey.ToString();
                    }
                }
            }
            public string filename { get; set; }
            public long filesize { get; set; }
            public string filesizeString { get { StringBuilder builder = new StringBuilder(20); StrFormatByteSize(filesize, builder, 20); return builder.ToString(); } }
            public TitleType type { get; set; }
            public string typeString
            {
                get
                {
                    switch (type)
                    {
                        case TitleType.Application:
                            return "Base";
                        case TitleType.Patch:
                            return "Update";
                        case TitleType.AddOnContent:
                            return "DLC";
                        default:
                            return "";
                    }
                }
            }
            public Distribution distribution { get; set; } = Distribution.Invalid;
            public HashSet<Structure> structure { get; set; } = new HashSet<Structure>();
            public string structureString
            {
                get
                {
                    if (distribution == Distribution.Cartridge)
                    {
                        if (new HashSet<Structure>(new[] { Structure.UpdatePartition, Structure.NormalPartition, Structure.SecurePartition }).All(value => structure.Contains(value)))
                        {
                            return "Scene";
                        }
                        else if (new HashSet<Structure>(new[] { Structure.SecurePartition }).All(value => structure.Contains(value)))
                        {
                            return "Converted";
                        }
                        else
                        {
                            return "Not complete";
                        }
                    }
                    else if (distribution == Distribution.Digital)
                    {
                        if (new HashSet<Structure>(new[] { Structure.LegalinfoXml, Structure.NacpXml, Structure.PrograminfoXml, Structure.CardspecXml }).All(value => structure.Contains(value)))
                        {
                            return "Scene";
                        }
                        else if (new HashSet<Structure>(new[] { Structure.AuthoringtoolinfoXml }).All(value => structure.Contains(value)))
                        {
                            return "Homebrew";
                        }
                        else if (new HashSet<Structure>(new[] { Structure.Cert, Structure.Tik }).All(value => structure.Contains(value)))
                        {
                            return "CDN";
                        }
                        else if (new HashSet<Structure>(new[] { Structure.CnmtXml }).All(value => structure.Contains(value)))
                        {
                            return "Converted";
                        }
                        else
                        {
                            return "Not complete";
                        }
                    }

                    return "";
                }
            }
            public bool? signature { get; set; } = null;
            public string signatureString { get { return signature == null ? "" : (bool)signature ? "Passed" : "Not Passed"; } }
            public Permission permission { get; set; } = Permission.Invalid;
            public string permissionString { get { return permission == Permission.Invalid ? "" : permission.ToString(); } }
        }

        private class VersionTitle
        {
            public string id { get; set; }
            public uint version { get; set; }
            public uint required_version { get; set; }
        }

        private class VersionList
        {
            public List<VersionTitle> titles { get; set; }
            public uint format_version { get; set; }
            public uint last_modified { get; set; }
        }

        private Keyset keyset;
        private Dictionary<string, uint> versionList = new Dictionary<string, uint>();

        public Main()
        {
            InitializeComponent();

            if (!File.Exists(PROD_KEYS))
            {
                MessageBox.Show("File not found. Check if '" + PROD_KEYS + "' exist and try again.", Application.ProductName);
                Environment.Exit(-1);
            }

            try
            {
                keyset = ExternalKeys.ReadKeyFile(PROD_KEYS, File.Exists(TITLE_KEYS) ? TITLE_KEYS : null);
            }
            catch { }

            bool haveKakSource = !((bool)keyset?.AesKekGenerationSource.All(b => b == 0) || (bool)keyset?.AesKeyGenerationSource.All(b => b == 0) || (bool)keyset?.KeyAreaKeyApplicationSource.All(b => b == 0));
            if ((bool)keyset?.HeaderKey.All(b => b == 0) ||
                ((haveKakSource && (bool)keyset?.MasterKeys[0].All(b => b == 0)) || (bool)keyset?.KeyAreaKeys[0][0].All(b => b == 0)) || (bool)keyset?.Titlekeks[0].All(b => b == 0) ||
                ((haveKakSource && (bool)keyset?.MasterKeys[1].All(b => b == 0)) || (bool)keyset?.KeyAreaKeys[1][0].All(b => b == 0)) || (bool)keyset?.Titlekeks[1].All(b => b == 0) ||
                ((haveKakSource && (bool)keyset?.MasterKeys[2].All(b => b == 0)) || (bool)keyset?.KeyAreaKeys[2][0].All(b => b == 0)) || (bool)keyset?.Titlekeks[2].All(b => b == 0) ||
                ((haveKakSource && (bool)keyset?.MasterKeys[3].All(b => b == 0)) || (bool)keyset?.KeyAreaKeys[3][0].All(b => b == 0)) || (bool)keyset?.Titlekeks[3].All(b => b == 0) ||
                ((haveKakSource && (bool)keyset?.MasterKeys[4].All(b => b == 0)) || (bool)keyset?.KeyAreaKeys[4][0].All(b => b == 0)) || (bool)keyset?.Titlekeks[4].All(b => b == 0))
            {
                MessageBox.Show("Keyfile missing required keys. Check if these keys exist and try again.\n" +
                    "header_key, aes_kek_generation_source, aes_key_generation_source, key_area_key_application_source, master_key_00-04.", Application.ProductName);
                Environment.Exit(-1);
            }

            if (!Properties.Settings.Default.MasterKey5)
            {
                if ((haveKakSource && (bool)keyset?.MasterKeys[5].All(b => b == 0)) || (bool)keyset?.KeyAreaKeys[5][0].All(b => b == 0) || (bool)keyset?.Titlekeks[5].All(b => b == 0))
                {
                    MessageBox.Show("master_key_05, key_area_key_application_05 or titlekek_05 are missing from Keyfile.\nGames using this key may be missing or incorrect.", Application.ProductName);

                    Properties.Settings.Default.MasterKey5 = true;
                    Properties.Settings.Default.Save();
                }
            }

            if (!Properties.Settings.Default.MasterKey6)
            {
                if ((haveKakSource && (bool)keyset?.MasterKeys[6].All(b => b == 0)) || (bool)keyset?.KeyAreaKeys[6][0].All(b => b == 0) || (bool)keyset?.Titlekeks[6].All(b => b == 0))
                {
                    MessageBox.Show("master_key_06, key_area_key_application_06 or titlekek_06 are missing from Keyfile.\nGames using this key may be missing or incorrect.", Application.ProductName);

                    Properties.Settings.Default.MasterKey6 = true;
                    Properties.Settings.Default.Save();
                }
            }

            if (!Properties.Settings.Default.TitleKeys)
            {
                if (keyset?.TitleKeys.Count == 0)
                {
                    MessageBox.Show("Title Keys is missing.\nGames using Titlekey crypto may be missing or incorrect.", Application.ProductName);

                    Properties.Settings.Default.TitleKeys = true;
                    Properties.Settings.Default.Save();
                }
            }

            try
            {
                var versionlist = JsonConvert.DeserializeObject<VersionList>(File.ReadAllText(HAC_VERSIONLIST));

                foreach (var title in versionlist.titles)
                {
                    string titleID = title.id;
                    if (titleID.EndsWith("800"))
                    {
                        titleID = titleID.Substring(0, Math.Min(titleID.Length, 13)) + "000";
                    }

                    versionList.Add(titleID.ToUpper(), title.version);
                }
            }
            catch { }
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
            openFileDialog.Filter = "NX Game Files (*.xci;*.nsp)|*.xci;*.nsp|Gamecard Files (*.xci)|*.xci|Package Files (*.nsp)|*.nsp|All Files (*.*)|*.*";
            openFileDialog.Multiselect = true;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.InitialDirectory = Properties.Settings.Default.InitialDirectory;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                objectListView.Items.Clear();

                List<string> filenames = openFileDialog.FileNames.ToList();
                filenames.Sort();

                Properties.Settings.Default.InitialDirectory = Path.GetDirectoryName(filenames.First());
                Properties.Settings.Default.Save();

                backgroundWorkerProcess.RunWorkerAsync(filenames);
            }
        }

        private void openDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (backgroundWorkerProcess.IsBusy)
            {
                MessageBox.Show("Please wait until the current process is finished and try again.", Application.ProductName);
                return;
            }

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = Properties.Settings.Default.InitialDirectory;

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                objectListView.Items.Clear();

                List<string> filenames = Directory.EnumerateFiles(folderBrowserDialog.SelectedPath, "*.*", SearchOption.AllDirectories)
                    .Where(filename => filename.ToLower().EndsWith(".xci") || filename.ToLower().EndsWith(".nsp")).ToList();
                filenames.Sort();

                Properties.Settings.Default.InitialDirectory = folderBrowserDialog.SelectedPath;
                Properties.Settings.Default.Save();

                backgroundWorkerProcess.RunWorkerAsync(filenames);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(-1);
        }

        private void backgroundWorkerProcess_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            List<string> filenames = (List<string>)e.Argument;
            List<Title> titles = new List<Title>();

            foreach (var filename in filenames)
            {
                Title title = processFile(filename);
                if (title != null)
                {
                    titles.Add(title);
                }
            }

            e.Result = titles;
        }

        private void backgroundWorkerProcess_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            List<Title> titles = (List<Title>)e.Result;

            objectListView.SetObjects(titles);

            foreach (OLVListItem listItem in objectListView.Items)
            {
                Title title = listItem.RowObject as Title;

                if (title.signature != true)
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

        private Title processFile(string filename)
        {
            try
            {
                Title title = processXci(filename) ?? processNsp(filename);

                try
                {
                    title.filename = filename;
                    title.filesize = new FileInfo(filename).Length;
                }
                catch (NullReferenceException)
                {
                    return null;
                }

                return title;
            }
            catch (MissingKeyException)
            { }

            return null;
        }

        private Title processXci(string filename)
        {
            Title title = new Title();

            using (var filestream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                Xci xci;
                string biggestNca = null, controlNca = null;

                try
                {
                    xci = new Xci(keyset, filestream.AsStorage());

                    title.distribution = Title.Distribution.Cartridge;
                }
                catch (InvalidDataException)
                {
                    return null;
                }

                if (xci.RootPartition != null)
                {
                    title.structure.Add(Title.Structure.RootPartition);
                }

                if (xci.UpdatePartition != null)
                {
                    title.structure.Add(Title.Structure.UpdatePartition);
                }

                if (xci.NormalPartition != null)
                {
                    title.structure.Add(Title.Structure.NormalPartition);
                }

                if (xci.SecurePartition != null)
                {
                    PfsFileEntry[] fileEntries = xci.SecurePartition.Files;
                    foreach (PfsFileEntry entry in fileEntries)
                    {
                        if (entry.Name.EndsWith(".cnmt.nca"))
                        {
                            using (var cnmtNca = xci.SecurePartition.OpenFile(entry))
                            {
                                Nca nca = new Nca(keyset, cnmtNca, false);

                                try
                                {
                                    Pfs ncaPfs = new Pfs(nca.OpenSection(0, false, IntegrityCheckLevel.ErrorOnInvalid, true));

                                    PfsFileEntry[] ncaFileEntries = ncaPfs.Files;
                                    foreach (PfsFileEntry pfsEntry in ncaFileEntries)
                                    {
                                        Cnmt cnmt = new Cnmt(ncaPfs.OpenFile(pfsEntry).AsStream());

                                        if (title.version == unchecked((uint)-1) || cnmt.TitleVersion?.Version > title.version)
                                        {
                                            title.type = cnmt.Type;

                                            byte[] titleID = BitConverter.GetBytes(cnmt.TitleId);
                                            Array.Reverse(titleID);
                                            title.titleID = BitConverter.ToString(titleID).Replace("-", "").ToUpper();

                                            title.version = cnmt.TitleVersion?.Version ?? title.version;

                                            uint firmware = cnmt.MinimumSystemVersion?.Version ?? 0;
                                            if (firmware == 0)
                                            {
                                                title.firmware = "0";
                                            }
                                            else if (firmware <= 450)
                                            {
                                                title.firmware = "1.0.0";
                                            }
                                            else if (firmware <= 65796)
                                            {
                                                title.firmware = "2.0.0";
                                            }
                                            else if (firmware <= 131162)
                                            {
                                                title.firmware = "2.1.0";
                                            }
                                            else if (firmware <= 196628)
                                            {
                                                title.firmware = "2.2.0";
                                            }
                                            else if (firmware <= 262164)
                                            {
                                                title.firmware = "2.3.0";
                                            }
                                            else
                                            {
                                                title.firmware = ((firmware >> 26) & 0x3F) + "." + ((firmware >> 20) & 0x3F) + "." + ((firmware >> 16) & 0x0F);
                                            }

                                            CnmtContentEntry[] contentEntries = cnmt.ContentEntries;
                                            foreach (CnmtContentEntry contentEntry in contentEntries)
                                            {
                                                if (title.type == TitleType.Application || title.type == TitleType.Patch)
                                                {
                                                    if (contentEntry.Type == CnmtContentType.Program)
                                                    {
                                                        biggestNca = BitConverter.ToString(contentEntry.NcaId).Replace("-", "").ToLower() + ".nca";
                                                    }
                                                    else if (contentEntry.Type == CnmtContentType.Control)
                                                    {
                                                        controlNca = BitConverter.ToString(contentEntry.NcaId).Replace("-", "").ToLower() + ".nca";
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (MissingKeyException) { }
                            }

                            title.structure.Add(Title.Structure.CnmtNca);
                        }
                    }

                    if (!String.IsNullOrEmpty(biggestNca))
                    {
                        using (var biggest = xci.SecurePartition.OpenFile(biggestNca))
                        {
                            Nca nca = new Nca(keyset, biggest, false);
                            if (nca.Header.ContentType == ContentType.Program)
                            {
                                title.signature = (nca.Header.FixedSigValidity == Validity.Valid);
                            }

                            if (nca.Header.ContentType == ContentType.Program)
                            {
                                title.masterkey = (uint)nca.Header.CryptoType == 2 ? Math.Max((uint)nca.Header.CryptoType2 - 1, 0) : 0;

                                try
                                {
                                    nca.ParseNpdm();
                                }
                                catch { }

                                if (nca.Npdm != null)
                                {
                                    if (nca.Npdm.AciD.ServiceAccess.Services.Count == 0 || nca.Npdm.AciD.ServiceAccess.Services.Keys.Any(key => key.StartsWith("fsp-")))
                                    {
                                        if (nca.Npdm.AciD.FsAccess.PermissionsBitmask == 0xffffffffffffffff)
                                        {
                                            title.permission = Title.Permission.Dangerous;
                                        }
                                        else if ((nca.Npdm.AciD.FsAccess.PermissionsBitmask & 0x8000000000000000) != 0)
                                        {
                                            title.permission = Title.Permission.Unsafe;
                                        }
                                        else
                                        {
                                            title.permission = Title.Permission.Safe;
                                        }
                                    }
                                    else
                                    {
                                        title.permission = Title.Permission.Safe;
                                    }
                                }
                            }
                        }
                    }

                    if (!String.IsNullOrEmpty(controlNca))
                    {
                        using (var control = xci.SecurePartition.OpenFile(controlNca))
                        {
                            Nca nca = new Nca(keyset, control, false);
                            if (nca.Header.ContentType == ContentType.Control)
                            {
                                byte[] titleID = BitConverter.GetBytes(nca.Header.TitleId);
                                Array.Reverse(titleID);
                                title.titleID = BitConverter.ToString(titleID).Replace("-", "").ToUpper();

                                try
                                {
                                    Romfs romfs = new Romfs(nca.OpenSection(0, false, IntegrityCheckLevel.ErrorOnInvalid, true));

                                    RomfsFile[] romfsFiles = romfs.Files.ToArray();
                                    foreach (RomfsFile romfsFile in romfsFiles)
                                    {
                                        if (romfsFile.Name.Equals("control.nacp"))
                                        {
                                            Nacp nacp = new Nacp(romfs.OpenFile(romfsFile).AsStream());

                                            title.titleName = nacp.Descriptions.First().Title;
                                            title.displayVersion = nacp.DisplayVersion;
                                        }
                                    }
                                }
                                catch (MissingKeyException) { }
                            }
                        }
                    }

                    title.structure.Add(Title.Structure.SecurePartition);
                }

                if (xci.LogoPartition != null)
                {
                    title.structure.Add(Title.Structure.LogoPartition);
                }
            }

            if (title.type == TitleType.Application || title.type == TitleType.Patch)
            {
                uint version;
                if (versionList.TryGetValue(title.titleIDApplication, out version))
                {
                    title.latestVersion = version;
                }
            }

            return title;
        }

        private Title processNsp(string filename)
        {
            Title title = new Title();

            using (var filestream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                Pfs pfs;
                string biggestNca = null, controlNca = null;

                try
                {
                    pfs = new Pfs(filestream.AsStorage());

                    title.distribution = Title.Distribution.Digital;
                }
                catch (InvalidDataException)
                {
                    return null;
                }

                PfsFileEntry[] fileEntries = pfs.Files;
                foreach (PfsFileEntry entry in fileEntries)
                {
                    if (entry.Name.EndsWith(".cnmt.xml"))
                    {
                        using (var cnmtXml = pfs.OpenFile(entry))
                        {
                            XDocument xml = XDocument.Load(cnmtXml.AsStream());

                            TitleType titleType;
                            Enum.TryParse(xml.Element("ContentMeta").Element("Type").Value, true, out titleType);
                            title.type = titleType;

                            title.titleID = xml.Element("ContentMeta").Element("Id").Value.Remove(1, 2).ToUpper();
                            title.version = Convert.ToUInt32(xml.Element("ContentMeta").Element("Version").Value);

                            uint firmware = (uint)(Convert.ToUInt64(xml.Element("ContentMeta").Element("RequiredSystemVersion").Value) % 0x100000000);
                            if (firmware == 0)
                            {
                                title.firmware = "0";
                            }
                            else if (firmware <= 450)
                            {
                                title.firmware = "1.0.0";
                            }
                            else if (firmware <= 65796)
                            {
                                title.firmware = "2.0.0";
                            }
                            else if (firmware <= 131162)
                            {
                                title.firmware = "2.1.0";
                            }
                            else if (firmware <= 196628)
                            {
                                title.firmware = "2.2.0";
                            }
                            else if (firmware <= 262164)
                            {
                                title.firmware = "2.3.0";
                            }
                            else
                            {
                                title.firmware = ((firmware >> 26) & 0x3F) + "." + ((firmware >> 20) & 0x3F) + "." + ((firmware >> 16) & 0x0F);
                            }

                            title.masterkey = Math.Max(Convert.ToUInt32(xml.Element("ContentMeta").Element("KeyGenerationMin").Value) - 1, 0);

                            foreach (XElement element in xml.Descendants("Content"))
                            {
                                if (title.type == TitleType.Application || title.type == TitleType.Patch)
                                {
                                    if (element.Element("Type").Value == "Program")
                                    {
                                        biggestNca = element.Element("Id").Value + ".nca";
                                    }
                                    else if (element.Element("Type").Value == "Control")
                                    {
                                        controlNca = element.Element("Id").Value + ".nca";
                                    }
                                }
                                else if (title.type == TitleType.AddOnContent)
                                {
                                    if (element.Element("Type").Value == "Data")
                                    {
                                        biggestNca = element.Element("Id").Value + ".nca";
                                    }
                                }
                            }
                        }

                        title.structure.Add(Title.Structure.CnmtXml);
                    }
                    else if (entry.Name.EndsWith(".cnmt.nca"))
                    {
                        using (var cnmtNca = pfs.OpenFile(entry))
                        {
                            Nca nca = new Nca(keyset, cnmtNca, false);

                            try
                            {
                                Pfs ncaPfs = new Pfs(nca.OpenSection(0, false, IntegrityCheckLevel.ErrorOnInvalid, true));

                                PfsFileEntry[] ncaFileEntries = ncaPfs.Files;
                                foreach (PfsFileEntry pfsEntry in ncaFileEntries)
                                {
                                    Cnmt cnmt = new Cnmt(ncaPfs.OpenFile(pfsEntry).AsStream());

                                    title.type = cnmt.Type;

                                    byte[] titleID = BitConverter.GetBytes(cnmt.TitleId);
                                    Array.Reverse(titleID);
                                    title.titleID = BitConverter.ToString(titleID).Replace("-", "").ToUpper();

                                    title.version = cnmt.TitleVersion?.Version ?? title.version;

                                    uint firmware = cnmt.MinimumSystemVersion?.Version ?? 0;
                                    if (firmware == 0)
                                    {
                                        title.firmware = "0";
                                    }
                                    else if (firmware <= 450)
                                    {
                                        title.firmware = "1.0.0";
                                    }
                                    else if (firmware <= 65796)
                                    {
                                        title.firmware = "2.0.0";
                                    }
                                    else if (firmware <= 131162)
                                    {
                                        title.firmware = "2.1.0";
                                    }
                                    else if (firmware <= 196628)
                                    {
                                        title.firmware = "2.2.0";
                                    }
                                    else if (firmware <= 262164)
                                    {
                                        title.firmware = "2.3.0";
                                    }
                                    else
                                    {
                                        title.firmware = ((firmware >> 26) & 0x3F) + "." + ((firmware >> 20) & 0x3F) + "." + ((firmware >> 16) & 0x0F);
                                    }

                                    CnmtContentEntry[] contentEntries = cnmt.ContentEntries;
                                    foreach (CnmtContentEntry contentEntry in contentEntries)
                                    {
                                        if (title.type == TitleType.Application || title.type == TitleType.Patch)
                                        {
                                            if (contentEntry.Type == CnmtContentType.Program)
                                            {
                                                biggestNca = BitConverter.ToString(contentEntry.NcaId).Replace("-", "").ToLower() + ".nca";
                                            }
                                            else if (contentEntry.Type == CnmtContentType.Control)
                                            {
                                                controlNca = BitConverter.ToString(contentEntry.NcaId).Replace("-", "").ToLower() + ".nca";
                                            }
                                        }
                                        else if (title.type == TitleType.AddOnContent)
                                        {
                                            if (contentEntry.Type == CnmtContentType.Data)
                                            {
                                                biggestNca = BitConverter.ToString(contentEntry.NcaId).Replace("-", "").ToLower() + ".nca";
                                            }
                                        }
                                    }
                                }
                            }
                            catch (MissingKeyException) { }
                        }

                        title.structure.Add(Title.Structure.CnmtNca);
                    }
                    else if (entry.Name.EndsWith(".cert"))
                    {
                        title.structure.Add(Title.Structure.Cert);
                    }
                    else if (entry.Name.EndsWith(".tik"))
                    {
                        title.structure.Add(Title.Structure.Tik);
                    }
                    else if (entry.Name.EndsWith(".legalinfo.xml"))
                    {
                        title.structure.Add(Title.Structure.LegalinfoXml);
                    }
                    else if (entry.Name.EndsWith(".nacp.xml"))
                    {
                        using (var nacpXml = pfs.OpenFile(entry))
                        {
                            XDocument xml = XDocument.Load(nacpXml.AsStream());

                            title.titleName = xml.Element("Application").Element("Title").Element("Name").Value;
                            title.displayVersion = xml.Element("Application").Element("DisplayVersion").Value;
                        }

                        title.structure.Add(Title.Structure.NacpXml);
                    }
                    else if (entry.Name.EndsWith(".programinfo.xml"))
                    {
                        title.structure.Add(Title.Structure.PrograminfoXml);
                    }
                    else if (entry.Name.Equals("cardspec.xml"))
                    {
                        title.structure.Add(Title.Structure.CardspecXml);
                    }
                    else if (entry.Name.Equals("authoringtoolinfo.xml"))
                    {
                        title.structure.Add(Title.Structure.AuthoringtoolinfoXml);
                    }
                }

                if (!String.IsNullOrEmpty(biggestNca))
                {
                    using (var biggest = pfs.OpenFile(biggestNca))
                    {
                        Nca nca = new Nca(keyset, biggest, false);
                        if (((title.type == TitleType.Application || title.type == TitleType.Patch) && nca.Header.ContentType == ContentType.Program) ||
                            (title.type == TitleType.AddOnContent && nca.Header.ContentType == ContentType.AocData))
                        {
                            title.signature = (nca.Header.FixedSigValidity == Validity.Valid);
                        }

                        if (nca.Header.ContentType == ContentType.Program)
                        {
                            title.masterkey = (uint)nca.Header.CryptoType == 2 ? Math.Max((uint)nca.Header.CryptoType2 - 1, 0) : 0;

                            try
                            {
                                nca.ParseNpdm();
                            }
                            catch { }

                            if (nca.Npdm != null)
                            {
                                if (nca.Npdm.AciD.ServiceAccess.Services.Count == 0 || nca.Npdm.AciD.ServiceAccess.Services.Keys.Any(key => key.StartsWith("fsp-")))
                                {
                                    if (nca.Npdm.AciD.FsAccess.PermissionsBitmask == 0xffffffffffffffff)
                                    {
                                        title.permission = Title.Permission.Dangerous;
                                    }
                                    else if ((nca.Npdm.AciD.FsAccess.PermissionsBitmask & 0x8000000000000000) != 0)
                                    {
                                        title.permission = Title.Permission.Unsafe;
                                    }
                                    else
                                    {
                                        title.permission = Title.Permission.Safe;
                                    }
                                }
                                else
                                {
                                    title.permission = Title.Permission.Safe;
                                }
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(controlNca))
                {
                    using (var control = pfs.OpenFile(controlNca))
                    {
                        Nca nca = new Nca(keyset, control, false);
                        if (nca.Header.ContentType == ContentType.Control)
                        {
                            byte[] titleID = BitConverter.GetBytes(nca.Header.TitleId);
                            Array.Reverse(titleID);
                            title.titleID = BitConverter.ToString(titleID).Replace("-", "").ToUpper();

                            try
                            {
                                Romfs romfs = new Romfs(nca.OpenSection(0, false, IntegrityCheckLevel.ErrorOnInvalid, true));

                                RomfsFile[] romfsFiles = romfs.Files.ToArray();
                                foreach (RomfsFile romfsFile in romfsFiles)
                                {
                                    if (romfsFile.Name.Equals("control.nacp"))
                                    {
                                        Nacp nacp = new Nacp(romfs.OpenFile(romfsFile).AsStream());

                                        title.titleName = nacp.Descriptions.First().Title;
                                        title.displayVersion = nacp.DisplayVersion;
                                    }
                                }
                            }
                            catch (MissingKeyException) { }
                        }
                    }
                }
            }

            if (title.type == TitleType.Application || title.type == TitleType.Patch)
            {
                uint version;
                if (versionList.TryGetValue(title.titleIDApplication, out version))
                {
                    title.latestVersion = version;
                }
            }

            return title;
        }
    }
}
