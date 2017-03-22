using FlexLabs.IIS.Logging;
using System;
using System.Collections.Generic;

namespace FlexLabs.IIS.LogIngester
{
    public class W3LogItem : LogItem
    {
        public W3LogItem(Dictionary<string, int> headers, string[] values, string serverName, string siteName, string defaultHost)
            : base(headers, values, serverName, siteName, defaultHost)
        { }

        public DateTime Date => Get<DateTime>("date");
        public TimeSpan Time => Get<TimeSpan>("time");
        public Guid? SessionID => Get<Guid?>("x-session");
        public string UserName => Get("cs-username").NullIfEmpty();
        public string RemoteIPAddress => Get("c-ip") ?? string.Empty;
        public int RemoteIPPort => Get<int>("c-port");
        public string LocalIPAddress => Get("s-ip") ?? string.Empty;
        public int LocalIPPort => Get<int>("s-port");
        public int Status => Get<int>("sc-status");
        public int SubStatus => Get<int>("sc-substatus");
        public string Method => Get("cs-method") ?? string.Empty;
        public string UriStem => Get("cs-uri-stem").NullIfEmpty();
        public int ElapsedMilliseconds => Get<int>("time-taken");
        public long BytesSent => Get<int>("sc-bytes");
        public long BytesReceived => Get<int>("cs-bytes");
        public string HostName => Get("cs-host") ?? DefaultHost ?? string.Empty;
        public string UriQuery => Get("cs-uri-query").NullIfEmpty();
        public string UserAgent => Get("cs(User-Agent)").NullIfEmpty();
        public string Referrer => Get("cs(Referer)").NullIfEmpty();
        public string ReferrerHost
            => Uri.TryCreate(Referrer, UriKind.Absolute, out Uri uri)
                ? uri.Host
                : null;
    }
}
