using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AutoFileMover.Desktop.Interfaces;

namespace AutoFileMover.Desktop.Models
{
    public class ApplicationContainer : IApplicationContainer
    {
        public Application Current
        {
            get { return App.Current; }
        }
    }
}
