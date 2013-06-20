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
    public class FileOperationViewModel : ReactiveObject
    {
        public string OldFilePath { get; private set; }

        private ObservableAsPropertyHelper<string> _filePath;
        public string FilePath
        {
            get { return _filePath.Value; }
        }

        private ObservableAsPropertyHelper<FileOperationState> _state;
        public FileOperationState State
        {
            get { return _state.Value; }
        }

        private ObservableAsPropertyHelper<int> _percentage;
        public int Percentage
        {
            get { return _percentage.Value; }
        }

        private ObservableAsPropertyHelper<Exception> _error;
        public Exception Error
        {
            get { return _error.Value; }
        }

        public FileOperationViewModel(string filePath, IEngine engine)
        {
            //this property doesn't change
            OldFilePath = filePath;

            //filepath property
            _filePath = Observable.FromEventPattern<EventHandler<FileEventArgs>, FileEventArgs>(h => engine.FileMoveStarted += h, h => engine.FileMoveStarted -= h)
                            .Where(e => e.EventArgs.OldFilePath == OldFilePath)            
                            .Select(e => e.EventArgs.FilePath)
                            .ToProperty(this, vm => vm.FilePath);

            //state property
            _state =  Observable.Merge(Observable.FromEventPattern<EventHandler<FileEventArgs>, FileEventArgs>(h => engine.FileMoveStarted += h, h => engine.FileMoveStarted -= h)
                                                .Where(e => e.EventArgs.OldFilePath == OldFilePath)
                                                .Select(e => FileOperationState.Moving),
                                        Observable.FromEventPattern<EventHandler<FileEventArgs>, FileEventArgs>(h => engine.FileMoveCompleted += h, h => engine.FileMoveCompleted -= h)
                                                .Where(e => e.EventArgs.OldFilePath == OldFilePath)
                                                .Select(e => FileOperationState.Completed),
                                        Observable.FromEventPattern<EventHandler<FileErrorEventArgs>, FileErrorEventArgs>(h => engine.FileMoveError += h, h => engine.FileMoveError -= h)
                                                .Where(e => e.EventArgs.OldFilePath == OldFilePath)
                                                .Select(e => FileOperationState.Error))
                                        .StartWith(FileOperationState.Detected)
                                        .ToProperty(this, vm => vm.State);

            //percentage
            _percentage = Observable.FromEventPattern<EventHandler<FileMoveEventArgs>, FileMoveEventArgs>(h => engine.FileMoveProgress += h, h => engine.FileMoveProgress -= h)
                            .Where(e => e.EventArgs.OldFilePath == OldFilePath)
                            .Select(e => e.EventArgs.Percentage)
                            .ToProperty(this, vm => vm.Percentage);

            //error
            _error = Observable.FromEventPattern<EventHandler<FileErrorEventArgs>, FileErrorEventArgs>(h => engine.FileMoveError += h, h => engine.FileMoveError -= h)
                            .Where(e => e.EventArgs.OldFilePath == OldFilePath)
                            .Select(e => e.EventArgs.Exception)
                            .ToProperty(this, vm => vm.Error);
        }
    }

    public enum FileOperationState
    {
        Detected,
        Moving,
        Completed,
        Error
    }
}
