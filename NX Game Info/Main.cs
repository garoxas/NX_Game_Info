using System;
using System.Data;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using LibHac;
using LibHac.IO;
using LibHac.Npdm;
using System.Text;
using System.Collections.Generic;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Drawing;

namespace NX_Game_Info
{
    public partial class Main : Form
    {
        private const string PROD_KEYS = "prod.keys";
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
                Invalid = -1
            }

            public enum Permission
            {
                Safe,
                Unsafe,
                Dangerous,
                Invalid = -1
            }

            public static Dictionary<ulong, Permission> permissions = new Dictionary<ulong, Permission>
            {
                { 0x8000000000000801, Permission.Safe },        // CanMountLogo, CanMountContentMeta, CanMountContentControl, CanMountContentManual, CanMountContentData, CanMountApplicationPackage
                { 0x8000000000000000, Permission.Safe },        // CanMountSaveDataStorage, CanOpenSaveDataInternalStorage
                { 0x8000000000000800, Permission.Safe },        // CanMountContentStorage
                { 0x8000000000001000, Permission.Safe },        // CanMountImageAndVideoStorage
                { 0x8000000200000000, Permission.Safe },        // CanMountCloudBackupWorkStorage
                { 0x8000000000000084, Permission.Unsafe },      // CanMountBisCalibrationFile, CanOpenBisPartitionCalibrationBinary, CanOpenBisPartitionCalibrationFile
                { 0x8000000000000080, Permission.Unsafe },      // CanMountBisSafeMode, CanMountBisUser, CanMountBisSystemProperPartition, CanOpenBisPartitionSafeMode, CanOpenBisPartitionUser, CanOpenBisPartitionSystem, CanOpenBisPartitionSystemProperEncryption, CanOpenBisPartitionSystemProperPartition
                { 0x8000000000008080, Permission.Unsafe },      // CanMountBisSystem, CanMountBisSystemProperEncryption, CanOpenBisPartitionUserDataRoot, CanOpenBisPartitionBootConfigAndPackage2*
                { 0xC000000000200000, Permission.Safe },        // CanMountSdCard, CanOpenSdCardStorage
                { 0x8000000000000010, Permission.Safe },        // CanMountGameCard
                { 0x8000000000040020, Permission.Safe },        // CanMountDeviceSaveData
                { 0x8000000000000028, Permission.Safe },        // CanMountSystemSaveData
                { 0x8000000000000020, Permission.Safe },        // CanMountOthersSaveData, CanMountOthersSystemSaveData
                { 0x8000000000010082, Permission.Unsafe },      // CanOpenBisPartitionBootPartition1Root
                { 0x8000000000010080, Permission.Unsafe },      // CanOpenBisPartitionBootPartition2Root
                { 0x8000000000000100, Permission.Safe },        // CanOpenGameCardStorage
                { 0x8000000000100008, Permission.Safe },        // CanMountSystemDataPrivate
                { 0xC000000000400000, Permission.Safe },        // CanMountHost
                { 0x8000000000010000, Permission.Safe },        // CanMountRegisteredUpdatePartition
            };

