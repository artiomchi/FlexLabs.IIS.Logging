using Microsoft.Web.FtpServer;
using System;

namespace FlexLabs.IIS.FtpSqlBulkLogging
{
    public class SqlBulkLogging : BaseProvider, IFtpLogProvider
    {
        SqlQueuedBulkPushService<SqlFtpLogEntry> _bulkPushService;
        public SqlBulkLogging()
        {
            _bulkPushService = new SqlQueuedBulkPushService<SqlFtpLogEntry>("ConnectionString", "dbo.FtpLogs", 1000);
        }

        public void Log(FtpLogEntry logEntry)
        {
            var sqlEntry = new SqlFtpLogEntry
            {
                Time = DateTime.UtcNow,
                ServerName = Environment.MachineName,
                SessionID = logEntry.SessionId,
                UserName = logEntry.UserName,
                RemoteIPAddress = logEntry.RemoteIPAddress,
                RemoteIPPort = logEntry.RemoteIPPort,
                LocalIPAddress = logEntry.LocalIPAddress,
                LocalIPPort = logEntry.LocalIPPort,
                Information = logEntry.Information,
                HRStatus = logEntry.HRStatus,
                SiteName = logEntry.SiteName,
                HostName = logEntry.HostName,
                FtpStatus = logEntry.FtpStatus,
                FtpSubStatus = logEntry.FtpSubStatus,
                Command = logEntry.Command,
                CommandParameters = logEntry.CommandParameters,
                ElapsedMilliseconds = logEntry.ElapsedMilliseconds,
                BytesSent = logEntry.BytesSent,
                BytesReceived = logEntry.BytesReceived,
                FullPath = logEntry.FullPath,
            };
            _bulkPushService.Add(sqlEntry);
        }

        protected override void Dispose(bool disposing)
        {
            _bulkPushService.Dispose();
            base.Dispose(disposing);
        }
    }
}
