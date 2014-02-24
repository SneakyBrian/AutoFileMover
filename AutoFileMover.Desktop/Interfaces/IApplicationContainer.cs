using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutoFileMover.Desktop.Interfaces
{
    /// <summary>
    /// Abstraction contract for dealing with the host application
    /// </summary>
    public interface IApplicationContainer
    {
        /// <summary>
        /// Show the main window
        /// </summary>
        void ShowWindow();

        /// <summary>
        /// Exit the application
        /// </summary>
        void Exit();

        /// <summary>
        /// Restart the application
        /// </summary>
        void Restart();
    }
}
