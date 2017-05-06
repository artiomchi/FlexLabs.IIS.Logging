CREATE SCHEMA [flexlabs]

CREATE PARTITION FUNCTION [IISLogsDate](date) AS RANGE LEFT FOR VALUES (
	'2011-01-01', '2011-04-01', '2011-07-01', '2011-10-01', 
	'2012-01-01', '2012-04-01', '2012-07-01', '2012-10-01', 
	'2013-01-01', '2013-04-01', '2013-07-01', '2013-10-01', 
	'2014-01-01', '2014-04-01', '2014-07-01', '2014-10-01', 
	'2015-01-01', '2015-04-01', '2015-07-01', '2015-10-01', 
	'2016-01-01', '2016-04-01', '2016-07-01', '2016-10-01', 
	'2017-01-01', '2017-04-01', '2017-07-01', '2017-10-01',
	'2018-01-01', '2018-04-01', '2018-07-01', '2018-10-01')

CREATE PARTITION SCHEME [IISLogsDate] AS PARTITION [IISLogsDate] ALL TO ([PRIMARY])

CREATE TABLE [flexlabs].[IIS_WebLogs](
    [PK] [bigint] IDENTITY(1,1) NOT NULL,
    [Date] [date] NOT NULL,
    [Time] [time](0) NOT NULL,
    [SessionID] [uniqueidentifier] NULL,
    [UserName] [nvarchar](255) NULL,
    [RemoteIPAddress] [varchar](50) NOT NULL,
    [RemoteIPPort] [int] NOT NULL,
    [LocalIPAddress] [varchar](50) NOT NULL,
    [LocalIPPort] [int] NOT NULL,
    [SiteName] [nvarchar](255) NULL,
    [ServerName] [nvarchar](255) NOT NULL,
    [HostName] [nvarchar](255) NOT NULL,
    [Method] [varchar](15) NOT NULL,
    [UriStem] [nvarchar](2048) NOT NULL,
    [UriQuery] [nvarchar](2048) NULL,
    [Status] [int] NOT NULL,
    [SubStatus] [int] NOT NULL,
    [BytesSent] [bigint] NOT NULL,
    [BytesReceived] [bigint] NOT NULL,
    [ElapsedMilliseconds] [int] NOT NULL,
    [UserAgent] [nvarchar](1024) NULL,
    [Referrer] [nvarchar](2048) NULL,
    [ReferrerHost] [nvarchar](255) NULL,
)

-- Temporary index to force align the table for the columnstore index
CREATE CLUSTERED INDEX [ClusteredIndex_on_IISLogsDate_636254681970734467] ON [flexlabs].[IIS_WebLogs]
(
    [Date]
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [IISLogsDate]([Date])

DROP INDEX [ClusteredIndex_on_IISLogsDate_636254681970734467] ON [flexlabs].[IIS_WebLogs]


CREATE CLUSTERED COLUMNSTORE INDEX [PK_IIS_WebLogs] ON [flexlabs].[IIS_WebLogs] WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0, DATA_COMPRESSION = COLUMNSTORE) ON [IISLogsDate]([Date])
