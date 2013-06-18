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

        public IObservable<FileEventArgs> FileDetected { get; private set; }
        public IObservable<FileEventArgs> FileMoveStarted { get; private set; }
        public IObservable<FileEventArgs> FileMoveCompleted { get; private set; }
        public IObservable<FileMoveEventArgs> FileMoveProgress { get; private set; }
        public IObservable<FileErrorEventArgs> FileMoveError { get; private set; }

        public IObservable<EngineState> State { get; private set; }
        public IObservable<ErrorEventArgs> Error { get; private set; }

        public ReactiveCommand Start { get; protected set; }
        public ReactiveCommand Stop { get; protected set; }

        public EngineViewModel()
        {
            using (var container = new UnityContainer())
            {
                container.Configure(x =>
                {
                    x.AddRegistry<IoCRegistry>();
                });

                _engine = container.Resolve<IEngine>();
            }

            Initialise(_engine);
        }

        public EngineViewModel(IEngine engine)
        {
            _engine = engine;

            Initialise(_engine);
        }

        private void Initialise(IEngine engine)
        {
            FileDetected = Observable.FromEventPattern<EventHandler<FileEventArgs>, FileEventArgs>(h => engine.FileDetected += h, h => engine.FileDetected -= h)
                                        .Select(e => e.EventArgs);
            FileDetected.Subscribe(e => this.RaisePropertyChanged("FileDetected"));


            FileMoveStarted = Observable.FromEventPattern<EventHandler<FileEventArgs>, FileEventArgs>(h => engine.FileMoveStarted += h, h => engine.FileMoveStarted -= h)
                                        .Select(e => e.EventArgs);
            FileMoveStarted.Subscribe(e => this.RaisePropertyChanged("FileMoveStarted"));


            FileMoveCompleted = Observable.FromEventPattern<EventHandler<FileEventArgs>, FileEventArgs>(h => engine.FileMoveCompleted += h, h => engine.FileMoveCompleted -= h)
                                        .Select(e => e.EventArgs);
            FileMoveCompleted.Subscribe(e => this.RaisePropertyChanged("FileMoveCompleted"));


            FileMoveProgress = Observable.FromEventPattern<EventHandler<FileMoveEventArgs>, FileMoveEventArgs>(h => engine.FileMoveProgress += h, h => engine.FileMoveProgress -= h)
                                        .Select(e => e.EventArgs);
            FileMoveProgress.Subscribe(e => this.RaisePropertyChanged("FileMoveProgress"));


            FileMoveError = Observable.FromEventPattern<EventHandler<FileErrorEventArgs>, FileErrorEventArgs>(h => engine.FileMoveError += h, h => engine.FileMoveError -= h)
                                        .Select(e => e.EventArgs);
            FileMoveError.Subscribe(e => this.RaisePropertyChanged("FileMoveError"));


            State = Observable.Merge(Observable.FromEventPattern(h => engine.Starting += h, h => engine.Starting -= h).Select(e => EngineState.Starting),
                                        Observable.FromEventPattern(h => engine.Started += h, h => engine.Started -= h).Select(e => EngineState.Started),
                                        Observable.FromEventPattern(h => engine.Stopping += h, h => engine.Stopping -= h).Select(e => EngineState.Stopping),
                                        Observable.FromEventPattern(h => engine.Stopped += h, h => engine.Stopped -= h).Select(e => EngineState.Stopped))
                                        .StartWith(EngineState.Stopped);
            State.Subscribe(e => this.RaisePropertyChanged("State"));


            Error = Observable.FromEventPattern<EventHandler<ErrorEventArgs>, ErrorEventArgs>(h => engine.Error += h, h => engine.Error -= h)
                                        .Select(e => e.EventArgs);
            Error.Subscribe(e => this.RaisePropertyChanged("Error"));


            Start = new ReactiveCommand(State.Select(e => e == EngineState.Stopped));
            Start.Subscribe(x => engine.Start());
            
            Stop = new ReactiveCommand(State.Select(e => e == EngineState.Started));
            Stop.Subscribe(x => engine.Stop());
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
