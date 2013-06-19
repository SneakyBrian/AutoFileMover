using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFileMover.Core;
using UnityConfiguration;

namespace AutoFileMover.Desktop.IoC
{
    public class IoCRegistry : UnityRegistry
    {
        public IoCRegistry()
        {
            //scan for implementations
            Scan(scan =>
            {
                scan.AssembliesInBaseDirectory();
                scan.ForRegistries();
                scan.With<FirstInterfaceConvention>();
            });

            //configure the engine as a singleton
            Configure<IEngine>().AsSingleton();
        }
    }
}
