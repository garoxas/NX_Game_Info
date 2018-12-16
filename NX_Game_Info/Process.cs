using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LibHac;
using LibHac.IO;
using Newtonsoft.Json;
using Title = NX_Game_Info.Common.Title;

#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
#pragma warning disable RECS0061 // Warns when a culture-aware 'EndsWith' call is used by default.
#pragma warning disable RECS0063 // Warns when a culture-aware 'StartsWith' call is used by default.

namespace NX_Game_Info
{
    class Process
    {
        public static Keyset keyset;
        public static Dictionary<string, uint> versionList = new Dictionary<string, uint>();

        public static bool initialize(out List<string> messages)
        {
            messages = new List<string>();

            if (!File.Exists(Common.PROD_KEYS))
            {
                messages.Add("File not found. Check if '" + Common.PROD_KEYS + "' exist and try again.");
                return false;
            }

            try
            {
                keyset = ExternalKeys.ReadKeyFile(Common.PROD_KEYS, Common.TITLE_KEYS);
            }
            catch { }

            bool haveKakSource = !((bool)keyset?.AesKekGenerationSource.All(b => b == 0) || (bool)keyset?.AesKeyGenerationSource.All(b => b == 0) || (bool)keyset?.KeyAreaKeyApplicationSource.All(b => b == 0));
            if ((bool)keyset?.HeaderKey.All(b => b == 0) ||
                ((haveKakSource && (bool)keyset?.MasterKeys[0].All(b => b == 0)) && (bool)keyset?.KeyAreaKeys[0][0].All(b => b == 0)) && (bool)keyset?.Titlekeks[0].All(b => b == 0))
            {
                messages.Add("Keyfile missing required keys. Check if these keys exist and try again.\n" +
                    "header_key, aes_kek_generation_source, aes_key_generation_source, key_area_key_application_source, master_key_00.");
                Environment.Exit(-1);
            }

            int[] masterkey = new int[6];
            for (int i = 0; i < masterkey.Length; i++)
            {
                if (((haveKakSource && (bool)keyset?.MasterKeys[i + 1].All(b => b == 0)) && (bool)keyset?.KeyAreaKeys[i + 1][0].All(b => b == 0)) && (bool)keyset?.Titlekeks[i + 1].All(b => b == 0))
                {
                    masterkey[i] = i + 1;
                }
            }

            if (!Common.Settings.Default.MasterKey)
            {
                if (masterkey.Any(b => b != 0))
                {
                    messages.Add("master_key_0X, key_area_key_application_0X or titlekek_0X for MasterKey " + string.Join(", ", masterkey.Where(b => b != 0)) +
                        " are missing from Keyfile.\nGames using this key may be missing or incorrect.");

                    Common.Settings.Default.MasterKey = true;
                    Common.Settings.Default.Save();
                }
            }

            if (!Common.Settings.Default.TitleKeys)
            {
                if (keyset?.TitleKeys.Count == 0)
                {
                    messages.Add("Title Keys is missing.\nGames using Titlekey crypto may be missing or incorrect.");

                    Common.Settings.Default.TitleKeys = true;
                    Common.Settings.Default.Save();
                }
            }

            try
            {
                var versionlist = JsonConvert.DeserializeObject<Common.VersionList>(File.ReadAllText(Common.HAC_VERSIONLIST));

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

            return true;
        }

        public static Title processFile(string filename)
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

        public static Title processXci(string filename)
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

        public static Title processNsp(string filename)
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
