using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFileMover.Core;
using ReactiveUI;

namespace AutoFileMover.Desktop.ViewModels.Main
{
    public class EngineViewModel : ReactiveObject
    {
        public IObservable<FileEventArgs> FileDetected { get; private set; }

        public EngineViewModel(IEngine engine)
        {
            FileDetected = Observable.FromEventPattern<EventHandler<FileEventArgs>, FileEventArgs>(h => engine.FileDetected += h, h => engine.FileDetected -= h)
                                        .Select(e => e.EventArgs);

            FileDetected.Subscribe(e => this.RaisePropertyChanged("FileDetected"));
                
        }

    }
}
