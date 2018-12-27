using System;
using System.Collections.Generic;
using System.IO;
using Serilog.Events;
using Serilog.Tests.Support;
using Serilog.Context;
using Xunit;

namespace Serilog.Tests.AppSettings.Tests
{
    public class AppSettingsTests
    {
        static string GetConfigPath()
        {
            const string testsConfig = "tests.config";
            if (File.Exists(testsConfig))
                return Path.GetFullPath(testsConfig);
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            return Path.GetFullPath(Path.Combine(basePath, testsConfig));
        }

        [Fact]
        public void TheTestConfigFileExists()
        {
            var config= GetConfigPath();
            Assert.True(File.Exists(config), "Can't find the test configuration file");
        }

        [Fact]
        public void EnvironmentVariableExpansionIsApplied()
        {
            var propertyValuesDict = new Dictionary<string, string>();

            propertyValuesDict.Add("prop1", "a");
            propertyValuesDict.Add("prop2", "b");

            LogEvent evt = null;
            var log = new LoggerConfiguration()
                .ReadFrom.AppSettings(filePath: GetConfigPath(), propertyValuesDict: propertyValuesDict) 
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            log.Information("Has a Path property with value expanded from the environment variable");

            Assert.NotNull(evt);
            Assert.NotEmpty((string)evt.Properties["Path"].LiteralValue());
            Assert.NotEqual("%PATH%", evt.Properties["Path"].LiteralValue());
        }

        [Fact]
        public void PropertySubstitutionIsApplied()
        {
            var propertyValuesDict = new Dictionary<string, string>();

            propertyValuesDict.Add("prop1", "a");
            propertyValuesDict.Add("prop2", "b");

            LogEvent evt = null;
            var log = new LoggerConfiguration()
                .ReadFrom.AppSettings(filePath: GetConfigPath(), propertyValuesDict: propertyValuesDict)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            log.Information("Has a PropertySubstitution property with value passed in from caller");

            Assert.NotNull(evt);
            Assert.NotEmpty((string)evt.Properties["PropertySubstitution"].LiteralValue());
            Assert.Equal("a and b", evt.Properties["PropertySubstitution"].LiteralValue());
        }

        [Fact]
        public void PropertySubstitutionIsNotNecessary()
        {
            var propertyValuesDict = new Dictionary<string, string>();

            propertyValuesDict.Add("prop1", "a");
            propertyValuesDict.Add("prop2", "b");

            LogEvent evt = null;
            var log = new LoggerConfiguration()
                .ReadFrom.AppSettings(filePath: GetConfigPath(), propertyValuesDict: propertyValuesDict)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            log.Information("Has a NoPropertySubstitution property with no substitutions");

            Assert.NotNull(evt);
            Assert.NotEmpty((string)evt.Properties["NoPropertySubstitution"].LiteralValue());
            Assert.Equal("Nothing here", evt.Properties["NoPropertySubstitution"].LiteralValue());
        }

        [Fact]
        public void PropertySubstitutionFailsIfNoValuesPassedIn()
        {
            Assert.Throws<KeyNotFoundException>(() => new LoggerConfiguration().ReadFrom.AppSettings(filePath: GetConfigPath()));
        }

        [Fact]
        public void CanUseCustomPrefixToConfigureSettings()
        {
            const string prefix1 = "custom1";
            const string prefix2 = "custom2";

            var log1 = new LoggerConfiguration()
                .WriteTo.Observers(o => { })
                .ReadFrom.AppSettings(prefix1, filePath: GetConfigPath())
                .CreateLogger();

            var log2 = new LoggerConfiguration()
                .WriteTo.Observers(o => { })
                .ReadFrom.AppSettings(prefix2, filePath: GetConfigPath())
                .CreateLogger();

            Assert.False(log1.IsEnabled(LogEventLevel.Information));
            Assert.True(log1.IsEnabled(LogEventLevel.Warning));

            Assert.False(log2.IsEnabled(LogEventLevel.Warning));
            Assert.True(log2.IsEnabled(LogEventLevel.Error));
        }

