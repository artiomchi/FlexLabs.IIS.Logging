using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FlexLabs.IIS.LogIngester
{
    interface ILogParser<out TLogItem> where TLogItem : LogItem
    {
        IEnumerable<TLogItem> ParseFile(string file);
    }
    class LogParser<TLogItem> : ILogParser<TLogItem> where TLogItem : LogItem
    {
        private const string HeaderPrefix = "#Fields: ";

        private readonly string _serverName;
        private readonly string _siteName;
        private readonly string _defaultHost;
        public LogParser(string serverName, string siteName, string defaultHost)
        {
            _serverName = serverName;
            _siteName = siteName;
            _defaultHost = defaultHost;
        }

        public IEnumerable<TLogItem> ParseFile(string file)
        {
            using (var fileStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fileStream))
            {
                Dictionary<string, int> headers = null;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line[0] == '#')
                    {
                        if (line.StartsWith(HeaderPrefix))
                        {
                            headers = line.Substring(HeaderPrefix.Length)
                                .Split(' ')
                                .Select((v, i) => (index: i, header: v))
                                .ToDictionary(x => x.header, x => x.index);
                            continue;
                        }

                        headers = null;
                        continue;
                    }

                    var values = line.Split(' ');
                    if (values.Length > 0 && values.Length != headers.Count)
                        throw new Exception("Headers were invalid!");

                    if (typeof(TLogItem) == typeof(FtpLogItem))
                        yield return new FtpLogItem(headers, values, _serverName, _siteName, _defaultHost) as TLogItem;
                    else
                        yield return new W3LogItem(headers, values, _serverName, _siteName, _defaultHost) as TLogItem;
                }
            }
        }
    }
}
