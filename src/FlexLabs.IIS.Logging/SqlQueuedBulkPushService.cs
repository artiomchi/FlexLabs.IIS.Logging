﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading;

namespace FlexLabs.IIS.Logging
{
    public class SqlQueuedBulkPushService<T> : IDisposable
    {
        private static readonly object _queueLock = new object();
        private static readonly object _flushLock = new object();
        private readonly System.Timers.Timer _timer;
        private List<T> _queue = new List<T>();

        public SqlQueuedBulkPushService(String connectionString, String tableName, Int32 batchSize)
        {
            ConnectionString = connectionString;
            TableName = tableName;
            BatchSize = batchSize;

            _timer = new System.Timers.Timer(30_000)
            {
                AutoReset = true,
            };
            _timer.Elapsed += (s, e) => FlushQueue();
        }

        public string ConnectionString { get; set; }
        public string TableName { get; set; }
        public int BatchSize { get; set; }
        public bool SynchronousBatches { get; set; }
        public bool TimerDisabled { get; set; }

        public void Dispose()
        {
            Logger.DebugWrite("Dispose()");
            _timer.Dispose();
            FlushQueue()?.Join();
        }

        private void StopTimer()
        {
            if (!_timer.Enabled) return;
            Logger.DebugWrite("StopTimer()");
            _timer.Stop();
        }

        private void StartTimer()
        {
            if (_timer.Enabled || TimerDisabled) return;
            Logger.DebugWrite("StartTimer()");
            _timer.Start();
        }

        public void Add(T value)
        {
            Logger.DebugWrite("Add()");
            lock (_queueLock)
            {
                _queue.Add(value);
                if (_queue.Count >= BatchSize)
                    FlushQueue();
                else
                    StartTimer();
            }
        }

        public void AddRange(IEnumerable<T> values)
        {
            Logger.DebugWrite("AddRange()");
            Thread flushThread = null;
            lock (_queueLock)
            {
                _queue.AddRange(values);
                if (_queue.Count >= BatchSize)
                    flushThread = FlushQueue();
                else
                    StartTimer();
            }
            if (SynchronousBatches && flushThread != null)
                flushThread.Join();
        }

        public Thread FlushQueue()
        {
            Logger.DebugWrite("FlushQueue()");
            if (_queue.Count == 0)
                return null;

            var thread = new Thread(() =>
            {
                Logger.DebugWrite("FlushQueue() -> Thread");
                try
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

                        if (_queue.Count == 0)
                            StopTimer();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex);
                }
            });
            thread.Start();
            return thread;
        }

        void BulkPushBatch(IList<T> batch)
        {
            Logger.DebugWrite("BulkPushBatch()");
            using (var reader = new ListDataReader<T>(batch))
            using (var conn = new SqlConnection(ConnectionString))
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
