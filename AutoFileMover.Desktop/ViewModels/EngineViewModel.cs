using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFileMover.Core;
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

        private ObservableAsPropertyHelper<EngineState> _state;
        public EngineState State 
        {
            get { return _state.Value; } 
        }
        
        public ReactiveCollection<Exception> Errors { get; set; }
        public ReactiveCollection<FileOperationViewModel> FileOperations { get; set; }

        public ReactiveCommand Start { get; set; }
        public ReactiveCommand Stop { get; set; }
        public ReactiveCommand Scan { get; set; }

        public EngineViewModel()
        {
            using (var container = new UnityContainer())
            {
                container.Configure(x =>
                {
                    x.AddRegistry<IoCRegistry>();
                });

                _engine = container.Resolve<IEngine>();
                //_engine.Config = container.Resolve<IConfig>();
                
                //temp code to inject simple config
                _engine.Config = new Config
                {
                    DestinationPath = @"D:\Temp\AFMTEST\Output",
                    FileMoveRetries = 3,
                    IncludeSubdirectories = false,
                    SourcePaths = new[] { @"D:\Temp\AFMTEST\Input" },
                    SourceRegex = new[] { @"^(\b.*\b(?=[.])).*(?<Season>(?:(?<=s)[1-9][0-9]|(?<=s0)[1-9])).*\.(?:avi|mkv|mp4)$" }
                };
            }

            Initialise(_engine);
        }

        public EngineViewModel(IEngine engine, IConfig config)
        {
            _engine = engine;
            _engine.Config = config;

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

            Errors = Observable.FromEventPattern<EventHandler<ErrorEventArgs>, ErrorEventArgs>(h => engine.Error += h, h => engine.Error -= h)
                                 .Select(e => e.EventArgs.GetException())
                                 .CreateCollection();


            FileOperations = Observable.FromEventPattern<EventHandler<FileEventArgs>, FileEventArgs>(h => engine.FileDetected += h, h => engine.FileDetected -= h)
                                .Select(e => new FileOperationViewModel(e.EventArgs.OldFilePath, engine))
                                .CreateCollection();
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
