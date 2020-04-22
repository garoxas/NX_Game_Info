using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LibHac;
using Mono.Options;
using OfficeOpenXml;
using FsTitle = LibHac.Title;
using Title = NX_Game_Info.Common.Title;

namespace NX_Game_Info
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

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
            string export = "";

            OptionSet options = null;
            options = new OptionSet()
            {
                { "c|sdcard", "open path as sdcard", v => sdcard = v != null },
                { "s|sort=", "sort by titleid, titlename or filename [default: filename]", (string s) => sort = s },
                { "x|export=", "export filename, only *.csv or *.xlsx supported", (string s) => export = s },
                { "l|delimiter=", "csv delimiter character [default: ,]", (char c) => Common.Settings.Default.CsvSeparator = c },
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

            processPaths(paths, sort, export, sdcard);
        }

        static void printHelp(OptionSet options)
        {
            Console.Error.WriteLine("usage: {0} [-h|--help] [-d|--debug] [-c|--sdcard] [-s(titleid|titlename|filename)|--sort=(titleid|titlename|filename)] [-x(<filename.csv>|<filename.xlsx>)|--export=(<filename.csv>|<filename.xlsx>)] [-l(<delimiter>)|--delimiter=(<delimiter>)] paths...\n", Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location).Quote());
            options.WriteOptionDescriptions(Console.Error);

            Environment.Exit(-1);
        }

        static void processPaths(List<string> paths, string sort, string export, bool sdcard)
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

            exportTitles(titles, export);
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

        static void exportTitles(List<Title> titles, string filename)
        {
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

            if (filename.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filename));
                }
                catch
                {
                    Console.Error.WriteLine("\n{0} is not supported or not a valid path", filename);
                }

                using (var writer = new StreamWriter(filename))
                {
                    char separator = Common.Settings.Default.CsvSeparator;
                    if (separator != '\0')
                    {
                        writer.WriteLine("sep={0}", separator);
                    }
                    else
                    {
                        separator = ',';
                    }

                    if (info != null)
                    {
                        writer.WriteLine("# publisher {0} {1}", info.ProductName, info.ProductVersion);
                    }
                    else
                    {
                        writer.WriteLine("# publisher {0} {1}", Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version);
                    }

                    writer.WriteLine("# updated {0}", String.Format("{0:F}", DateTime.Now));

                    writer.WriteLine(String.Join(separator.ToString(), Common.Title.Properties));

                    uint index = 0, count = (uint)titles.Count;

                    foreach (var title in titles)
                    {
                        index++;

                        writer.WriteLine(String.Join(separator.ToString(), new string[] {
                                title.titleID.Quote(separator),
                                title.baseTitleID.Quote(separator),
                                title.titleName.Quote(separator),
                                title.displayVersion.Quote(separator),
                                title.versionString.Quote(separator),
                                title.latestVersionString.Quote(separator),
                                title.systemUpdateString.Quote(separator),
                                title.systemVersionString.Quote(separator),
                                title.applicationVersionString.Quote(separator),
                                title.masterkeyString.Quote(separator),
                                title.titleKey.Quote(separator),
                                title.publisher.Quote(separator),
                                title.languagesString.Quote(separator),
                                title.filename.Quote(separator),
                                title.filesizeString.Quote(separator),
                                title.typeString.Quote(separator),
                                title.distribution.ToString().Quote(separator),
                                title.structureString.Quote(separator),
                                title.signatureString.Quote(separator),
                                title.permissionString.Quote(separator),
                                title.error.Quote(separator),
                            }));
                    }

                    Process.log?.WriteLine("\n{0} of {1} titles exported to {2}", index, titles.Count, filename);
                    Console.Error.WriteLine("\n{0} of {1} titles exported to {2}", index, titles.Count, filename);
                }
            }
            else if (filename.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filename));
                }
                catch
                {
                    Console.Error.WriteLine("\n{0} is not supported or not a valid path", filename);
                }

                using (ExcelPackage excel = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add(DateTime.Now.ToString("dd MMMM yyyy HH:mm:ss"));

                    worksheet.Cells[1, 1, 1, Title.Properties.Count()].LoadFromArrays(new List<string[]> { Title.Properties });
                    worksheet.Cells["1:1"].Style.Font.Bold = true;
                    worksheet.Cells["1:1"].Style.Font.Color.SetColor(Color.White);
                    worksheet.Cells["1:1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells["1:1"].Style.Fill.BackgroundColor.SetColor(Color.MidnightBlue);

                    uint index = 0, count = (uint)titles.Count;

                    foreach (var title in titles)
                    {
                        index++;

                        var data = new List<string[]>
                        {
                            new string[] {
                                title.titleID,
                                title.baseTitleID,
                                title.titleName,
                                title.displayVersion,
                                title.versionString,
                                title.latestVersionString,
                                title.systemUpdateString,
                                title.systemVersionString,
                                title.applicationVersionString,
                                title.masterkeyString,
                                title.titleKey,
                                title.publisher,
                                title.languagesString,
                                title.filename,
                                title.filesizeString,
                                title.typeString,
                                title.distribution.ToString(),
                                title.structureString,
                                title.signatureString,
                                title.permissionString,
                                title.error,
                            }
                        };

                        worksheet.Cells[(int)index + 1, 1].LoadFromArrays(data);

                        string titleID = title.type == TitleType.AddOnContent ? title.titleID : title.baseTitleID ?? "";

                        Process.latestVersions.TryGetValue(titleID, out uint latestVersion);
                        Process.versionList.TryGetValue(titleID, out uint version);
                        Process.titleVersions.TryGetValue(titleID, out uint titleVersion);

                        if (latestVersion < version || latestVersion < titleVersion)
                        {
                            worksheet.Cells[(int)index + 1, 1, (int)index + 1, Title.Properties.Count()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            worksheet.Cells[(int)index + 1, 1, (int)index + 1, Title.Properties.Count()].Style.Fill.BackgroundColor.SetColor(title.signature != true ? Color.OldLace : Color.LightYellow);
                        }
                        else if (title.signature != true)
                        {
                            worksheet.Cells[(int)index + 1, 1, (int)index + 1, Title.Properties.Count()].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            worksheet.Cells[(int)index + 1, 1, (int)index + 1, Title.Properties.Count()].Style.Fill.BackgroundColor.SetColor(Color.WhiteSmoke);
                        }

                        if (title.permission == Title.Permission.Dangerous)
                        {
                            worksheet.Cells[(int)index + 1, 1, (int)index + 1, Title.Properties.Count()].Style.Font.Color.SetColor(Color.DarkRed);
                        }
                        else if (title.permission == Title.Permission.Unsafe)
                        {
                            worksheet.Cells[(int)index + 1, 1, (int)index + 1, Title.Properties.Count()].Style.Font.Color.SetColor(Color.Indigo);
                        }
                    }

                    ExcelRange range = worksheet.Cells[1, 1, (int)count + 1, Title.Properties.Count()];
                    range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    worksheet.Column(1).Width = 18;
                    worksheet.Column(2).Width = 18;
                    worksheet.Column(3).AutoFit();
                    worksheet.Column(3).Width = Math.Max(worksheet.Column(3).Width, 30);
                    worksheet.Column(4).Width = 16;
                    worksheet.Column(5).Width = 16;
                    worksheet.Column(6).Width = 16;
                    worksheet.Column(7).Width = 16;
                    worksheet.Column(8).Width = 16;
                    worksheet.Column(9).Width = 16;
                    worksheet.Column(10).Width = 16;
                    worksheet.Column(11).AutoFit();
                    worksheet.Column(11).Width = Math.Max(worksheet.Column(11).Width, 36);
                    worksheet.Column(12).AutoFit();
                    worksheet.Column(12).Width = Math.Max(worksheet.Column(12).Width, 30);
                    worksheet.Column(13).Width = 18;
                    worksheet.Column(14).AutoFit();
                    worksheet.Column(14).Width = Math.Max(worksheet.Column(14).Width, 54);
                    worksheet.Column(15).Width = 10;
                    worksheet.Column(16).Width = 10;
                    worksheet.Column(17).Width = 12;
                    worksheet.Column(18).Width = 12;
                    worksheet.Column(19).Width = 10;
                    worksheet.Column(20).Width = 10;
                    worksheet.Column(21).Width = 40;

                    excel.SaveAs(new FileInfo(filename));

                    Process.log?.WriteLine("\n{0} of {1} titles exported to {2}", index, titles.Count, filename);
                    Console.Error.WriteLine("\n{0} of {1} titles exported to {2}", index, titles.Count, filename);
                }
            }
            else
            {
                Process.log?.WriteLine("\nExport to {0} file type is not supported", Path.GetExtension(filename));
                Console.Error.WriteLine("\nExport to {0} file type is not supported", Path.GetExtension(filename));
            }
        }
    }
}
