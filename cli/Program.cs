using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LibHac;
using Mono.Options;
using FsTitle = LibHac.Title;
using Title = NX_Game_Info.Common.Title;

namespace NX_Game_Info
{
    class Program
    {
        static void Main(string[] args)
        {
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            if (info != null)
            {
                Console.WriteLine("{0} {1}", info.ProductName, info.ProductVersion);
                Console.WriteLine("{0} {1}", info.LegalCopyright, info.CompanyName);
            }
            else
            {
                Console.WriteLine("{0} {1}", Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version);
            }

            Console.WriteLine();

            bool sdcard = false;
            string sort = "";

            OptionSet options = null;
            options = new OptionSet()
            {
                { "c|sdcard", "open path as sdcard", v => sdcard = v != null },
                { "s|sort=", "sort by titleid, titlename or filename [default: filename]", (string s) => sort = s },
                { "h|help", "show this help message and exit", v => printHelp(options) },
                { "z|nsz", "enable nsz extension", v => Common.Settings.Default.NszExtension = v != null, true },
                { "d|debug", "enable debug log", v => Common.Settings.Default.DebugLog = v != null },
            };

            List<string> paths;
            try
            {
                paths = options.Parse(args);
            }
            catch (OptionException)
            {
                printHelp(options);
                return;
            }

            if (!paths.Any())
            {
                printHelp(options);
                return;
            }

            bool init = Process.initialize(out List<string> messages);

            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (var message in messages)
            {
                Console.WriteLine(message);
            }
            Console.ResetColor();

            if (!init)
            {
                Environment.Exit(-1);
            }

            processPaths(paths, sort, sdcard);
        }

        static void printHelp(OptionSet options)
        {
            Console.Error.WriteLine("usage: {0} [-h|--help] [-d|--debug] [-c|--sdcard] [-s(titleid|titlename|filename)|--sort=(titleid|titlename|filename)] paths...\n", Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location).Quote());
            options.WriteOptionDescriptions(Console.Error);

            Environment.Exit(-1);
        }

        static void processPaths(List<string> paths, string sort, bool sdcard)
        {
            List<Title> titles = new List<Title>();

            foreach (string path in paths)
            {
                if (Directory.Exists(path))
                {
                    if (sdcard)
                    {
                        titles.AddRange(openSDCard(path));
                    }
                    else
                    {
                        titles.AddRange(openDirectory(path));
                    }
                }
                else if (File.Exists(path) && (Common.Settings.Default.NszExtension ? new string[] { ".xci", ".nsp", ".xcz", ".nsz", ".nro" } : new string[] { ".xci", ".nsp", ".nro" }).Any(ext => ext.Equals(Path.GetExtension(path).ToLower())) && !sdcard)
                {
                    Title title = openFile(path);
                    if (title != null)
                    {
                        titles.Add(title);
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Error.WriteLine("{0} is not supported or not a valid path", path);
                    Console.ResetColor();
                }
            }

            titles = titles.Distinct().OrderBy(x => sort.Equals("titleid") ? x.titleID : sort.Equals("titlename") ? x.titleName : x.filename).ToList();

            foreach (Title title in titles)
            {
                if (title.permission == Title.Permission.Dangerous)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                }
                else if (title.permission == Title.Permission.Unsafe)
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                Console.WriteLine("\n{0}", title.filename);
                Console.ResetColor();

                Console.WriteLine(@"├ {0}: {1}
├ {2}: {3}
├ {4}: {5}
├ {6}: {7}
├ {8}: {9}
├ {10}: {11}
├ {12}: {13}
├ {14}: {15}
├ {16}: {17}
├ {18}: {19}
├ {20}: {21}
├ {22}: {23}
├ {24}: {25}
├ {26}: {27}
├ {28}: {29}
├ {30}: {31}
├ {32}: {33}
├ {34}: {35}
├ {36}: {37}
├ {38}: {39}
└ {40}: {41}",
                Title.Properties[0], title.titleID,
                Title.Properties[1], title.baseTitleID,
                Title.Properties[2], title.titleName,
                Title.Properties[3], title.displayVersion,
                Title.Properties[4], title.versionString,
                Title.Properties[5], title.latestVersionString,
                Title.Properties[6], title.systemUpdateString,
                Title.Properties[7], title.systemVersionString,
                Title.Properties[8], title.applicationVersionString,
                Title.Properties[9], title.masterkeyString,
                Title.Properties[10], title.titleKey,
                Title.Properties[11], title.publisher,
                Title.Properties[12], title.languagesString,
                Title.Properties[13], title.filename,
                Title.Properties[14], title.filesizeString,
                Title.Properties[15], title.typeString,
                Title.Properties[16], title.distribution,
                Title.Properties[17], title.structureString,
                Title.Properties[18], title.signatureString,
                Title.Properties[19], title.permissionString,
                Title.Properties[20], title.error
                );
            }

            Process.log?.WriteLine("\n{0} titles processed", titles.Count);
            Console.Error.WriteLine("\n{0} titles processed", titles.Count);
        }

        static Title openFile(string filename)
        {
            Process.log?.WriteLine("\nOpen File");

            Process.log?.WriteLine("File selected");
            Console.Error.WriteLine("Opening file {0}", filename);

            Title title = Process.processFile(filename);

            Process.log?.WriteLine("\nTitle processed");

            return title;
        }

        static List<Title> openDirectory(string path)
        {
            Process.log?.WriteLine("\nOpen Directory");

            List<string> filenames = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                .Where(filename => filename.ToLower().EndsWith(".xci") || filename.ToLower().EndsWith(".nsp") || filename.ToLower().EndsWith(".nro") ||
                (Common.Settings.Default.NszExtension && (filename.ToLower().EndsWith(".xcz") || filename.ToLower().EndsWith(".nsz")))).ToList();
            filenames.Sort();

            Process.log?.WriteLine("{0} files selected", filenames.Count);
            Console.Error.WriteLine("Opening {0} files from directory {1}", filenames.Count, path);

            List<Title> titles = new List<Title>();

            foreach (string filename in filenames)
            {
                Title title = Process.processFile(filename);
                if (title != null)
                {
                    titles.Add(title);
                }
            }

            Process.log?.WriteLine("\n{0} titles processed", titles.Count);

            return titles;
        }

        static List<Title> openSDCard(string pathSd)
        {
            Process.log?.WriteLine("\nOpen SD Card");

            List<FsTitle> fsTitles = Process.processSd(pathSd);

            List<Title> titles = new List<Title>();

            if (fsTitles != null)
            {
                foreach (var fsTitle in fsTitles)
                {
                    Title title = Process.processTitle(fsTitle);
                    if (title != null)
                    {
                        titles.Add(title);
                    }
                }

                Process.log?.WriteLine("\n{0} titles processed", titles.Count);
            }
            else
            {
                string error = "SD card \"Contents\" directory could not be found";
                Process.log?.WriteLine(error);

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Error.WriteLine(error);
                Console.ResetColor();
            }

            return titles;
        }
    }
}
