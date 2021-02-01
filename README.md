# Serilog.Settings.AppSettings [![Build status](https://ci.appveyor.com/api/projects/status/lpkpthfap819flva?svg=true)](https://ci.appveyor.com/project/serilog/serilog-settings-appsettings) [![NuGet Version](http://img.shields.io/nuget/v/Serilog.Settings.AppSettings.svg?style=flat)](https://www.nuget.org/packages/Serilog.Settings.AppSettings/) [![Join the chat at https://gitter.im/serilog/serilog](https://img.shields.io/gitter/room/serilog/serilog.svg)](https://gitter.im/serilog/serilog)

An XML `<appSettings>` reader for [Serilog](https://serilog.net).

### Getting started

The `<appSettings>` support package needs to be installed from NuGet:

```powershell
Install-Package Serilog.Settings.AppSettings
```

To read configuration from `<appSettings>` use the `ReadFrom.AppSettings()` extension method on your `LoggerConfiguration`:

```csharp
Log.Logger = new LoggerConfiguration()
  .ReadFrom.AppSettings()
  ... // Other configuration here, then
  .CreateLogger()
```

You can mix and match XML and code-based configuration, but each sink must be configured **either** using XML **or** in code - sinks added in code can't be modified via app settings.

### Configuration syntax

To configure the logger, an `<appSettings>` element should be included in the program's _App.config_ or _Web.config_ file.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="serilog:minimum-level" value="Verbose" />
    <!-- More settings here -->
```

Serilog settings are prefixed with `serilog:`.

### Setting the minimum level

To set the logging level for the app use the `serilog:minimum-level` setting key. 

```xml
    <add key="serilog:minimum-level" value="Verbose" />
```

Valid values are those defined in the `LogEventLevel` enumeration: `Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal`.

### Adding a sink

Sinks are added with the `serilog:write-to` key. The setting name matches the configuration method name that you'd use in code, so the following are equivalent:

```csharp
    .WriteTo.Console()
```

In XML:

```xml
    <add key="serilog:write-to:Console" />
```

**NOTE: When using `serilog:*` keys need to be unique.**

Sink assemblies must be specified using the `serilog:using` syntax. For example, to configure 

```csharp
<add key="serilog:using:Console" value="Serilog.Sinks.Console" />
<add key="serilog:write-to:Console"/>
```

If the sink accepts parameters, these are specified by appending the parameter name to the setting.

```csharp
    .WriteTo.File(@"C:\Logs\myapp-{Date}.txt", retainedFileCountLimit: 10)
```

In XML:

```xml
    <add key="serilog:write-to:File.path" value="C:\Logs\myapp-{Date}.txt" />
    <add key="serilog:write-to:File.retainedFileCountLimit" value="10" />
```

Any environment variables specified in a setting value (e.g. `%TEMP%`) will be expanded appropriately when read.

### Using sink extensions from additional assemblies

To use sinks and enrichers from additional assemblies, specify them with a `serilog:using` key.

For example, to use configuration from the `Serilog.Sinks.EventLog` assembly:

```xml 
    <add key="serilog:using:EventLog" value="Serilog.Sinks.EventLog" />
    <add key="serilog:write-to:EventLog.source" value="Serilog Demo" />
```

### Enriching with properties

To attach additional properties to log events, specify them with the `serilog:enrich:with-property` directive.

For example, to add the property `Release` with the value `"1.2-develop"` to all events:

```xml 
    <add key="serilog:enrich:with-property:Release" value="1.2-develop" />
```

### Adding minimum level overrides

Since Serilog 2.1, [minimum level overrides](https://nblumhardt.com/2016/07/serilog-2-minimumlevel-override/) can be added to change the minimum level for some specific namespaces. This is done with the setting key `serilog:minimum-level:override:` followed by the *source context prefix*.

For instance, the following are equivalent :

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Error)
```

and in XML

```xml
    <add key="serilog:minimum-level" value="Information" />
    <add key="serilog:minimum-level:override:Microsoft" value="Warning" />
    <add key="serilog:minimum-level:override:Microsoft.AspNetCore.Mvc" value="Error" />
```

### Filtering

Filters can be specified using the _Serilog.Filters.Expressions_ package; see the [README](https://github.com/serilog/serilog-filters-expressions) there for more information.

### Setting values from enumerations

When configuring sink settings with values from enumerations, use the member name, without specifying the name of the enumeration.

For example, to configure the `RollingInterval` of the [File Sink](https://github.com/serilog/serilog-sinks-file) to create a new log file per day, use `Day` instead of `RollingInterval.Day`:

```xml
    <add key="serilog:write-to:File.rollingInterval" value="Day"/>
```

If you specify the the name of the enumeration, you'll receive an error similar to `System.ArgumentException: Requested value 'RollingInterval.Day' was not found`
