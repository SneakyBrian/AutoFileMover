using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFileMover.Core
{
    public class AppSettingsConfig : IConfig
    {
        private const string SOURCEPATHS = "SourcePaths";
        private const string SOURCEREGEX = "SourceRegex";
        private const string DESTINATIONPATH = "DestinationPath";
        private const string INCLUDESUBDIRECTORIES = "IncludeSubdirectories";
        private const string FILEMOVERETRIES = "FileMoveRetries";

        public IEnumerable<string> SourcePaths
        {
            get { return ConfigurationManager.AppSettings.Get(SOURCEPATHS).Split(',').Where(s => !string.IsNullOrWhiteSpace(s)); }
            set { ConfigurationManager.AppSettings.Set(SOURCEPATHS, value.Aggregate("", (a, b) => a + "," + b)); }
        }

        public IEnumerable<string> SourceRegex
        {
            get { return ConfigurationManager.AppSettings.Get(SOURCEREGEX).Split(',').Where(s => !string.IsNullOrWhiteSpace(s)); }
            set { ConfigurationManager.AppSettings.Set(SOURCEREGEX, value.Aggregate("", (a, b) => a + "," + b)); }
        }

        public string DestinationPath
        {
            get { return ConfigurationManager.AppSettings.Get(DESTINATIONPATH); }
            set { ConfigurationManager.AppSettings.Set(DESTINATIONPATH, value); }
        }

        public bool IncludeSubdirectories
        {
            get { return bool.Parse(ConfigurationManager.AppSettings.Get(DESTINATIONPATH)); }
            set { ConfigurationManager.AppSettings.Set(DESTINATIONPATH, value.ToString()); }
        }

        public int FileMoveRetries
        {
            get { return int.Parse(ConfigurationManager.AppSettings.Get(DESTINATIONPATH)); }
            set { ConfigurationManager.AppSettings.Set(DESTINATIONPATH, value.ToString()); }
        }
    }
}
