using System;
using System.Diagnostics;

namespace FlexLabs.IIS.FtpSqlBulkLogging
{
    public static class Logger
    {
        private const string Category = "FlexLabs.IIS.FtpSqlBulkLogging";

        public static void DebugWrite(String value) => Debug.WriteLine(value, Category);
        public static void TraceWrite(String value) => Trace.WriteLine(value, Category);
        public static void Exception(Exception ex)
        {
            TraceWrite($"[Exception]: {ex.Message}");
            TraceWrite(ex.StackTrace);
        }
    }
}
