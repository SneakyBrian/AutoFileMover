using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFileMover.Core.Interfaces
{
    public interface IConfig
    {
        IEnumerable<string> SourcePaths { get; }
        IEnumerable<string> SourceRegex { get; }
        string DestinationPath { get; }
        bool IncludeSubdirectories { get; }
        int FileMoveRetries { get; }
    }
}
