using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
#if WINDOWS
using System.Reflection;
#endif
using System.Text;
#if MACOS
using System.Text.RegularExpressions;
#endif
using System.Xml.Linq;
#if MACOS
using Foundation;
#endif
using LibHac;
using LibHac.IO;
using Newtonsoft.Json;
using FsTitle = LibHac.Title;
using Title = NX_Game_Info.Common.Title;
using ArrayOfTitle = NX_Game_Info.Common.ArrayOfTitle;

#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
#pragma warning disable RECS0061 // Warns when a culture-aware 'EndsWith' call is used by default.
#pragma warning disable RECS0063 // Warns when a culture-aware 'StartsWith' call is used by default.
#pragma warning disable IDE1006 // Naming rule violation: These words must begin with upper case characters

namespace NX_Game_Info
{
    public class Process
    {
        public static Keyset keyset;
        public static Dictionary<string, uint> versionList = new Dictionary<string, uint>();

        public static Dictionary<string, string> titleNames = new Dictionary<string, string>();
        public static Dictionary<string, uint> titleVersions = new Dictionary<string, uint>();

        public static Dictionary<string, uint> latestVersions = new Dictionary<string, uint>();

        public static string path_prefix = File.Exists(Common.APPLICATION_DIRECTORY_PATH_PREFIX + Common.PROD_KEYS) ? Common.APPLICATION_DIRECTORY_PATH_PREFIX : Common.USER_PROFILE_PATH_PREFIX;
        public static StreamWriter log;

        public static bool initialize(out List<string> messages)
        {
            messages = new List<string>();

            string prod_keys = path_prefix + Common.PROD_KEYS;
            string title_keys = path_prefix + Common.TITLE_KEYS;
            string console_keys = path_prefix + Common.CONSOLE_KEYS;

            if (!File.Exists(prod_keys))
            {
                messages.Add("File not found. Check if '" + Common.PROD_KEYS + "' exist and try again.");
                return false;
            }

            if (Common.Settings.Default.DebugLog)
            {
                try
                {
                    log = File.AppendText(path_prefix + Common.LOG_FILE);
                    log.AutoFlush = true;
                }
                catch { }
            }

            log?.WriteLine("--------------------------------------------------------------");
            log?.WriteLine("Application starts at {0}\n", String.Format("{0:F}", DateTime.Now));

            try
            {
                log?.WriteLine("Reading keys");
                log?.WriteLine(" - {0} ({1}exists)", prod_keys, File.Exists(prod_keys) ? "" : "not ");
                log?.WriteLine(" - {0} ({1}exists)", title_keys, File.Exists(title_keys) ? "" : "not ");
                log?.WriteLine(" - {0} ({1}exists)", console_keys, File.Exists(console_keys) ? "" : "not ");

                keyset = ExternalKeys.ReadKeyFile(prod_keys, title_keys, console_keys);

                log?.WriteLine("Found {0} title keys", keyset?.TitleKeys?.Count);

                titleNames = keyset.TitleNames.ToDictionary(p => BitConverter.ToString(p.Key.Take(8).ToArray()).Replace("-", "").ToUpper(), p => p.Value);
                titleVersions = keyset.TitleVersions.ToDictionary(p => BitConverter.ToString(p.Key.Take(8).ToArray()).Replace("-", "").ToUpper(), p => p.Value);
            }
            catch { }

            try
            {
                if (!(bool)keyset?.HeaderKey?.Any(b => b != 0) ||
                    !(bool)keyset?.AesKekGenerationSource?.Any(b => b != 0) || !(bool)keyset?.AesKeyGenerationSource?.Any(b => b != 0) || !(bool)keyset?.KeyAreaKeyApplicationSource?.Any(b => b != 0) ||
                    !((bool)keyset?.MasterKeys?[0]?.Any(b => b != 0) || (bool)keyset?.KeyAreaKeys?[0]?[0]?.Any(b => b != 0)))
                {
                    log?.WriteLine("Keyfile missing required keys");
                    log?.WriteLine(" - {0} ({1}exists)", "header_key", (bool)keyset?.HeaderKey?.Any(b => b != 0) ? "" : "not ");
                    log?.WriteLine(" - {0} ({1}exists)", "aes_kek_generation_source", (bool)keyset?.AesKekGenerationSource?.Any(b => b != 0) ? "" : "not ");
                    log?.WriteLine(" - {0} ({1}exists)", "aes_key_generation_source", (bool)keyset?.AesKeyGenerationSource?.Any(b => b != 0) ? "" : "not ");
                    log?.WriteLine(" - {0} ({1}exists)", "key_area_key_application_source", (bool)keyset?.KeyAreaKeyApplicationSource?.Any(b => b != 0) ? "" : "not ");
                    log?.WriteLine(" - {0} ({1}exists)", "master_key_00", (bool)keyset?.MasterKeys[0]?.Any(b => b != 0) ? "" : "not ");
                    log?.WriteLine(" - {0} ({1}exists)", "key_area_key_application_00", (bool)keyset?.KeyAreaKeys[0][0]?.Any(b => b != 0) ? "" : "not ");

                    messages.Add("Keyfile missing required keys. Check if these keys exist and try again.\n" +
                        "header_key, aes_kek_generation_source, aes_key_generation_source, key_area_key_application_source, master_key_00 or key_area_key_application_00.");
                    return false;
                }
            }
            catch
            {
                messages.Add("Keyfile missing required keys. Check if these keys exist and try again.\n" +
                    "header_key, aes_kek_generation_source, aes_key_generation_source, key_area_key_application_source, master_key_00 or key_area_key_application_00.");
                return false;
            }

            string hac_versionlist = path_prefix + Common.HAC_VERSIONLIST;

            try
            {
                log?.WriteLine("Reading version list");

                var versionlist = JsonConvert.DeserializeObject<Common.VersionList>(File.ReadAllText(hac_versionlist));

                foreach (var title in versionlist.titles)
                {
                    string titleID = title.id;
                    if (titleID.EndsWith("800"))
                    {
                        titleID = titleID.Substring(0, Math.Min(titleID.Length, 13)) + "000";
                    }

                    versionList.TryGetValue(titleID.ToUpper(), out uint version);
                    versionList[titleID.ToUpper()] = Math.Max(version, title.version);
                }

                log?.WriteLine("Found {0} titles, last modified at {1}", versionList.Count, versionlist.last_modified);
            }
            catch { }

            log?.WriteLine("Initialization success");

            return true;
        }

