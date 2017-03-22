using System;
using System.Collections.Generic;

namespace FlexLabs.IIS.LogIngester
{
    public abstract class LogItem
    {
        private readonly Dictionary<string, int> _headers;
        private readonly string[] _values;
        public LogItem(Dictionary<string, int> headers, string[] values, string serverName, string siteName, string defaultHost)
        {
            _headers = headers;
            _values = values;
            ServerName = serverName;
            SiteName = siteName;
            DefaultHost = defaultHost;
        }

        public string ServerName { get; }
        public string SiteName { get; }
        protected string DefaultHost { get; }

        protected string Get(string header)
        {
            if (!_headers.ContainsKey(header))
                return null;
            var index = _headers[header];
            var value = _values[index];
            if (value == "-")
                return null;
            return value;
        }
        protected TValue Get<TValue>(string header)
        {
            var value = Get(header);
            if (value == null)
                return default(TValue);
            if (typeof(TValue) == typeof(TimeSpan))
                return (TValue)Convert.ChangeType(TimeSpan.Parse(value), typeof(TValue));
            if (typeof(TValue) == typeof(Guid))
                return (TValue)Convert.ChangeType(Guid.Parse(value), typeof(TValue));
            if (typeof(TValue) == typeof(int))
                if (int.TryParse(value, out int intValue))
                    return (TValue)Convert.ChangeType(intValue, typeof(TValue));
                else
                    return default(TValue);
            return (TValue)Convert.ChangeType(value, typeof(TValue));
        }
    }
}
