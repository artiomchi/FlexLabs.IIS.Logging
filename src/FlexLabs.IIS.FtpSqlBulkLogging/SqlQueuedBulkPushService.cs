using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading;

namespace FlexLabs.IIS.FtpSqlBulkLogging
{
    class SqlQueuedBulkPushService<T> : IDisposable
    {
        private static readonly object _queueLock = new object();
        private static readonly object _flushLock = new object();
        private List<T> _queue = new List<T>();

        public SqlQueuedBulkPushService(String connectionString, String tableName, Int32 batchSize)
        {
            ConnecyionString = connectionString;
            TableName = tableName;
            BatchSize = batchSize;
        }

        public string ConnecyionString { get; private set; }
        public string TableName { get; private set; }
        public int BatchSize { get; private set; }

        public void Dispose() => FlushQueue().Join();

        public void Add(T value)
        {
            lock (_queueLock)
            {
                _queue.Add(value);
                if (_queue.Count >= BatchSize)
                    FlushQueue();
            }
        }

        public void AddRange(IEnumerable<T> values)
        {
            lock (_queueLock)
            {
                _queue.AddRange(values);
                if (_queue.Count >= BatchSize)
                    FlushQueue();
            }
        }

        public Thread FlushQueue()
        {
            var thread = new Thread(() =>
            {
                lock (_flushLock)
                {
                    if (_queue.Count == 0)
                        return;

                    List<T> batch;
                    lock (_queueLock)
                    {
                        batch = _queue;
                        _queue = new List<T>();
                    }

                    if (batch.Count == 0)
                        return;

                    BulkPushBatch(batch);
                    batch.Clear();
                    batch = null;
                }
            });
            thread.Start();
            return thread;
        }

        void BulkPushBatch(IList<T> batch)
        {
            using (var reader = new ListDataReader<T>(batch))
            using (var conn = new SqlConnection(ConnecyionString))
            using (var copy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, null)
            {
                BulkCopyTimeout = 30,
                DestinationTableName = TableName,
                BatchSize = BatchSize,
            })
            {
                foreach (var prop in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    copy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(prop.Name, prop.Name));

                conn.Open();
                copy.WriteToServer(reader);
            }
        }
    }
}