        public static void migrateSettings()
        {
            int version = Common.Settings.Default.Version;

            if (version < 00_06_00_00)
            {
#if WINDOWS
                int columnIndex = Common.Settings.Default.Columns.FindIndex(x => x.Equals("firmware"));
                if (columnIndex != -1)
                {
                    Common.Settings.Default.Columns.RemoveAt(columnIndex);
                    Common.Settings.Default.Columns.InsertRange(columnIndex, new string[] { "systemUpdateString", "systemVersionString", "applicationVersionString" });

                    Common.Settings.Default.ColumnWidth.RemoveAt(columnIndex);
                    Common.Settings.Default.ColumnWidth.InsertRange(columnIndex, new int[] { 100, 100, 100 });
                }
#endif
            }
            if (version < 00_07_00_00)
            {
#if WINDOWS
                int columnIndex = Common.Settings.Default.Columns.FindIndex(x => x.Equals("filename") || x.Equals("filesizeString") ||
                    x.Equals("typeString") || x.Equals("distribution") || x.Equals("structureString") || x.Equals("signatureString") || x.Equals("permissionString") || x.Equals("error"));
                if (columnIndex == -1)
                {
                    columnIndex = Common.Settings.Default.Columns.Count;
                }
                Common.Settings.Default.Columns.InsertRange(columnIndex, new string[] { "titleKey", "publisher" });
                Common.Settings.Default.ColumnWidth.InsertRange(columnIndex, new int[] { 240, 200 });
#endif
            }
            if (version < 00_07_00_01)
            {
#if WINDOWS
                int columnIndex = Common.Settings.Default.Columns.FindIndex(x => x.Equals("filename") || x.Equals("filesizeString") ||
                    x.Equals("typeString") || x.Equals("distribution") || x.Equals("structureString") || x.Equals("signatureString") || x.Equals("permissionString") || x.Equals("error"));
                if (columnIndex == -1)
                {
                    columnIndex = Common.Settings.Default.Columns.Count;
                }
                Common.Settings.Default.Columns.InsertRange(columnIndex, new string[] { "languagesString" });
                Common.Settings.Default.ColumnWidth.InsertRange(columnIndex, new int[] { 120 });
#endif
            }

#if WINDOWS
            Common.Settings.Default.Version = Assembly.GetExecutingAssembly().GetName().Version.ToInt();
#elif MACOS
            string versionString = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString").ToString();
            Match match = Regex.Match(versionString, @"\d+(\.\d+)*");
            if (match.Success)
            {
                if (Version.TryParse(match.Value, out Version ver))
                {
                    Common.Settings.Default.Version = ver.ToInt();
                }
            }
#endif
        }

