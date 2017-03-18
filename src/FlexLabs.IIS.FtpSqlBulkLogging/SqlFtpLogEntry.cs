using System;

namespace FlexLabs.IIS.FtpSqlBulkLogging
{
    class SqlFtpLogEntry
    {
        public DateTime Time { get; set; }
        public string SessionID { get; set; }
        public string UserName { get; set; }
        public string RemoteIPAddress { get; set; }
        public int RemoteIPPort { get; set; }
        public string LocalIPAddress { get; set; }
        public int LocalIPPort { get; set; }
        public string Information { get; set; }
        public int HRStatus { get; set; }
        public string SiteName { get; set; }
        public string HostName { get; set; }
        public int FtpStatus { get; set; }
        public int FtpSubStatus { get; set; }
        public string Command { get; set; }
        public string CommandParameters { get; set; }
        public int ElapsedMilliseconds { get; set; }
        public long BytesSent { get; set; }
        public long BytesReceived { get; set; }
        public string FullPath { get; set; }
        public string ServerName { get; set; }
    }
}
