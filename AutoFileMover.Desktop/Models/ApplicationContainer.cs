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
        public void ShowWindow()
        {
            App.Current.MainWindow.WindowState = WindowState.Normal;
            App.Current.MainWindow.Activate();
        }

        public void Exit()
        {
            App.Current.Shutdown();
        }
    }
}
