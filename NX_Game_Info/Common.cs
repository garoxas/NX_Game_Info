using System;
using System.Collections.Generic;
using System.Configuration;
#if !MACOS
using System.Drawing;
#endif
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;
#if MACOS
using Foundation;
#endif
using LibHac;

#pragma warning disable IDE1006 // Naming rule violation: These words must begin with upper case characters

namespace NX_Game_Info
{
    public class Common
    {
        public static readonly string APPLICATION_DIRECTORY_PATH_PREFIX =
#if MACOS
            Path.GetDirectoryName(NSBundle.MainBundle.BundleUrl.Path) + "/";
#else
            "";
#endif
        public static readonly string USER_PROFILE_PATH_PREFIX = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.switch/";

        public static readonly string LOG_FILE = "debug.log";

        public static readonly string USER_SETTINGS = "user.settings";
        public static readonly int HISTORY_SIZE = 10;

        public static readonly string PROD_KEYS = "prod.keys";
        public static readonly string TITLE_KEYS = "title.keys";
        public static readonly string CONSOLE_KEYS = "console.keys";
        public static readonly string HAC_VERSIONLIST = "hac_versionlist.json";
        public static readonly string TAGAYA_VERSIONLIST = "https://pastebin.com/raw/9N26Bx10";

        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern Int32 StrFormatByteSize(
            long fileSize,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer,
            int bufferSize);

        public class Settings : ApplicationSettingsBase
        {
            [UserScopedSetting()]
            [DefaultSettingValue("")]
            public string InitialDirectory
            {
                get { return (string)this["InitialDirectory"]; }
                set { this["InitialDirectory"] = value; }
            }

            [UserScopedSetting()]
            [DefaultSettingValue("")]
            public string SDCardDirectory
            {
                get { return (string)this["SDCardDirectory"]; }
                set { this["SDCardDirectory"] = value; }
            }

            [UserScopedSetting()]
            [DefaultSettingValue("False")]
            public bool DebugLog
            {
                get { return (bool)this["DebugLog"]; }
                set { this["DebugLog"] = value; }
            }

#if !MACOS
            [UserScopedSetting()]
            [DefaultSettingValue("0, 0")]
            public Point WindowLocation
            {
                get { return (Point)this["WindowLocation"]; }
                set { this["WindowLocation"] = value; }
            }

            [UserScopedSetting()]
            [DefaultSettingValue("800, 600")]
            public Size WindowSize
            {
                get { return (Size)this["WindowSize"]; }
                set { this["WindowSize"] = value; }
            }

            [UserScopedSetting()]
            [DefaultSettingValue("")]
            public List<string> Columns
            {
                get { return (List<string>)this["Columns"]; }
                set { this["Columns"] = value; }
            }

            [UserScopedSetting()]
            [DefaultSettingValue("")]
            public List<int> ColumnWidth
            {
                get { return (List<int>)this["ColumnWidth"]; }
                set { this["ColumnWidth"] = value; }
            }
#endif

            public static Settings Default = (Settings)Synchronized(new Settings());
        }

        public class History : ApplicationSettingsBase
        {
            [UserScopedSetting()]
            [DefaultSettingValue("")]
            [SettingsSerializeAs(SettingsSerializeAs.Xml)]
            public List<List<Title>> Titles
            {
                get { return (List<List<Title>>)this["Titles"]; }
                set { this["Titles"] = value; }
            }

            public static History Default = (History)Synchronized(new History());
        }

        [Serializable]
        public class Title
        {
            public enum Distribution
            {
                Digital,
                Cartridge,
                Homebrew,
                Filesystem,
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

            public Title() { }

            [XmlElement("TitleID")]
            public string titleID { get; set; }
            [XmlElement("titleIDApplication")]
            public string titleIDApplication { get { return String.IsNullOrEmpty(titleID) ? "" : titleID.Substring(0, Math.Min(titleID.Length, 13)) + "000"; } }
            [XmlElement("TitleName")]
            public string titleName { get; set; }
            [XmlElement("DisplayVersion")]
            public string displayVersion { get; set; }
            [XmlElement("Version")]
            public uint version { get; set; } = unchecked((uint)-1);
            public string versionString { get { return version != unchecked((uint)-1) ? version.ToString() : ""; } }
            [XmlElement("LatestVersion")]
            public uint latestVersion { get; set; } = unchecked((uint)-1);
            public string latestVersionString { get { return latestVersion != unchecked((uint)-1) ? latestVersion.ToString() : ""; } }
            [XmlElement("Firmware")]
            public string firmware { get; set; }
            [XmlElement("Masterkey")]
            public uint masterkey { get; set; } = unchecked((uint)-1);
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
                        case 7:
                            return masterkey.ToString() + " (7.0.0-8.0.1)";
                        case unchecked((uint)-1):
                            return "";
                        default:
                            return masterkey.ToString();
                    }
                }
            }
            [XmlElement("Filename")]
            public string filename { get; set; }
            [XmlElement("Filesize")]
            public long filesize { get; set; }
            public string filesizeString
            {
                get
                {
#if MACOS
                    return NSByteCountFormatter.Format(filesize, NSByteCountFormatterCountStyle.File);
#else
                    StringBuilder builder = new StringBuilder(20); StrFormatByteSize(filesize, builder, 20); return builder.ToString();
#endif
                }
            }
            [XmlElement("Type")]
            public TitleType type { get; set; } = TitleType.Application;
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
            [XmlElement("Distribution")]
            public Distribution distribution { get; set; } = Distribution.Invalid;
            [XmlElement("Structure")]
            public HashSet<Structure> structure { get; set; } = new HashSet<Structure>();
            public string structureString
            {
                get
                {
                    if (distribution == Distribution.Cartridge)
                    {
                        if (new HashSet<Structure>(new[] { Structure.UpdatePartition, Structure.SecurePartition }).All(value => structure.Contains(value)) &&
                            new HashSet<Structure>(new[] { Structure.RootPartition, Structure.NormalPartition }).Any(value => structure.Contains(value)))
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
                    else if (distribution == Distribution.Filesystem)
                    {
                        return "Filesystem";
                    }

                    return "";
                }
            }
            [XmlElement("Signature")]
            public bool? signature { get; set; } = null;
            public string signatureString { get { return signature == null ? "" : (bool)signature ? "Passed" : "Not Passed"; } }
            [XmlElement("Permission")]
            public Permission permission { get; set; } = Permission.Invalid;
            public string permissionString { get { return permission == Permission.Invalid ? "" : permission.ToString(); } }
            [XmlElement("Error")]
            public string error { get; set; }
        }

        public class VersionTitle
        {
            public string id { get; set; }
            public uint version { get; set; }
            public uint required_version { get; set; }
        }

        public class VersionList
        {
            public List<VersionTitle> titles { get; set; }
            public uint format_version { get; set; }
            public uint last_modified { get; set; }
        }
    }
}
