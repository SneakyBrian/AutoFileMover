using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutoFileMover.Desktop.Interfaces
{
    public interface IApplicationContainer
    {
        void ShowWindow();
        void Exit();
        void Restart();

        string EntryPoint { get; set; }
    }
}
