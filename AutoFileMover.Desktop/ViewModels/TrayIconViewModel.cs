using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AutoFileMover.Core.Interfaces;
using AutoFileMover.Desktop.Interfaces;
using AutoFileMover.Desktop.IoC;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Practices.Unity;
using ReactiveUI;
using ReactiveUI.Xaml;
using UnityConfiguration;

namespace AutoFileMover.Desktop.ViewModels
{
    public class TrayIconViewModel : ReactiveObject
    {
        private IEngine _engine;
        private IApplicationConfig _appConfig;
        private IApplicationContainer _appContainer;

        private ObservableAsPropertyHelper<string> _icon;
        public string Icon
        {
            get { return _icon.Value; }
        }

        public IReactiveDerivedList<FileOperationViewModel> FileOperations { get; private set; }

        public ReactiveCommand ShowWindow { get; private set; }
        public ReactiveCommand Exit { get; private set; }

        public TrayIconViewModel(IEngine engine, IApplicationConfig config)
        {
            using (var container = new UnityContainer())
            {
                container.Configure(x =>
                {
                    x.AddRegistry<IoCRegistry>();
                });

                _appContainer = container.Resolve<IApplicationContainer>();
            }

            _engine = engine;
            _appConfig = config;

            Initialise(_engine, _appConfig, _appContainer);
        }

        public TrayIconViewModel(IEngine engine, IApplicationConfig config, IApplicationContainer appContainer)
        {
            _engine = engine;
            _appConfig = config;
            _appContainer = appContainer;

            Initialise(_engine, _appConfig, _appContainer);
        }

        public void Initialise(IEngine engine, IApplicationConfig config, IApplicationContainer appContainer)
        {
            var defaultIcon = "/AutoFileMover.Desktop;component/Images/AutoFileMover.ico";
            var errorIcon = "/AutoFileMover.Desktop;component/Images/AutoFileMover_Error.ico";
            var progressIcon = "/AutoFileMover.Desktop;component/Images/AutoFileMover_InProgress.ico";

            _icon = Observable.Merge(Observable.FromEventPattern<EventHandler<FileEventArgs>, FileEventArgs>(h => engine.FileMoveStarted += h, h => engine.FileMoveStarted -= h).Select(e => progressIcon),
                                    Observable.FromEventPattern<EventHandler<FileMoveEventArgs>, FileMoveEventArgs>(h => engine.FileMoveProgress += h, h => engine.FileMoveProgress -= h).Select(e => progressIcon),
                                    Observable.FromEventPattern<EventHandler<FileEventArgs>, FileEventArgs>(h => engine.FileMoveCompleted += h, h => engine.FileMoveCompleted -= h).Select(e => defaultIcon),
                                    Observable.FromEventPattern<EventHandler<FileErrorEventArgs>, FileErrorEventArgs>(h => engine.FileMoveError += h, h => engine.FileMoveError -= h).Select(e => errorIcon),
                                    Observable.FromEventPattern<EventHandler<ErrorEventArgs>, ErrorEventArgs>(h => engine.Error += h, h => engine.Error -= h).Select(e => errorIcon))
                                    .StartWith(defaultIcon)
                                    .ToProperty(this, x => x.Icon);

            var fileOperations = Observable.FromEventPattern<EventHandler<FileEventArgs>, FileEventArgs>(h => engine.FileDetected += h, h => engine.FileDetected -= h)
                    .Select(e => new FileOperationViewModel(e.EventArgs.OldFilePath, engine))
                    .Distinct(fovm => fovm.OldFilePath)
                    .CreateCollection();

            fileOperations.ChangeTrackingEnabled = true;

            FileOperations = fileOperations.CreateDerivedCollection(x => x, x => !(_appConfig.AutoClear && x.State == FileOperationState.Completed));

            ShowWindow = new ReactiveCommand();
            ShowWindow.Subscribe(x => 
            {
                appContainer.Current.MainWindow.WindowState = WindowState.Normal;
                appContainer.Current.MainWindow.Activate();
            });
            
            Exit = new ReactiveCommand();
            Exit.Subscribe(x =>
            {
                appContainer.Current.Shutdown();
            });
        }
    }
}
