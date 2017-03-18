using Microsoft.Web.FtpServer;

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
            _bulkPushService.Add(new SqlFtpLogEntry(logEntry));
        }

        protected override void Dispose(bool disposing)
        {
            _bulkPushService.Dispose();
            base.Dispose(disposing);
        }
    }
}
