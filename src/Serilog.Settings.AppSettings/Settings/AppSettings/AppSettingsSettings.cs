// Copyright 2013-2015 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog.Configuration;
using Serilog.Debugging;

namespace Serilog.Settings.AppSettings
{
    class AppSettingsSettings : ILoggerSettings
    {
        readonly string _filePath;
        readonly string _settingPrefix;
        private readonly Dictionary<string, string> _propertyValuesDict;

        public AppSettingsSettings(string settingPrefix = null, string filePath = null, Dictionary<string, string> propertyValuesDict = null)
        {
            _filePath = filePath;
            _settingPrefix = settingPrefix == null ? "serilog:" : $"{settingPrefix}:serilog:";
            _propertyValuesDict = propertyValuesDict ?? new Dictionary<string, string>();
        }

        public void Configure(LoggerConfiguration loggerConfiguration)
        {
            if (loggerConfiguration == null) throw new ArgumentNullException(nameof(loggerConfiguration));

            IEnumerable<KeyValuePair<string, string>> settings;

            if (!string.IsNullOrWhiteSpace(_filePath))
            {
                if (!File.Exists(_filePath))
                {
                    SelfLog.WriteLine("The specified configuration file `{0}` does not exist and will be ignored.", _filePath);
                    return;
                }

                var map = new ExeConfigurationFileMap { ExeConfigFilename = _filePath };
                var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                settings = config.AppSettings.Settings
                    .Cast<KeyValueConfigurationElement>()
                    .Select(k => new KeyValuePair<string, string>(k.Key, k.Value));
            }
            else
            {
                settings = ConfigurationManager.AppSettings.AllKeys
                    .Select(k => new KeyValuePair<string, string>(k, ConfigurationManager.AppSettings[k]));
            }

            var pairs = settings
                .Where(k => k.Key.StartsWith(_settingPrefix))
                .Select(k => new KeyValuePair<string, string>(
                    k.Key.Substring(_settingPrefix.Length),
                    SubstituteVariables(k.Value, _propertyValuesDict)));

            loggerConfiguration.ReadFrom.KeyValuePairs(pairs);
        }

        private string SubstituteVariables(string value, Dictionary<string, string> propertyValuesDict)
        {
            var substitutedStr = Environment.ExpandEnvironmentVariables(value);
            var matches = Regex.Matches(substitutedStr, @"\%property\{([^\}]+)\}");

            foreach (Match match in matches)
            {
                if(!propertyValuesDict.ContainsKey(match.Groups[1].Value))
                    throw new KeyNotFoundException($"Can not find match for property '{match.Groups[1].Value}' referenced in configuration file. Property must be passed in.");

                substitutedStr = substitutedStr.Replace(match.Value, propertyValuesDict[match.Groups[1].Value]);
            }

            return substitutedStr;
        }
    }
}