        [Fact]
        public void CustomPrefixCannotContainColon()
        {
            Assert.Throws<ArgumentException>(() =>
                new LoggerConfiguration().ReadFrom.AppSettings("custom1:custom2", filePath: GetConfigPath()));
        }

        [Fact]
        public void CustomPrefixCannotBeSerilog()
        {
            Assert.Throws<ArgumentException>(() =>
                new LoggerConfiguration().ReadFrom.AppSettings("serilog", filePath: GetConfigPath()));
        }

        [Fact]
        public void ThreadIdEnricherIsApplied()
        {
            var propertyValuesDict = new Dictionary<string, string>();

            propertyValuesDict.Add("prop1", "a");
            propertyValuesDict.Add("prop2", "b");

            LogEvent evt = null;
            var log = new LoggerConfiguration()
                .ReadFrom.AppSettings(filePath: GetConfigPath(), propertyValuesDict: propertyValuesDict)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            log.Information("Has a ThreadId property with value generated by ThreadIdEnricher");

            Assert.NotNull(evt);
            Assert.NotNull(evt.Properties["ThreadId"]);
            Assert.NotNull(evt.Properties["ThreadId"].LiteralValue() as int?);
        }

        [Fact]
        public void MachineNameEnricherIsApplied()
        {
            var propertyValuesDict = new Dictionary<string, string>();

            propertyValuesDict.Add("prop1", "a");
            propertyValuesDict.Add("prop2", "b");

            LogEvent evt = null;
            var log = new LoggerConfiguration()
                .ReadFrom.AppSettings(filePath: GetConfigPath(), propertyValuesDict: propertyValuesDict)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            log.Information("Has a MachineName property with value generated by MachineNameEnricher");

            Assert.NotNull(evt);
            Assert.NotNull(evt.Properties["MachineName"]);
            Assert.NotEmpty((string)evt.Properties["MachineName"].LiteralValue());
        }

        [Fact]
        public void EnrivonmentUserNameEnricherIsApplied()
        {
            var propertyValuesDict = new Dictionary<string, string>();

            propertyValuesDict.Add("prop1", "a");
            propertyValuesDict.Add("prop2", "b");

            LogEvent evt = null;
            var log = new LoggerConfiguration()
                .ReadFrom.AppSettings(filePath: GetConfigPath(), propertyValuesDict: propertyValuesDict)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            log.Information("Has a EnrivonmentUserName property with value generated by EnrivonmentUserNameEnricher");

            Assert.NotNull(evt);
            Assert.NotNull(evt.Properties["EnvironmentUserName"]);
            Assert.NotEmpty((string)evt.Properties["EnvironmentUserName"].LiteralValue());
        }

        [Fact]
        public void ProcessIdEnricherIsApplied()
        {
            var propertyValuesDict = new Dictionary<string, string>();

            propertyValuesDict.Add("prop1", "a");
            propertyValuesDict.Add("prop2", "b");

            LogEvent evt = null;
            var log = new LoggerConfiguration()
                .ReadFrom.AppSettings(filePath: GetConfigPath(), propertyValuesDict: propertyValuesDict)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            log.Information("Has a ProcessId property with value generated by ProcessIdEnricher");

            Assert.NotNull(evt);
            Assert.NotNull(evt.Properties["ProcessId"]);
            Assert.NotNull(evt.Properties["ProcessId"].LiteralValue() as int?);
        }

        [Fact]
        public void LogContextEnricherIsApplied()
        {
            var propertyValuesDict = new Dictionary<string, string>();

            propertyValuesDict.Add("prop1", "a");
            propertyValuesDict.Add("prop2", "b");

            LogEvent evt = null;
            var log = new LoggerConfiguration()
                .ReadFrom.AppSettings(filePath: GetConfigPath(), propertyValuesDict: propertyValuesDict)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            using (LogContext.PushProperty("A", 1))
            {
                log.Information("Has a LogContext property with value generated by LogContextEnricher");
            }

            Assert.NotNull(evt);
            Assert.NotNull(evt.Properties["A"]);
            Assert.NotNull(evt.Properties["A"].LiteralValue() as int?);
            Assert.Equal(1, (int)evt.Properties["A"].LiteralValue());
        }
    }
}
