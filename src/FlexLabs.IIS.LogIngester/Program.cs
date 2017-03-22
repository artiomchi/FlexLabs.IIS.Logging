using FlexLabs.IIS.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace FlexLabs.IIS.LogIngester
{
    class Program
    {
        static void Main(string[] args) => new Program().Run(args);
        void Run(string[] args)
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

        void ProcessLogs<TLogItem>(string serverName, string siteName, string defaultHost, string connectionString, string tableName, string[] files) where TLogItem : LogItem
        {
            var parserLoader = new LogParserLoader<TLogItem>(serverName, siteName, defaultHost);
            using (var bulkPushService = new SqlQueuedBulkPushService<TLogItem>(connectionString, tableName, 1000) { SynchronousBatches = true })
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