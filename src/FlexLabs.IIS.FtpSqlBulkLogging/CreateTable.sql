CREATE SCHEMA [flexlabs]

CREATE PARTITION FUNCTION [IISLogsDate](datetime) AS RANGE LEFT FOR VALUES (N'2017-01-01T00:00:00', N'2017-04-01T00:00:00', N'2017-07-01T00:00:00', N'2017-10-01T00:00:00', N'2018-01-01T00:00:00')

CREATE PARTITION SCHEME [IISLogsDate] AS PARTITION [IISLogsDate] ALL TO ([PRIMARY])

CREATE TABLE [flexlabs].[IIS_FtpLogs](
    [PK] [bigint] IDENTITY(1,1) NOT NULL,
    [Time] [datetime] NOT NULL,
    [SessionID] uniqueidentifier NOT NULL,
    [UserName] [nvarchar](255) NULL,
    [RemoteIPAddress] [varchar](50) NOT NULL,
    [RemoteIPPort] [int] NOT NULL,
    [LocalIPAddress] [varchar](50) NOT NULL,
    [LocalIPPort] [int] NOT NULL,
    [Information] [nvarchar](1000) NULL,
    [HRStatus] [int] NOT NULL,
    [SiteName] [nvarchar](255) NOT NULL,
    [HostName] [nvarchar](255) NULL,
    [FtpStatus] [int] NOT NULL,
    [FtpSubStatus] [int] NOT NULL,
    [Command] [nvarchar](255) NOT NULL,
    [CommandParameters] [nvarchar](255) NULL,
    [ElapsedMilliseconds] [int] NOT NULL,
    [BytesSent] [bigint] NOT NULL,
    [BytesReceived] [bigint] NOT NULL,
    [FullPath] [nvarchar](255) NULL,
    [ServerName] [nvarchar](255) NOT NULL
)

-- Temporary index to force align the table for the columnstore index
CREATE CLUSTERED INDEX [ClusteredIndex_on_IISLogsDate_636254681970734467] ON [flexlabs].[IIS_FtpLogs]
(
    [Time]
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [IISLogsDate]([Time])

DROP INDEX [ClusteredIndex_on_IISLogsDate_636254681970734467] ON [flexlabs].[IIS_FtpLogs]


CREATE CLUSTERED COLUMNSTORE INDEX [PK_IIS_FtpLogs] ON [flexlabs].[IIS_FtpLogs] WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0, DATA_COMPRESSION = COLUMNSTORE) ON [IISLogsDate]([Time])

ALTER TABLE [flexlabs].[IIS_FtpLogs] REBUILD PARTITION = 1 WITH(DATA_COMPRESSION = COLUMNSTORE_ARCHIVE )
