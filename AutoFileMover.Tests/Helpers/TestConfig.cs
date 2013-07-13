using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFileMover.Core.Interfaces;

namespace AutoFileMover.Tests.Helpers
{
    public class TestConfig : IConfig
    {
        public IEnumerable<string> SourcePaths
        {
            get;
            set;
        }

        public IEnumerable<string> SourceRegex
        {
            get;
            set;
        }

        public string DestinationPath
        {
            get;
            set;
        }

        public bool IncludeSubdirectories
        {
            get;
            set;
        }

        public int FileMoveRetries
        {
            get;
            set;
        }

        public int ConcurrentOperations
        {
            get;
            set;
        }

        public bool VerifyFiles
        {
            get;
            set;
        }
    }
}
