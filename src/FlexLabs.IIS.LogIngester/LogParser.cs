using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FlexLabs.IIS.LogIngester
{
    class LogParserLoader<TLogItem> where TLogItem : LogItem
    {
        private readonly string _serverName;
        private readonly string _siteName;
        private readonly string _defaultHost;
        public LogParserLoader(string serverName, string siteName, string defaultHost)
        {
            _serverName = serverName;
            _siteName = siteName;
            _defaultHost = defaultHost;
        }

        public LogParser<TLogItem> LoadFile(string file)
        {
            return new LogParser<TLogItem>(file, _serverName, _siteName, _defaultHost);
        }
    }

    class LogParser<TLogItem> : IDisposable where TLogItem : LogItem
    {
        private const string HeaderPrefix = "#Fields: ";

        private readonly string _serverName;
        private readonly string _siteName;
        private readonly string _defaultHost;
        private Stream _fileStream;
        private StreamReader _reader;
        private Dictionary<string, int> _headers = null;
        public LogParser(string file, string serverName, string siteName, string defaultHost)
        {
            _serverName = serverName;
            _siteName = siteName;
            _defaultHost = defaultHost;

            _fileStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _reader = new StreamReader(_fileStream);
        }

        public int LinesRead { get; private set; }

        public void Dispose()
        {
            _fileStream.Dispose();
            _reader.Dispose();
        }

        public IEnumerable<TLogItem> ReadNext(int batchSize)
        {
            LinesRead = 0;
            while (!_reader.EndOfStream && batchSize-- > 0)
            {
                LinesRead++;
                var line = _reader.ReadLine();
                if (line[0] == '#')
                {
                    if (line.StartsWith(HeaderPrefix))
                    {
                        _headers = line.Substring(HeaderPrefix.Length)
                            .Split(' ')
                            .Select((v, i) => (index: i, header: v))
                            .ToDictionary(x => x.header, x => x.index);
                        continue;
                    }

                    _headers = null;
                    continue;
                }

                var values = line.Split(' ');
                if (values.Length > 0 && values.Length != _headers.Count)
                    throw new Exception("Headers were invalid!");

                if (typeof(TLogItem) == typeof(FtpLogItem))
                    yield return new FtpLogItem(_headers, values, _serverName, _siteName, _defaultHost) as TLogItem;
                else
                    yield return new W3LogItem(_headers, values, _serverName, _siteName, _defaultHost) as TLogItem;
            }
        }
    }
}
