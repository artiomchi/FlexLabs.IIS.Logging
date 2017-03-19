# IIS SQL Bulk Logging

> This project is currently in alpha stage, and has some final issues being sorted out. Even though it has gone through some basic testing, and shouldn't cause any issues with your system, I can give no guarantees regarding that. Use at your own risk.

This project contains two modules for IIS that will stream your logs into a SQL database. Typically streaming the logs directly to SQL was implemented using ODBC logging, and was strongly not recommended. That was due to the fact that it would cause performance issues, especially on high traffic hosts.

This project attempts to solve that by pushing the logs to the server asynchronously, in batches, either when the batch size is reached, or at specific intervals in time. With the default configuration, it will either flush the logs when there are at least 1000 log entries collected, or once every 30 seconds.

The timed batches are implemented using a timer class, and the timer is paused if the batch queue is empty, preventing any unnecessary resource drain.

The table structures suggested for this module use columnstore indexes to reduce the space used by the tables, as well as to increase the performance of querying the data. They are also partitioned by the calendar quarter, although currently the partitioning has to be performed manually.

Both modules can use the same tables for multiple websites and from multiple servers at the same time. The module will store the server name and the site name (if possible), so that you can query the logs from various sites, without the need for separate databases, although keeping it separate is of course also supported.)

## Web request logging

The web request logging module is implemented as an HTTP Module. There are two ways that you can install this module:

 1. You can bundle it with your application, add it to your bin directory, and then register it in your web.config as follows:
```xml
<configuration>
  <system.webServer>
    <modules>
      <add name="SqlBulkLoggingModule" type="FlexLabs.IIS.SqlBulkLoggingModule.SqlBulkLoggingModule, FlexLabs.IIS.SqlBulkLoggingModule, Culture=neutral, PublicKeyToken=7cb1bef1f9e47ae6"/>
    </modules>
  </system.webServer>
</configuration>
```

2. You can register it for the whole webserver, which will enable logging from ALL the websites on this server, unless they explicitly disabled it in their web.config. To do this you have to register the dll in your GAC. Then in IIS manager, navigate to your server -> Modules -> Add Managed Module….  
Use `SqlBulkLoggingModule` as a module name and `FlexLabs.IIS.SqlBulkLoggingModule.SqlBulkLoggingModule, FlexLabs.IIS.SqlBulkLoggingModule, Culture=neutral, PublicKeyToken=7cb1bef1f9e47ae6` for module type.

Some things to be aware of:

 * The module will only be loaded if the app pool for the website is using .NET. If you chose `No Managed Code` in the app pool settings, this module will not be loaded.
 * The module makes an attempt to identify the site it's running under, but for some sites the hosting environment currently returns null.

### Configuration

This module loads the configuration from web.config. Note that if you're installing it for the whole server, you can set these settings in the system's configuration by editing the system-wide `web.config` file (`C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Config\Web.config`)

```xml
<configuration>
  <!-- Below you can see the default settings if none are provided -->
  <connectionStrings>
    <!-- Primary connection string setting -->
    <add name="FlexLabs.IIS.SqlBulkLogging" connectionString="Server=localhost;Database=IISLogs;Integrated Security=true;" />
  </connectionStrings>
  <appSettings>
    <!-- Fallback connection string, if the primary was not specified -->
    <add key="FlexLabs.IIS.SqlBulkLogging.ConnectionString" value=""/>
    <!-- The table name where data will be pushed to -->
    <add key="FlexLabs.IIS.SqlBulkLogging.TableName" value="flexlabs.IIS_WebLogs"/>
    <!-- Default batch size soft limit -->
    <add key="FlexLabs.IIS.SqlBulkLogging.BatchSize" value="1000"/>
  </appSettings>
</configuration>
```

This module doesn't require any elevated permissions to the database, so the database table will have to be created in advance. You can see the script to create the table in [CreateTable.sql](blob/master/src/FlexLabs.IIS.SqlBulkLoggingModule/CreateTable.sql)

## FTP request logging

The FTP request logging module is implemented as a custom feature provider. It has to be registered and configured on each ftp site separately.

To register the module you will first have to add the `FlexLabs.IIS.FtpSqlBulkLogging` assembly to GAC, and then register it on your server. Navigate to your server (not site) in IIS -> FTP -> Authentication -> Custom Providers…. Click on the Register button, and add the `SqlBulkLogging` provider. Pick `Managed Provider (.NET)`, and enter the following type: `FlexLabs.IIS.FtpSqlBulkLogging.SqlBulkLogging, FlexLabs.IIS.FtpSqlBulkLogging, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7cb1bef1f9e47ae6`.

**Important:** Make sure to _unselect_ the provider from that dialog.

To enable the provider for a site, navigate to the FTP site in IIS manager and open the Configuration manager. In the _section_ box on the top navigate to `system.ftpServer/providerDefinitions`. Open the activation section, and add a new provider. Use the same name you used above(!): `SqlBulkLogging`. Then open the collection section for the provider you added and add a new entry in the list, named `ConnectionString`, populating it with the connection string of the SQL server.

### Configuration

In each site's `system.ftpServer/provicerDefinitions/activation['SqlBulkLogging']` collection, you can add several configuration properties:

 * **ConnectionString**: The connection string to be used by the module for this site
 * **TableName**: Table where the data will be stored
 * **BatchSize**: Default batch size soft limit

This module doesn't require any elevated permissions to the database, so the database table will have to be created in advance. You can see the script to create the table in [CreateTable.sql](blob/master/src/FlexLabs.IIS.FtpSqlBulkLogging/CreateTable.sql)
