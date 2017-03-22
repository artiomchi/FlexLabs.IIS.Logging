using FlexLabs.IIS.Logging;
using Microsoft.Web.FtpServer;
using System;

namespace FlexLabs.IIS.FtpSqlBulkLogging
{
    class SqlFtpLogEntry
    {
        public SqlFtpLogEntry(FtpLogEntry logEntry)
        {
            Date = DateTime.UtcNow;
            Time = DateTime.UtcNow.TimeOfDay;
            ServerName = Environment.MachineName;
            SessionID = logEntry.SessionId.ParseToGuid();
            UserName = logEntry.UserName.NullIfEmpty();
            RemoteIPAddress = logEntry.RemoteIPAddress ?? string.Empty;
            RemoteIPPort = logEntry.RemoteIPPort;
            LocalIPAddress = logEntry.LocalIPAddress ?? string.Empty;
            LocalIPPort = logEntry.LocalIPPort;
            Information = logEntry.Information.NullIfEmpty();
            HRStatus = logEntry.HRStatus;
            SiteName = logEntry.SiteName ?? string.Empty;
            HostName = logEntry.HostName.NullIfEmpty();
            FtpStatus = logEntry.FtpStatus;
            FtpSubStatus = logEntry.FtpSubStatus;
            Command = logEntry.Command ?? string.Empty;
            CommandParameters = logEntry.CommandParameters.NullIfEmpty();
            ElapsedMilliseconds = logEntry.ElapsedMilliseconds;
            BytesSent = logEntry.BytesSent;
            BytesReceived = logEntry.BytesReceived;
            FullPath = logEntry.FullPath.NullIfEmpty();
        }

        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public Guid SessionID { get; set; }
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
