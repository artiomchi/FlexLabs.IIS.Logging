using FlexLabs.IIS.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FlexLabs.IIS.LogIngester
{
    class Program
    {
        const int batch_size = 500_000;

        static void Main(string[] args) => new Program().Auto(args);

        void Auto(string[] args)
        {
            Logger.ConsoleExceptions = true;

            var serverName = Prompt("Server");
            var defaultPath = $@"\\{serverName}\c$\inetpub\logs\LogFiles";
            string path;
            do
            {
                path = Prompt($"Path ({defaultPath})", v => true).NullIfEmpty() ?? defaultPath;
                if (Directory.Exists(path))
                    break;
                Console.WriteLine($"  [{path}] is not a valid path!");
            } while (true);

            var redirectionDoc = XDocument.Load($@"\\{serverName}\c$\windows\system32\inetsrv\config\redirection.config");
            Console.WriteLine("redirection.config loaded: " + (redirectionDoc != null));
            var redNote = redirectionDoc.Root.Element("configurationRedirection");
            var configPath = string.Equals(redNote?.Attribute("enabled")?.Value, "true", StringComparison.OrdinalIgnoreCase)
                ? redNote.Attribute("path").Value
                : $@"\\{serverName}\c$\windows\system32\inetsrv\config";
            Console.WriteLine("Config path: " + configPath);

            var sites = XDocument.Load(Path.Combine(configPath, "applicationHost.config"))
                .Root
                .Element("system.applicationHost")
                .Element("sites")
                .Elements("site");

            var siteBindings = sites
                .ToDictionary(
                    e => e.Attribute("id").Value, 
                    e => new
                    {
                        Name = e.Attribute("name").Value,
                        DefaultHost = e.Element("bindings")
                            .Elements("binding")
                            .Select(be => be.Attribute("bindingInformation").Value)
                            .Select(bi => bi.Split(':').Last())
                            .GroupBy(bi => bi.ToLower())
                            .OrderByDescending(g => g.Count())
                            .First()
                            .Key,
                    });
            foreach (var binding in siteBindings)
                Console.WriteLine($"Found site: {binding.Key}: {binding.Value.Name}");

            var ftpSites = Directory.GetDirectories(path, "FTPSVC*")
                .Select(ftpPath =>
                {
                    var id = Path.GetFileName(ftpPath).Substring("FTPSVC".Length);
                    if (!siteBindings.ContainsKey(id))
                    {
                        Console.WriteLine($"  [{id}]: Unknown FTP site!");
                        return null;
                    }
                    return new
                    {
                        id = id,
                        path = ftpPath,
                        siteName = siteBindings[id].Name,
                    };
                })
                .Where(s => s != null)
                .ToArray();
            var w3Sites = Directory.GetDirectories(path, "W3SVC*")
                .Select(w3Path =>
                {
                    var id = Path.GetFileName(w3Path).Substring("W3SVC".Length);
                    if (!siteBindings.ContainsKey(id))
                    {
                        Console.WriteLine($"  [{id}]: Unknown W3 site!");
                        return null;
                    }
                    //var defaultHost = Prompt($"Host for {siteBindings[id]} ({id})", v => true);
                    return new
                    {
                        id = id,
                        path = w3Path,
                        siteName = siteBindings[id].Name,
                        defaultHost = siteBindings[id].DefaultHost,
                    };
                })
                .Where(s => s != null)
                .ToArray();

            Console.WriteLine();
            Console.WriteLine("FTP Sites: " + ftpSites.Length);
            Console.WriteLine("W3 Sites: " + w3Sites.Length);
            if (ftpSites.Length == 0 && w3Sites.Length == 0)
            {
                Console.WriteLine("  No sites found!");
                return;
            }

            var proceed = Prompt("Proceed (y|n)", v => v.ToLower() == "y" || v.ToLower() == "n");
            if (!proceed.Equals("y"))
                return;

            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
            Console.WriteLine(connectionString);
            var sw = Stopwatch.StartNew();
            foreach (var site in ftpSites)
            {
                ProcessLogs<FtpLogItem>(serverName, site.siteName, null, connectionString, "flexlabs.IIS_FtpLogs", Directory.GetFiles(site.path, "*.log"));
            }

            foreach (var site in w3Sites)
            {
                ProcessLogs<W3LogItem>(serverName, site.siteName, site.defaultHost, connectionString, "flexlabs.IIS_WebLogs", Directory.GetFiles(site.path, "*.log"));
            }

            sw.Stop();
            Console.WriteLine($"Uploaded in {sw.Elapsed}");
            Console.WriteLine($"Processed {totalCounter.ToString("N0")} logs at {(totalCounter / sw.Elapsed.TotalSeconds).ToString("N2")} logs/sec");
        }

        void Manual(string[] args)
        {
            var logsPath = Prompt("Logs location", v => Directory.Exists(v));
            var files = Directory.GetFiles(logsPath);
            Console.WriteLine($"  Logs found: {files.Length}");
            if (files.Length == 0)
                return;

            string serverName = null, logType = null;
            if (logsPath.StartsWith("\\"))
            {
                serverName = logsPath.Substring(2, logsPath.IndexOf('\\', 3) - 2);
                Console.WriteLine("Server name: " + serverName);
            }
            else
            {
                serverName = Prompt("Server name");
            }

            var logTypeMatch = Regex.Match(logsPath, $@"^\\\\{serverName}\\c$\\inetpub\\logs\\LogFiles\\(ftp|w3)svc(\d+)\\?$", RegexOptions.IgnoreCase);
            if (logTypeMatch.Success)
            {
                logType = logTypeMatch.Groups[1].Value;
                Console.WriteLine("Log type: " + logType);
            }
            else
            {
                logType = Prompt("Log type (ftp|w3)", v => v.ToLower() == "ftp" || v.ToLower() == "w3");
            }

            var siteName = Prompt("Site name");
            string defaultHost = null;
            if (logType.Equals("w3"))
                defaultHost = Prompt("Default Hostname");

            var proceed = Prompt("Proceed (y|n)", v => v.ToLower() == "y" || v.ToLower() == "n");
            if (!proceed.Equals("y"))
                return;

            var sw = Stopwatch.StartNew();
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
            if (logType.Equals("ftp"))
            {
                ProcessLogs<FtpLogItem>(serverName, siteName, defaultHost, connectionString, "flexlabs.IIS_FtpLogs", files);
            }
            else
            {
                ProcessLogs<W3LogItem>(serverName, siteName, defaultHost, connectionString, "flexlabs.IIS_WebLogs", files);
            }

            sw.Stop();
            Console.WriteLine($"Uploaded in {sw.Elapsed}");
            Console.ReadLine();
        }

        int totalCounter = 0;
        void ProcessLogs<TLogItem>(string serverName, string siteName, string defaultHost, string connectionString, string tableName, string[] files) where TLogItem : LogItem
        {
            var parserLoader = new LogParserLoader<TLogItem>(serverName, siteName, defaultHost);
            using (var bulkPushService = new SqlQueuedBulkPushService<TLogItem>(connectionString, tableName, batch_size) { SynchronousBatches = true, TimerDisabled = true })
            {
                foreach (var file in files)
                {
                    var parser = parserLoader.LoadFile(file);
                    var counter = 0;

                    do
                    {
                        var logs = parser.ReadNext(1000);
                        bulkPushService.AddRange(logs);
                        counter += parser.LinesRead;
                    }
                    while (parser.LinesRead > 0);
                    Console.WriteLine($"[{file}]: {counter}");
                    totalCounter += counter;
                }
            }
        }

        static string Prompt(string message) => Prompt(message, v => !string.IsNullOrWhiteSpace(v));
        static string Prompt(string message, Func<string, bool> validator)
        {
            while (true)
            {
                Console.Write(message + ": ");
                var value = Console.ReadLine();
                if (validator(value))
                    return value;
            }
        }
    }
}