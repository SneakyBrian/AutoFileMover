using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AutoFileMover.Desktop.Interfaces;
using AutoFileMover.Desktop.IoC;
using Microsoft.Practices.Unity;
using UnityConfiguration;

namespace AutoFileMover.Desktop.Models
{
    /// <summary>
    /// Implementation of the application container
    /// </summary>
    /// <remarks>
    /// Uses IoC to resolve IApplicationDeployment internally
    /// </remarks>
    public class ApplicationContainer : IApplicationContainer
    {
        private IApplicationDeployment _deployment;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ApplicationContainer()
        {
            using (var container = new UnityContainer())
            {
                container.Configure(x =>
                {
                    x.AddRegistry<IoCRegistry>();
                });

                _deployment = container.Resolve<IApplicationDeployment>();
            }
        }

        /// <summary>
        /// Shows the main window
        /// </summary>
        public void ShowWindow()
        {
            App.Current.MainWindow.WindowState = WindowState.Normal;
            App.Current.MainWindow.Activate();
        }

        /// <summary>
        /// Exits the application
        /// </summary>
        public void Exit()
        {
            App.Current.Shutdown();
        }

        /// <summary>
        /// Restarts the application
        /// </summary>
        public void Restart()
        {
            //if we are ClickOnce deployed
            if (_deployment.IsNetworkDeployed)
            {
                //Emulate what the windows shell does to launch ClickOnce .application files
                Process.Start("rundll32.exe", "dfshim.dll,ShOpenVerbApplication " + _deployment.UpdatedApplicationFullName);
            }
            else
            {
                //just use the current process main module filename
                Process.Start(Process.GetCurrentProcess().MainModule.FileName);
            }

            //call our exit function
            Exit();
        }
    }
}
