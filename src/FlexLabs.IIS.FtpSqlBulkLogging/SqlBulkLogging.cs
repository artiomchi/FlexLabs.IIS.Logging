using Microsoft.Web.FtpServer;
using System.Collections.Specialized;

namespace FlexLabs.IIS.FtpSqlBulkLogging
{
    public class SqlBulkLogging : BaseProvider, IFtpLogProvider
    {
        SqlQueuedBulkPushService<SqlFtpLogEntry> _bulkPushService;
        public SqlBulkLogging()
        {
            _bulkPushService = new SqlQueuedBulkPushService<SqlFtpLogEntry>("Server=localhost;Integrated Security=true;", "flexlabs.IIS_FtpLogs", 1000);
        }

        protected override void Initialize(StringDictionary config)
        {
            Logger.DebugWrite("Initialize()");
            var connectionString = config["ConnectionString"];
            if (connectionString != null)
                _bulkPushService.ConnectionString = connectionString;

            var tableName = config["TableName"];
            if (tableName != null)
                _bulkPushService.TableName = tableName;

            var batchSize = config["BatchSize"];
            if (int.TryParse(batchSize, out int batchSizeInt))
                _bulkPushService.BatchSize = batchSizeInt;
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
