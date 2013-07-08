using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFileMover.Core.Interfaces;

namespace AutoFileMover.Desktop.Interfaces
{
    public interface IApplicationConfig : IConfig
    {
        bool AutoStart { get; set; }
        bool AutoClear { get; set; }
    }
}
