using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFileMover.Core.Interfaces
{
    public interface IConfig
    {
        IEnumerable<string> SourcePaths { get; set; }
        IEnumerable<string> SourceRegex { get; set; }
        string DestinationPath { get; set; }
        bool IncludeSubdirectories { get; set; }
        int FileMoveRetries { get; set; }
        TimeSpan TimeBetweenRetries { get; set; }
        int ConcurrentOperations { get; set; }
        bool VerifyFiles { get; set; }
    }
}
