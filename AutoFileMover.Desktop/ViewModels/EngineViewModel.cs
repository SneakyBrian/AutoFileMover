using System;
using System.IO;
using System.Reactive.Linq;
using AutoFileMover.Core.Interfaces;
using AutoFileMover.Desktop.Interfaces;
using AutoFileMover.Desktop.IoC;
using Microsoft.Practices.Unity;
using ReactiveUI;
using ReactiveUI.Xaml;
using UnityConfiguration;

namespace AutoFileMover.Desktop.ViewModels
{
    public class EngineViewModel : ReactiveObject
    {
        private readonly IEngine _engine = null;
        private readonly IApplicationConfig _appConfig = null;

        private ObservableAsPropertyHelper<EngineState> _state;
        public EngineState State 
        {
            get { return _state.Value; } 
        }

        private bool _AutoStart;
        public bool AutoStart
        {
            get { return _AutoStart; }
            set { this.RaiseAndSetIfChanged(x => x.AutoStart, value); }
        }
        
        public ReactiveCollection<Exception> Errors { get; private set; }
        public ReactiveCollection<FileOperationViewModel> FileOperations { get; private set; }

        public ConfigViewModel Config { get; private set; }

        public TrayIconViewModel TrayIcon { get; private set; }

        public AboutViewModel About { get; private set; }

        public ReactiveCommand Start { get; private set; }
        public ReactiveCommand Stop { get; private set; }
        public ReactiveCommand Scan { get; private set; }

        public ReactiveCommand ClearErrors { get; private set; }
        public ReactiveCommand ClearFileOperations { get; private set; }

        public EngineViewModel()
        {
            using (var container = new UnityContainer())
            {
                container.Configure(x =>
                {
                    x.AddRegistry<IoCRegistry>();
                });

                _engine = container.Resolve<IEngine>();
                _appConfig = container.Resolve<IApplicationConfig>();
                _engine.Config = _appConfig;                
            }

            Initialise(_engine);
        }

        public EngineViewModel(IEngine engine, IApplicationConfig config)
        {
            _engine = engine;
            _engine.Config = _appConfig = config;

            Initialise(_engine);
        }

        private void Initialise(IEngine engine)
        {
            var stateObservable = Observable.Merge(Observable.FromEventPattern(h => engine.Starting += h, h => engine.Starting -= h).Select(e => EngineState.Starting),
                                                    Observable.FromEventPattern(h => engine.Started += h, h => engine.Started -= h).Select(e => EngineState.Started),
                                                    Observable.FromEventPattern(h => engine.Stopping += h, h => engine.Stopping -= h).Select(e => EngineState.Stopping),
                                                    Observable.FromEventPattern(h => engine.Stopped += h, h => engine.Stopped -= h).Select(e => EngineState.Stopped))
                                                    .StartWith(EngineState.Stopped);

            _state = stateObservable.ToProperty(this, vm => vm.State);

                                        
            Start = new ReactiveCommand(stateObservable.Select(e => e == EngineState.Stopped));
            Start.Subscribe(x => engine.Start());

            Stop = new ReactiveCommand(stateObservable.Select(e => e == EngineState.Started));
            Stop.Subscribe(x => engine.Stop());

            Scan = new ReactiveCommand(stateObservable.Select(e => e == EngineState.Started));
            Scan.Subscribe(x => engine.Scan());


            var errors = Observable.FromEventPattern<EventHandler<ErrorEventArgs>, ErrorEventArgs>(h => engine.Error += h, h => engine.Error -= h)
                                .Select(e => e.EventArgs.GetException())
                                .CreateCollection();

            errors.ChangeTrackingEnabled = true;

            Errors = errors.CreateDerivedCollection(x => x, x => !_appConfig.AutoClear);

            var fileOperations = Observable.FromEventPattern<EventHandler<FileEventArgs>, FileEventArgs>(h => engine.FileDetected += h, h => engine.FileDetected -= h)
                                .Select(e => new FileOperationViewModel(e.EventArgs.OldFilePath, engine))
                                .Distinct(fovm => fovm.OldFilePath)
                                .CreateCollection();

            fileOperations.ChangeTrackingEnabled = true;

            FileOperations = fileOperations.CreateDerivedCollection(x => x, x => !(_appConfig.AutoClear && x.State == FileOperationState.Completed));

            ClearErrors = new ReactiveCommand(Errors.IsEmpty.Select(c => !c).StartWith(false));
            ClearErrors.Subscribe(x => Errors.Reset());

            ClearFileOperations = new ReactiveCommand(FileOperations.IsEmpty.Select(c => !c).StartWith(false));
            ClearFileOperations.Subscribe(x => FileOperations.Reset());


            Config = new ConfigViewModel(_appConfig);

            if (Config.AutoStart)
            {
                engine.Start();
            }

            TrayIcon = new TrayIconViewModel(engine);

            //About = new AboutViewModel();
        }
    }

    public enum EngineState
    {
        Starting,
        Started,
        Stopping,
        Stopped
    }
}
