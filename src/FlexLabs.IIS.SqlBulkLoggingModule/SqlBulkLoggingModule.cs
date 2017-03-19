using FlexLabs.IIS.Logging;
using System;
using System.Diagnostics;
using System.Web;

namespace FlexLabs.IIS.SqlBulkLoggingModule
{
    public class SqlBulkLoggingModule : IHttpModule
    {
        SqlQueuedBulkPushService<SqlLogEntry> _bulkPushService;
        public SqlBulkLoggingModule()
        {
            _bulkPushService = new SqlQueuedBulkPushService<SqlLogEntry>("Server=localhost;Integrated Security=true;", "flexlabs.IIS_WebLogs", 1000);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += Context_BeginRequest;
            context.EndRequest += Context_EndRequest;
        }

        private void Context_BeginRequest(object sender, EventArgs e)
        {
            Logger.DebugWrite("Context_BeginRequest()");
            var stopwatch = Stopwatch.StartNew();
            HttpContext.Current.Items["Stopwatch"] = stopwatch;
        }

        private void Context_EndRequest(object sender, EventArgs e)
        {
            Logger.DebugWrite("Context_EndRequest()");
            var context = HttpContext.Current;
            var stopWatch = context.Items["Stopwatch"] as Stopwatch;
            stopWatch?.Stop();
            _bulkPushService.Add(new SqlLogEntry
            {
                Time = DateTime.UtcNow,
                ServerName = Environment.MachineName,
                SessionID = context.Session?.SessionID.ParseToGuid(),
                UserName = (context.User?.Identity?.Name).NullIfEmpty(),
                RemoteIPAddress = context.Request.UserHostAddress,
                RemoteIPPort = TryParseInt(context.Request.ServerVariables["REMOTE_PORT"]),
                LocalIPAddress = context.Request.ServerVariables["LOCAL_ADDR"],
                LocalIPPort = TryParseInt(context.Request.ServerVariables["SERVER_PORT"]),
                SiteName = GetSiteName(),
                HostName = context.Request.Url.Host,
                Method = context.Request.HttpMethod,
                UriStem = context.Request.Url.AbsolutePath,
                UriQuery = context.Request.Url.Query.NullIfEmpty(),
                Status = context.Response.StatusCode,
                SubStatus = context.Response.SubStatusCode,
                BytesSent = TryParseInt(context.Response.Headers["Content-Length"]),
                BytesReceived = TryParseInt(context.Request.ServerVariables["CONTENT_LENGTH"]),
                ElapsedMilliseconds = Convert.ToInt32(stopWatch.ElapsedMilliseconds),
                UserAgent = context.Request.UserAgent,
                Referrer = context.Request.UrlReferrer?.ToString(),
            });
        }

        private string GetSiteName()
        {
            try
            {
                return System.Web.Hosting.HostingEnvironment.ApplicationHost?.GetSiteName();
            }
            catch
            {
                return null;
            }
        }

        private int TryParseInt(string value)
        {
            if (int.TryParse(value, out int intValue))
                return intValue;
            return 0;
        }
    }
}
