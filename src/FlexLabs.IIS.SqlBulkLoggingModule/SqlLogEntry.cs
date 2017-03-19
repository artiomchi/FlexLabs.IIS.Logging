using System;

namespace FlexLabs.IIS.SqlBulkLoggingModule
{
    class SqlLogEntry
    {
        public DateTime Time { get; set; }
        public string ServerName { get; set; }
        public Guid? SessionID { get; set; }
        public string UserName { get; set; }
        public string RemoteIPAddress { get; set; }
        public int RemoteIPPort { get; set; }
        public string LocalIPAddress { get; set; }
        public int LocalIPPort { get; set; }
        public string SiteName { get; set; }
        public string HostName { get; set; }
        public string Method { get; set; }
        public string UriStem { get; set; }
        public string UriQuery { get; set; }
        public int Status { get; set; }
        public int SubStatus { get; set; }
        public long BytesSent { get; set; }
        public long BytesReceived { get; set; }
        public int ElapsedMilliseconds { get; set; }
        public string UserAgent { get; set; }
        public string Referrer { get; set; }
    }
}
