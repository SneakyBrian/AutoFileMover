using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AutoFileMover.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // hook on error before app really starts
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            base.OnStartup(e);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();

            var result = MessageBox.Show("Sorry, an error occurred. Click OK to log the error.", assemblyName.Name, MessageBoxButton.OKCancel, MessageBoxImage.Error);

            if (result == MessageBoxResult.OK)
            {
                var ex = e.ExceptionObject as Exception;

                if (ex != null)
                {
                    var version = assemblyName.Version.ToString();

                    if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                    {
                        version = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                    }

                    System.Diagnostics.Process.Start(string.Format("https://github.com/SneakyBrian/AutoFileMover/issues/new?title={0}&body={1}%20{2}", ex.Message, version, ex));
                }
            }
        }
    }
}