            public string titleID { get; set; }
            public string titleIDApplication { get { return String.IsNullOrEmpty(titleID) ? "" : titleID.Substring(0, Math.Min(titleID.Length, 13)) + "000"; } }
            public string titleName { get; set; }
            public string displayVersion { get; set; }
            public long version { get; set; } = -1;
            public string versionString { get { return version > -1 ? version.ToString() : ""; } }
            public long latestVersion { get; set; } = -1;
            public string latestVersionString { get { return latestVersion > -1 ? latestVersion.ToString() : ""; } }
            public string firmware { get; set; }
            public long masterkey { get; set; } = 0;
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
            public TitleType type { get; set; }
            public Distribution distribution { get; set; } = Distribution.Invalid;
            public HashSet<Structure> structure { get; set; } = new HashSet<Structure>();
            public string structureString
            {
                get
                {
                    if (distribution == Distribution.Cartridge)
                    {

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
            public ulong permissionsBitmask
            {
                set
                {
                    switch (value)
                    {
                        case 0x4000000000000000:
                            permission = Permission.Safe;
                            break;

                        case 0xffffffffffffffff:
                            permission = Permission.Dangerous;
                            break;

                        default:
                            permission = Permission.Safe;

                            foreach (var perms in permissions)
                            {
                                if ((value & perms.Key) != 0)
                                {
                                    if (perms.Value > permission)
                                    {
                                        permission = perms.Value;
                                        if (permission == Permission.Dangerous)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }

        private class VersionTitle
        {
            public string id { get; set; }
            public int version { get; set; }
            public int required_version { get; set; }
        }

        private class VersionList
        {
            public List<VersionTitle> titles { get; set; }
            public int format_version { get; set; }
            public int last_modified { get; set; }
        }

        private Keyset keyset;
        private Dictionary<string, int> versionList = new Dictionary<string, int>();

        public Main()
        {
            InitializeComponent();

            try
            {
                keyset = ExternalKeys.ReadKeyFile(PROD_KEYS);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("File not found. Check if '" + PROD_KEYS + "' exist and try again");
                Environment.Exit(-1);
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
                MessageBox.Show("Please wait until the current process is finished and try again");
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open NX Game Files";
            openFileDialog.Filter = "NX Game Files (*.xci;*.nsp)|*.xci;*.nsp|Gamecard Files (*.xci)|*.xci|Package Files (*.nsp)|*.nsp|All Files (*.*)|*.*";
            openFileDialog.Multiselect = true;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                listView.Items.Clear();

                backgroundWorkerProcess.RunWorkerAsync(openFileDialog.FileNames);
            }
        }

        private void openDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (backgroundWorkerProcess.IsBusy)
            {
                MessageBox.Show("Please wait until the current process is finished and try again");
                return;
            }

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                listView.Items.Clear();

                backgroundWorkerProcess.RunWorkerAsync(Directory.EnumerateFiles(folderBrowserDialog.SelectedPath, "*.*", SearchOption.AllDirectories)
                    .Where(filename => filename.ToLower().EndsWith(".xci") || filename.ToLower().EndsWith(".nsp")).ToArray());
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(-1);
        }

        private void backgroundWorkerProcess_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            string[] filenames = (string[])e.Argument;
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

            foreach (var title in titles)
            {
                ListViewItem listViewItem = new ListViewItem(new[] { title.titleID, title.titleName, title.displayVersion, title.versionString, title.latestVersionString,
                    title.firmware, title.masterkeyString, title.filename, new Func<long, string>((long filesize) => { StringBuilder builder = new StringBuilder(20); StrFormatByteSize(filesize, builder, 20); return builder.ToString(); })(title.filesize),
                    title.distribution.ToString(), title.structureString, title.signatureString, title.permissionString });

                if (title.signature != true)
                {
                    listViewItem.BackColor = Color.WhiteSmoke;
                }

                if (title.permission == Title.Permission.Dangerous)
                {
                    listViewItem.ForeColor = Color.DarkRed;
                }
                else if (title.permission == Title.Permission.Unsafe)
                {
                    listViewItem.ForeColor = Color.Indigo;
                }

                listView.Items.Add(listViewItem);
            }
        }

        private Title processFile(string filename)
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

        private Title processXci(string filename)
        {
            Title title = new Title();

            using (var filestream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    Xci xci = new Xci(keyset, filestream.AsStorage());

                    title.type = TitleType.Application;
                    title.distribution = Title.Distribution.Cartridge;
                }
                catch (InvalidDataException)
                {
                    return null;
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
                            title.version = Convert.ToInt64(xml.Element("ContentMeta").Element("Version").Value);

                            long firmware = Convert.ToInt64(xml.Element("ContentMeta").Element("RequiredSystemVersion").Value) % 0x100000000;
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

                            title.masterkey = Math.Max(Convert.ToInt64(xml.Element("ContentMeta").Element("KeyGenerationMin").Value) - 1, 0);

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
                            Pfs ncaPfs = new Pfs(nca.OpenSection(0, false, IntegrityCheckLevel.ErrorOnInvalid, true));

                            PfsFileEntry[] ncaFileEntries = ncaPfs.Files;
                            foreach (PfsFileEntry pfsEntry in ncaFileEntries)
                            {
                                Cnmt cnmt = new Cnmt(ncaPfs.OpenFile(pfsEntry).AsStream());

                                title.type = cnmt.Type;

                                byte[] titleID = BitConverter.GetBytes(cnmt.TitleId);
                                Array.Reverse(titleID);
                                title.titleID = BitConverter.ToString(titleID).Replace("-", "").ToUpper();

                                title.version = cnmt.TitleVersion?.Version != 0 ? cnmt.TitleVersion.Version : title.version;
                                title.firmware = cnmt.MinimumSystemVersion?.Version != 0 ? cnmt.MinimumSystemVersion?.ToString() : title.firmware ?? "0";

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
                            try
                            {
                                nca.ParseNpdm();
                            }
                            catch { }

                            if (nca.Npdm != null)
                            {
                                title.permissionsBitmask = nca.Npdm.AciD.FsAccess.PermissionsBitmask;
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
                    }
                }
            }

            if (title.type == TitleType.Application || title.type == TitleType.Patch)
            {
                int version;
                if (versionList.TryGetValue(title.titleIDApplication, out version))
                {
                    title.latestVersion = version;
                }
            }

            return title;
        }
    }
}
