using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFileMover.Core;
using AutoFileMover.Core.Interfaces;
using AutoFileMover.Desktop.Interfaces;
using UnityConfiguration;

namespace AutoFileMover.Desktop.IoC
{
    public class IoCRegistry : UnityRegistry
    {
        public IoCRegistry()
        {
            Scan(scan =>
            {
                scan.AssembliesInBaseDirectory();
                scan.ForRegistries();
                scan.With<FirstInterfaceConvention>();
            });

            Configure<IEngine>().AsSingleton();
        }
    }
}
