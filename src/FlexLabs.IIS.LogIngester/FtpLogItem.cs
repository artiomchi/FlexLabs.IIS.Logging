using FlexLabs.IIS.Logging;
using System;
using System.Collections.Generic;

namespace FlexLabs.IIS.LogIngester
{
    public class FtpLogItem : LogItem
    {
        public FtpLogItem(Dictionary<string, int> headers, string[] values, string serverName, string siteName, string defaultHost)
            : base(headers, values, serverName, siteName, defaultHost)
        { }

        public DateTime Date => Get<DateTime>("date");
        public TimeSpan Time => Get<TimeSpan>("time");
        public Guid SessionID => Get<Guid>("x-session");
        public string UserName => Get("cs-username").NullIfEmpty();
        public string RemoteIPAddress => Get("c-ip") ?? string.Empty;
        public int RemoteIPPort => Get<int>("c-port");
        public string LocalIPAddress => Get("s-ip") ?? string.Empty;
        public int LocalIPPort => Get<int>("s-port");
        public int HRStatus => Get<int>("sc-win32-status");
        public int FtpStatus => Get<int>("sc-status");
        public int FtpSubStatus => Get<int>("sc-substatus");
        public string Command => Get("cs-method") ?? string.Empty;
        public string CommandParameters => Get("cs-uri-stem").NullIfEmpty();
        public int ElapsedMilliseconds => Get<int>("time-taken");
        public long BytesSent => Get<int>("sc-bytes");
        public long BytesReceived => Get<int>("cs-bytes");
        public string FullPath => Get("x-fullpath").NullIfEmpty();
        public string HostName => Get("cs-host");
    }
}
