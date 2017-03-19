using System;
using System.Diagnostics;

namespace FlexLabs.IIS.Logging
{
    public static class Logger
    {
        private static string Category = typeof(Logger).Assembly.GetName().Name;

        public static bool DebugMode = false;

        public static void DebugWrite(String value)
        {
            if (DebugMode)
                Trace.WriteLine(value, Category);
            else
                Debug.WriteLine(value, Category);
        }
        public static void TraceWrite(String value)
        {
            Trace.WriteLine(value, Category);
        }
        public static void Exception(Exception ex)
        {
            TraceWrite($"[Exception]: {ex.Message}");
            TraceWrite(ex.StackTrace);
        }
    }
}
