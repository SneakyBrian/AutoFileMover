using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace AutoFileMover.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IDisposable errorHandler = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            TextElement.FontFamilyProperty.OverrideMetadata(typeof(TextElement), new FrameworkPropertyMetadata(new FontFamily("Segoe UI")));
            TextBlock.FontFamilyProperty.OverrideMetadata(typeof(TextBlock), new FrameworkPropertyMetadata(new FontFamily("Segoe UI")));

            if (!System.Diagnostics.Debugger.IsAttached)
            {
                // hook on error before app really starts
                Observable.FromEventPattern<UnhandledExceptionEventHandler, UnhandledExceptionEventArgs>(h => AppDomain.CurrentDomain.UnhandledException += h,
                                                                                                         h => AppDomain.CurrentDomain.UnhandledException -= h)
                            .Select(evt => evt.EventArgs.ExceptionObject)
                            .Cast<Exception>()
                            .Subscribe(ex => 
                            {
                                var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();

                                var result = MessageBox.Show("Sorry, an error occurred.\nClick OK to log the error.", 
                                                                assemblyName.Name, MessageBoxButton.OKCancel, MessageBoxImage.Error);

                                if (result == MessageBoxResult.OK)
                                {
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
 
                            });
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            //tear down the error handler
            if (errorHandler != null)
            {
                errorHandler.Dispose();
            }

            base.OnExit(e);
        }
    }
}