        public static bool updateTitleKeys()
        {
            string title_keys = path_prefix + Common.TITLE_KEYS;

            try
            {
                log?.WriteLine("\nDownloading title keys");

                using (var httpClient = new HttpClient())
                {
                    var response = httpClient.GetAsync(Common.TITLE_KEYS_URI).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var content = response.Content.ReadAsStringAsync().Result;
                        if (!String.IsNullOrEmpty(content))
                        {
                            File.WriteAllText(title_keys, content);

                            ExternalKeys.ReadTitleKeys(keyset, title_keys);

                            log?.WriteLine("Found {0} title keys", keyset?.TitleKeys?.Count);

                            titleNames = keyset.TitleNames.GroupBy(p => BitConverter.ToString(p.Key.Take(8).ToArray()).Replace("-", "").ToUpper()).ToDictionary(p => p.Key, p => p.Last().Value);
                            titleVersions = keyset.TitleVersions.GroupBy(p => BitConverter.ToString(p.Key.Take(8).ToArray()).Replace("-", "").ToUpper()).ToDictionary(p => p.Key, p => p.Last().Value);

                            return true;
                        }
                    }
                }
            }
            catch { }

            return false;
        }

        public static bool updateVersionList()
        {
            string hac_versionlist = path_prefix + Common.HAC_VERSIONLIST;

            try
            {
                log?.WriteLine("\nDownloading version list");

                using (var httpClient = new HttpClient())
                {
                    var response = httpClient.GetAsync(Common.HAC_VERSIONLIST_URI).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var content = response.Content.ReadAsStringAsync().Result;
                        if (!String.IsNullOrEmpty(content))
                        {
                            var versionlist = JsonConvert.DeserializeObject<Common.VersionList>(content);

                            versionList.Clear();

                            foreach (var title in versionlist.titles)
                            {
                                string titleID = title.id;
                                if (titleID.EndsWith("800"))
                                {
                                    titleID = titleID.Substring(0, Math.Min(titleID.Length, 13)) + "000";
                                }

                                versionList.TryGetValue(titleID.ToUpper(), out uint version);
                                versionList[titleID.ToUpper()] = Math.Max(version, title.version);
                            }

                            log?.WriteLine("Found {0} titles, last modified at {1}", versionList.Count, versionlist.last_modified);

                            File.WriteAllText(hac_versionlist, content);

                            return true;
                        }
                    }
                }
            }
            catch { }

