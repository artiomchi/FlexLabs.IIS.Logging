using Microsoft.Web.FtpServer;
using System;

namespace FlexLabs.IIS.FtpSqlBulkLogging
{
    class SqlFtpLogEntry
    {
        public SqlFtpLogEntry(FtpLogEntry logEntry)
        {
            Time = DateTime.UtcNow;
            ServerName = Environment.MachineName;
            SessionID = logEntry.SessionId;
            UserName = logEntry.UserName;
            RemoteIPAddress = logEntry.RemoteIPAddress;
            RemoteIPPort = logEntry.RemoteIPPort;
            LocalIPAddress = logEntry.LocalIPAddress;
            LocalIPPort = logEntry.LocalIPPort;
            Information = logEntry.Information;
            HRStatus = logEntry.HRStatus;
            SiteName = logEntry.SiteName;
            HostName = logEntry.HostName;
            FtpStatus = logEntry.FtpStatus;
            FtpSubStatus = logEntry.FtpSubStatus;
            Command = logEntry.Command;
            CommandParameters = logEntry.CommandParameters;
            ElapsedMilliseconds = logEntry.ElapsedMilliseconds;
            BytesSent = logEntry.BytesSent;
            BytesReceived = logEntry.BytesReceived;
            FullPath = logEntry.FullPath;
        }

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