            return false;
        }

        public static Title processFile(string filename)
        {
            log?.WriteLine("\nProcessing file {0}", filename);

            try
            {
                Title title = processXci(filename) ?? processNsp(filename) ?? processNro(filename);

                title.filename = filename;
                title.filesize = new FileInfo(filename).Length;

                string titleID = title.type == TitleType.AddOnContent ? title.titleID : title.baseTitleID ?? "";

                if (latestVersions.TryGetValue(titleID, out uint version))
                {
                    if (title.version > version)
                    {
                        latestVersions[titleID] = title.version;
                    }
                }
                else
                {
                    latestVersions.Add(titleID, title.version);
                }

                return title;
            }
            catch (MissingKeyException ex)
            {
                log?.WriteLine("Missing {0}: {1}", ex.Type == KeyType.Title ? "Title Key" : "Key", ex.Name.Replace("key_area_key_application", "master_key"));
            }
            catch (SystemException ex) when (ex is NullReferenceException || ex is ArgumentException)
            {
                log?.WriteLine(ex.StackTrace);

                log?.WriteLine("\nFile {0} has failed to process", filename);
            }

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

                    log?.WriteLine("Processing XCI {0}", filename);

                    if (xci.RootPartition?.Files.Length > 0)
                    {
                        title.structure.Add(Title.Structure.RootPartition);
                    }

                    if (xci.UpdatePartition?.Files.Length > 0)
                    {
                        PfsFileEntry[] fileEntries = xci.UpdatePartition.Files;

                        List<string> cnmtNca = fileEntries.Select(x => x.Name).Where(x => x.EndsWith(".cnmt.nca")).Intersect(Title.SystemUpdate.Keys).ToList();
                        if (cnmtNca.Any())
                        {
                            uint systemUpdate = unchecked((uint)-1);
                            Title.SystemUpdate.TryGetValue(cnmtNca.First(), out systemUpdate);
                            title.systemUpdate = systemUpdate;
                        }

                        title.structure.Add(Title.Structure.UpdatePartition);
                    }

                    if (xci.NormalPartition?.Files.Length > 0)
                    {
                        title.structure.Add(Title.Structure.NormalPartition);
                    }

                    if (xci.SecurePartition?.Files.Length > 0)
                    {
                        PfsFileEntry[] fileEntries = xci.SecurePartition.Files;
                        foreach (PfsFileEntry entry in fileEntries)
                        {
                            if (entry.Name.EndsWith(".cnmt.nca"))
                            {
                                try
                                {
                                    using (var cnmtNca = xci.SecurePartition.OpenFile(entry))
                                    {
                                        var nca = processCnmtNca(cnmtNca, ref title);
                                        if (nca.Item1 != null && (nca.Item2 != null || title.type == TitleType.AddOnContent))
                                        {
                                            (biggestNca, controlNca) = nca;
                                        }
                                    }
                                }
                                catch (FileNotFoundException)
                                {
                                    if (xci.SecurePartition.FileExists(entry.Name.Replace(".nca", ".ncz")))
                                    {
                                        title.error = "Unsupported Format: Compressed NCA";
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
                                try
                                {
                                    using (var tik = xci.SecurePartition.OpenFile(entry))
                                    {
                                        if (entry.Name.Split('.')[0].TryToBytes(out byte[] rightsId))
                                        {
                                            processTik(tik, rightsId, ref keyset, out byte[] titleKey);

                                            title.titleKey = BitConverter.ToString(titleKey).Replace("-", "").ToUpper();
                                        }
                                    }
                                }
                                catch (FileNotFoundException)
                                {
                                    if (xci.SecurePartition.FileExists(entry.Name.Replace(".nca", ".ncz")))
                                    {
                                        title.error = "Unsupported Format: Compressed NCA";
                                    }
                                }

                                title.structure.Add(Title.Structure.Tik);
                            }
                        }

                        if (!String.IsNullOrEmpty(biggestNca))
                        {
                            try
                            {
                                using (var biggest = xci.SecurePartition.OpenFile(biggestNca))
                                {
                                    processBiggestNca(biggest, ref title);
                                }
                            }
                            catch (FileNotFoundException)
                            {
                                if (xci.SecurePartition.FileExists(biggestNca.Replace(".nca", ".ncz")))
                                {
                                    title.error = "Unsupported Format: Compressed NCA";
                                }
                            }
                        }

                        if (!String.IsNullOrEmpty(controlNca))
                        {
                            try
                            {
                                using (var control = xci.SecurePartition.OpenFile(controlNca))
                                {
                                    processControlNca(control, ref title);
                                }
                            }
                            catch (FileNotFoundException)
                            {
                                if (xci.SecurePartition.FileExists(controlNca.Replace(".nca", ".ncz")))
                                {
                                    title.error = "Unsupported Format: Compressed NCA";
                                }
                            }
                        }

                        title.structure.Add(Title.Structure.SecurePartition);
                    }

                    if (xci.LogoPartition?.Files.Length > 0)
                    {
                        title.structure.Add(Title.Structure.LogoPartition);
                    }
                }
                catch (InvalidDataException)
                {
                    return null;
                }
            }

            if (title.type == TitleType.Application || title.type == TitleType.Patch)
            {
                if (versionList.TryGetValue(title.baseTitleID, out uint version))
                {
                    title.latestVersion = version;
                }
            }

            log?.WriteLine("XCI information for {0}: [{1}] {2}", filename, title.titleID, title.titleName);

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

                    log?.WriteLine("Processing NSP {0}", filename);

                    PfsFileEntry[] fileEntries = pfs.Files;
                    foreach (PfsFileEntry entry in fileEntries)
                    {
                        if (entry.Name.EndsWith(".cnmt.xml"))
                        {
                            try
                            {
                                using (var cnmtXml = pfs.OpenFile(entry))
                                {
                                    (biggestNca, controlNca) = processCnmtXml(cnmtXml, ref title);
                                }
                            }
                            catch (FileNotFoundException) { }

                            title.structure.Add(Title.Structure.CnmtXml);
                        }
                        else if (entry.Name.EndsWith(".cnmt.nca"))
                        {
                            try
                            {
                                using (var cnmtNca = pfs.OpenFile(entry))
                                {
                                    var nca = processCnmtNca(cnmtNca, ref title);
                                    if (nca.Item1 != null && (nca.Item2 != null || title.type == TitleType.AddOnContent))
                                    {
                                        (biggestNca, controlNca) = nca;
                                    }
                                }
                            }
                            catch (FileNotFoundException)
                            {
                                if (pfs.FileExists(entry.Name.Replace(".nca", ".ncz")))
                                {
                                    title.error = "Unsupported Format: Compressed NCA";
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
                            try
                            {
                                using (var tik = pfs.OpenFile(entry))
                                {
                                    if (entry.Name.Split('.')[0].TryToBytes(out byte[] rightsId))
                                    {
                                        processTik(tik, rightsId, ref keyset, out byte[] titleKey);

                                        title.titleKey = BitConverter.ToString(titleKey).Replace("-", "").ToUpper();
                                    }
                                }
                            }
                            catch (FileNotFoundException)
                            {
                                if (pfs.FileExists(entry.Name.Replace(".nca", ".ncz")))
                                {
                                    title.error = "Unsupported Format: Compressed NCA";
                                }
                            }

                            title.structure.Add(Title.Structure.Tik);
                        }
                        else if (entry.Name.EndsWith(".legalinfo.xml"))
                        {
                            title.structure.Add(Title.Structure.LegalinfoXml);
                        }
                        else if (entry.Name.EndsWith(".nacp.xml"))
                        {
                            try
                            {
                                using (var nacpXml = pfs.OpenFile(entry))
                                {
                                    processNacpXml(nacpXml, ref title);
                                }
                            }
                            catch (FileNotFoundException) { }

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
                        try
                        {
                            using (var biggest = pfs.OpenFile(biggestNca))
                            {
                                processBiggestNca(biggest, ref title);
                            }
                        }
                        catch (FileNotFoundException)
                        {
                            if (pfs.FileExists(biggestNca.Replace(".nca", ".ncz")))
                            {
                                title.error = "Unsupported Format: Compressed NCA";
                            }
                        }
                    }

                    if (!String.IsNullOrEmpty(controlNca))
                    {
                        try
                        {
                            using (var control = pfs.OpenFile(controlNca))
                            {
                                processControlNca(control, ref title);
                            }
                        }
                        catch (FileNotFoundException)
                        {
                            if (pfs.FileExists(controlNca.Replace(".nca", ".ncz")))
                            {
                                title.error = "Unsupported Format: Compressed NCA";
                            }
                        }
                    }
                }
                catch (InvalidDataException)
                {
                    return null;
                }
            }

            if (title.type == TitleType.Application || title.type == TitleType.Patch)
            {
                if (versionList.TryGetValue(title.baseTitleID, out uint version))
                {
                    title.latestVersion = version;
                }
            }

            log?.WriteLine("NSP information for {0}: [{1}] {2}", filename, title.titleID, title.titleName);

            return title;
        }

        public static Title processNro(string filename)
        {
            Title title = new Title();

            using (var filestream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                Nro nro;

                try
                {
                    nro = new Nro(filestream.AsStorage());

                    title.distribution = Title.Distribution.Homebrew;

                    log?.WriteLine("Processing NRO {0}", filename);

                    using (var control = nro.OpenNacp().AsStream())
                    {
                        processControlNacp(control, ref title);
                    }
                }
                catch (InvalidDataException)
                {
                    return null;
                }
            }

            log?.WriteLine("NRO information for {0}: [{1}] {2}", filename, title.titleName, title.displayVersion);

            return title;
        }

        public static List<FsTitle> processSd(string path)
        {
            try
            {
                using (var fs = new SwitchFs(keyset, new FileSystem(path), true))
                {
                    log?.WriteLine("{0} of {1} NCA processed", fs?.Titles?.Select(title => title.Value.Ncas.Count)?.Sum(), fs?.Ncas?.Count);

                    List<Application> applications = fs?.Applications?.Values?.ToList() ?? new List<Application>();

                    log?.WriteLine("Found {0} applications", applications?.Count);

                    List<FsTitle> fsTitles = new List<FsTitle>();

                    foreach (Application application in applications)
                    {
                        if (application.Main != null)
                        {
                            if (application.Main.MetaNca != null || application.Patch?.MetaNca == null)
                            {
                                fsTitles.Add(application.Main);
                            }
                        }

                        if (application.Patch?.MetaNca != null)
                        {
                            fsTitles.Add(application.Patch);
                        }

                        fsTitles.AddRange(application.AddOnContent);
                    }

                    return fsTitles.OrderBy(fsTitle => fsTitle.Id).ToList();
                }
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
        }

        public static Title processTitle(FsTitle fsTitle)
        {
            log?.WriteLine("\nProcessing title [{0:X16}] {1}", fsTitle.Id, fsTitle.Name);

            Title title = new Title
            {
                titleID = String.Format("{0:X16}", fsTitle.Id),
                type = fsTitle.Metadata.Type,
                filesize = fsTitle.GetSize(),
                distribution = Title.Distribution.Filesystem
            };

            foreach (Nca nca in fsTitle.Ncas)
            {
                if (nca.Header.ContentType == ContentType.Program)
                {
                    title.filename = nca.Filename;

                    log?.WriteLine("Found Biggest NCA {0}", nca.Filename);

                    processBiggestNca(nca, ref title);
                }
                if (nca.Header.ContentType == ContentType.Data)
                {
                    title.filename = nca.Filename;

                    log?.WriteLine("Found Biggest NCA {0}", nca.Filename);

                    processBiggestNca(nca, ref title);
                }
                else if (nca.Header.ContentType == ContentType.Meta)
                {
                    processCnmtNca(nca, ref title, false);
                }
                else if (nca.Header.ContentType == ContentType.Control)
                {
                    log?.WriteLine("Found Control NCA {0}", nca.Filename);

                    processControlNca(nca, ref title);
                }
                else if (nca.Header.ContentType == ContentType.AocData)
                {
                    title.filename = nca.Filename;

                    log?.WriteLine("Found Biggest NCA {0}", nca.Filename);

                    processBiggestNca(nca, ref title);
                }
            }

            if (title.type == TitleType.Application || title.type == TitleType.Patch)
            {
                if (versionList.TryGetValue(title.baseTitleID, out uint version))
                {
                    title.latestVersion = version;
                }
            }

            if (title.type == TitleType.Patch)
            {
                title.titleID = title.titleID.Substring(0, Math.Min(title.titleID.Length, 13)) + "800";
            }

            if (title.version > 0)
            {
                string titleID = title.type == TitleType.AddOnContent ? title.titleID : title.baseTitleID ?? "";

                if (latestVersions.TryGetValue(titleID, out uint version))
                {
                    if (title.version > version)
                    {
                        latestVersions[titleID] = title.version;
                    }
                }
                else
                {
                    latestVersions.Add(titleID, title.version);
                }
            }

            log?.WriteLine("Title information for {0}: [{1}] {2}", title.filename, title.titleID, title.titleName);

            return title;
        }

        private static (string, string) processCnmtXml(IStorage cnmtXml, ref Title title)
        {
            string biggestNca = null, controlNca = null;

            log?.WriteLine("Processing CNMT XML");

            Stream stream = cnmtXml.AsStream();
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Close();

            byte[] bom = Encoding.UTF8.GetPreamble();
            if (buffer.Take(bom.Length).SequenceEqual(bom))
            {
                Array.Copy(buffer, bom.Length, buffer, 0, buffer.Length - bom.Length);
                Array.Resize(ref buffer, buffer.Length - bom.Length);
            }

            XDocument xml = XDocument.Parse(Encoding.UTF8.GetString(buffer).Replace("&", "&amp;"));

            Enum.TryParse(xml.Element("ContentMeta").Element("Type").Value, true, out TitleType titleType);
            title.type = titleType;

            title.titleID = xml.Element("ContentMeta").Element("Id").Value.Remove(1, 2).ToUpper();
            title.baseTitleID = String.IsNullOrEmpty(title.titleID) ? "" : title.titleID.Substring(0, Math.Min(title.titleID.Length, 13)) + "000";
            title.version = Convert.ToUInt32(xml.Element("ContentMeta").Element("Version").Value);

            title.systemUpdate = (uint)(Convert.ToUInt64(xml.Element("ContentMeta").Element("RequiredSystemVersion").Value) % 0x100000000);
            title.masterkey = (uint)Math.Max(Convert.ToInt32(xml.Element("ContentMeta").Element("KeyGenerationMin").Value) - 1, 0);

            foreach (XElement element in xml.Descendants("Content"))
            {
                if (title.type == TitleType.Application || title.type == TitleType.Patch)
                {
                    if (element.Element("Type").Value == "Program")
                    {
                        biggestNca = element.Element("Id").Value + ".nca";

                        log?.WriteLine("Found Biggest NCA {0}", biggestNca);
                    }
                    else if (element.Element("Type").Value == "Control")
                    {
                        controlNca = element.Element("Id").Value + ".nca";

                        log?.WriteLine("Found Control NCA {0}", controlNca);
                    }
                }
                else if (title.type == TitleType.AddOnContent)
                {
                    if (element.Element("Type").Value == "Data")
                    {
                        biggestNca = element.Element("Id").Value + ".nca";

                        log?.WriteLine("Found Biggest NCA {0}", biggestNca);
                    }
                }
            }

            return (biggestNca, controlNca);
        }

        private static (string, string) processCnmtNca(Nca nca, ref Title title, bool cnmtContent = true)
        {
            string biggestNca = null, controlNca = null;

            log?.WriteLine("Processing CNMT NCA");

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

                        title.titleID = String.Format("{0:X16}", cnmt.TitleId);
                        title.baseTitleID = String.Format("{0:X16}", cnmt.ApplicationTitleId);
                        title.version = cnmt.TitleVersion?.Version ?? title.version;

                        title.systemVersion = cnmt.MinimumSystemVersion?.Version ?? unchecked((uint)-1);
                        title.applicationVersion = cnmt.MinimumApplicationVersion?.Version ?? unchecked((uint)-1);

                        if (cnmtContent)
                        {
                            CnmtContentEntry[] contentEntries = cnmt.ContentEntries;
                            foreach (CnmtContentEntry contentEntry in contentEntries)
                            {
                                if (title.type == TitleType.Application || title.type == TitleType.Patch)
                                {
                                    if (contentEntry.Type == CnmtContentType.Program)
                                    {
                                        biggestNca = BitConverter.ToString(contentEntry.NcaId).Replace("-", "").ToLower() + ".nca";

                                        log?.WriteLine("Found Biggest NCA {0}", biggestNca);
                                    }
                                    else if (contentEntry.Type == CnmtContentType.Control)
                                    {
                                        controlNca = BitConverter.ToString(contentEntry.NcaId).Replace("-", "").ToLower() + ".nca";

                                        log?.WriteLine("Found Control NCA {0}", controlNca);
                                    }
                                }
                                else if (title.type == TitleType.AddOnContent)
                                {
                                    if (contentEntry.Type == CnmtContentType.Data)
                                    {
                                        biggestNca = BitConverter.ToString(contentEntry.NcaId).Replace("-", "").ToLower() + ".nca";

                                        log?.WriteLine("Found Biggest NCA {0}", biggestNca);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (MissingKeyException ex)
            {
                title.error = String.Format("Missing {0}: {1}", ex.Type == KeyType.Title ? "Title Key" : "Key", ex.Name.Replace("key_area_key_application", "master_key"));

                log?.WriteLine(title.error);
            }
            catch (FileNotFoundException) { }

            return (biggestNca, controlNca);
        }

        private static (string, string) processCnmtNca(IStorage cnmtNca, ref Title title)
        {
            Nca nca = new Nca(keyset, cnmtNca, false);

            return processCnmtNca(nca, ref title);
        }

        private static void processNacpXml(IStorage nacpXml, ref Title title)
        {
            log?.WriteLine("Processing NACP XML");

            Stream stream = nacpXml.AsStream();
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Close();

            byte[] bom = Encoding.UTF8.GetPreamble();
            if (buffer.Take(bom.Length).SequenceEqual(bom))
            {
                Array.Copy(buffer, bom.Length, buffer, 0, buffer.Length - bom.Length);
                Array.Resize(ref buffer, buffer.Length - bom.Length);
            }

            XDocument xml = XDocument.Parse(Encoding.UTF8.GetString(buffer).Replace("&", "&amp;"));

            title.titleName = xml.Element("Application").Element("Title").Element("Name").Value;
            title.displayVersion = xml.Element("Application").Element("DisplayVersion").Value;
        }

        private static void processBiggestNca(Nca nca, ref Title title)
        {
            log?.WriteLine("Processing Biggest NCA");

            if (((title.type == TitleType.Application || title.type == TitleType.Patch) && nca.Header.ContentType == ContentType.Program) ||
                (title.type == TitleType.Patch && nca.Header.ContentType == ContentType.Data) ||
                (title.type == TitleType.AddOnContent && nca.Header.ContentType == ContentType.AocData))
            {
                title.signature = (nca.Header.FixedSigValidity == Validity.Valid);
            }

            if (nca.HasRightsId)
            {
                if (keyset.TitleNames.TryGetValue(nca.Header.RightsId, out string titleName))
                {
                    title.titleName = titleName;
                }
                else if (keyset.TitleNames.TryGetValue(BitConverter.GetBytes(nca.Header.TitleId).Concat(new byte[8]).ToArray(), out titleName))
                {
                    title.titleName = titleName;
                }
                if (keyset.TitleVersions.TryGetValue(nca.Header.RightsId, out uint titleVersion))
                {
                    title.latestVersion = titleVersion;
                }
                else if (keyset.TitleVersions.TryGetValue(BitConverter.GetBytes(nca.Header.TitleId).Concat(new byte[8]).ToArray(), out titleVersion))
                {
                    title.latestVersion = titleVersion;
                }
            }

            if (String.IsNullOrEmpty(title.titleName))
            {
                if (titleNames.TryGetValue(title.titleID, out string titleName))
                {
                    title.titleName = titleName;
                }
                else if (titleNames.TryGetValue(title.baseTitleID, out titleName))
                {
                    title.titleName = titleName;

                    if (title.type == TitleType.AddOnContent)
                    {
                        title.titleName += " [DLC]";
                    }
                }
                if (titleVersions.TryGetValue(title.titleID, out uint titleVersion))
                {
                    title.latestVersion = titleVersion;
                }
            }

            if (((title.type == TitleType.Application || title.type == TitleType.Patch) && nca.Header.ContentType == ContentType.Program) ||
                (title.type == TitleType.Patch && nca.Header.ContentType == ContentType.Data) ||
                (title.type == TitleType.AddOnContent && nca.Header.ContentType == ContentType.AocData))
            {
                title.masterkey = (uint)nca.Header.CryptoType == 2 ? (uint)Math.Max(nca.Header.CryptoType2 - 1, 0) : 0;

                if (nca.Header.ContentType == ContentType.Program)
                {
                    processNpdm(nca, ref title);
                }
            }
        }

        private static void processBiggestNca(IStorage biggestNca, ref Title title)
        {
            Nca nca = new Nca(keyset, biggestNca, false);

            processBiggestNca(nca, ref title);
        }

        private static void processControlNca(Nca nca, ref Title title)
        {
            log?.WriteLine("Processing Control NCA");

            if (nca.Header.ContentType == ContentType.Control)
            {
                title.titleID = String.Format("{0:X16}", nca.Header.TitleId);

                if (title.type == TitleType.Patch)
                {
                    title.titleID = title.titleID.Substring(0, Math.Min(title.titleID.Length, 13)) + "800";
                }

                try
                {
                    Romfs romfs = new Romfs(nca.OpenSection(0, false, IntegrityCheckLevel.ErrorOnInvalid, true));

                    RomfsFile[] romfsFiles = romfs.Files.ToArray();
                    foreach (RomfsFile romfsFile in romfsFiles)
                    {
                        if (romfsFile.Name.Equals("control.nacp"))
                        {
                            using (var control = romfs.OpenFile(romfsFile).AsStream())
                            {
                                processControlNacp(control, ref title);
                            }
                        }
                    }
                }
                catch (MissingKeyException ex)
                {
                    title.error = String.Format("Missing {0}: {1}", ex.Type == KeyType.Title ? "Title Key" : "Key", ex.Name.Replace("key_area_key_application", "master_key"));

                    log?.WriteLine(title.error);
                }
                catch (FileNotFoundException) { }
            }
        }

        private static void processControlNca(IStorage controlNca, ref Title title)
        {
            Nca nca = new Nca(keyset, controlNca, false);

            processControlNca(nca, ref title);
        }

        private static void processNpdm(Nca nca, ref Title title)
        {
            log?.WriteLine("Processing NPDM");

            try
            {
                nca.ParseNpdm();
            }
            catch (MissingKeyException ex)
            {
                title.error = String.Format("Missing {0}: {1}", ex.Type == KeyType.Title ? "Title Key" : "Key", ex.Name.Replace("key_area_key_application", "master_key"));

                log?.WriteLine(title.error);
            }
            catch { }

            if (nca.Npdm != null)
            {
                if (nca.Npdm.AciD.ServiceAccess.Services.Count == 0 || nca.Npdm.AciD.ServiceAccess.Services.Keys.Any(key => key.StartsWith("fsp-")))
                {
                    log?.WriteLine("Permissions Bitmask 0x{0:x16}", nca.Npdm.AciD.FsAccess.PermissionsBitmask);

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

        private static void processControlNacp(Stream controlNacp, ref Title title)
        {
            log?.WriteLine("Processing Control NACP");

            Nacp nacp = new Nacp(controlNacp);

            bool first = true;
            foreach (NacpDescription description in nacp.Descriptions)
            {
                if (!String.IsNullOrEmpty(description.Title))
                {
                    if (first)
                    {
                        title.titleName = description.Title;
                        title.publisher = description.Developer;
                        first = false;
                    }

                    title.languages.Add(Title.LanguageCode.ElementAtOrDefault((int)description.Language));
                }
            }

            title.displayVersion = nacp.DisplayVersion;
        }

        private static void processTik(IStorage tik, byte[] rightsId, ref Keyset keyset, out byte[] titleKey)
        {
            log?.WriteLine("Processing TIK");

            const int TitleKeySize = 0x10;
            titleKey = new byte[TitleKeySize];

            Stream stream = tik.AsStream();
            stream.Seek(0x180, SeekOrigin.Begin);
            stream.Read(titleKey, 0, titleKey.Length);
            stream.Close();

            if (rightsId.Length == TitleKeySize)
            {
                keyset.TitleKeys[rightsId] = titleKey;
            }
        }

        public static List<Title> processHistory(int index = -1)
        {
            ArrayOfTitle history = index != -1 ? Common.History.Default.Titles.ElementAtOrDefault(index) : Common.History.Default.Titles.LastOrDefault();
            List<Title> titles = history?.title?.ToList() ?? new List<Title>();

            foreach (var title in titles)
            {
                string titleID = title.type == TitleType.AddOnContent ? title.titleID : title.baseTitleID ?? "";

                if (latestVersions.TryGetValue(titleID, out uint version))
                {
                    if (title.version > version)
                    {
                        latestVersions[titleID] = title.version;
                    }
                }
                else
                {
                    latestVersions.Add(titleID, title.version);
                }
            }

            return titles;
        }
    }
}
