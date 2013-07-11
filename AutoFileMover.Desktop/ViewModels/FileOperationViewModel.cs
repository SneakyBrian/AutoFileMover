using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFileMover.Core;
using AutoFileMover.Core.Interfaces;
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

        private ObservableAsPropertyHelper<bool> _inProgress;
        public bool InProgress
        {
            get { return _inProgress.Value; }
        }

        private ObservableAsPropertyHelper<Exception> _error;
        public Exception Error
        {
            get { return _error.Value; }
        }

        private ObservableAsPropertyHelper<string> _triesText;
        public string TriesText
        {
            get { return _triesText.Value; }
        }

        private ObservableAsPropertyHelper<string> _sourceFileHash;
        public string SourceFileHash
        {
            get { return _sourceFileHash.Value; }
        }

        private ObservableAsPropertyHelper<string> _destinationFileHash;
        public string DestinationFileHash
        {
            get { return _destinationFileHash.Value; }
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
            _state = Observable.Merge(Observable.FromEventPattern<EventHandler<FileHashEventArgs>, FileHashEventArgs>(h => engine.FileHashProgress += h, h => engine.FileHashProgress -= h)
                                                .Where(e => e.EventArgs.OldFilePath == OldFilePath)
                                                .Select(e => FileOperationState.Verifying), 
                                        Observable.FromEventPattern<EventHandler<FileEventArgs>, FileEventArgs>(h => engine.FileMoveStarted += h, h => engine.FileMoveStarted -= h)
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
            _percentage = Observable.Merge(Observable.FromEventPattern<EventHandler<FileMoveEventArgs>, FileMoveEventArgs>(h => engine.FileMoveProgress += h, h => engine.FileMoveProgress -= h)
                                                        .Where(e => e.EventArgs.OldFilePath == OldFilePath)
                                                        .Select(e => e.EventArgs.Percentage),
                                            Observable.FromEventPattern<EventHandler<FileHashEventArgs>, FileHashEventArgs>(h => engine.FileHashProgress += h, h => engine.FileHashProgress -= h)
                                            .Where(e => e.EventArgs.OldFilePath == OldFilePath)
                                                        .Select(e => e.EventArgs.Percentage))
                                        .ToProperty(this, vm => vm.Percentage);

            //in progress
            _inProgress = _state.AsObservable()
                            .Select(e => e != FileOperationState.Completed && e != FileOperationState.Error)
                            .ToProperty(this, vm => vm.InProgress);

            //error
            _error = Observable.FromEventPattern<EventHandler<FileErrorEventArgs>, FileErrorEventArgs>(h => engine.FileMoveError += h, h => engine.FileMoveError -= h)
                            .Where(e => e.EventArgs.OldFilePath == OldFilePath)
                            .Select(e => e.EventArgs.Exception)
                            .ToProperty(this, vm => vm.Error);

            //tries
            _triesText = Observable.Merge(Observable.FromEventPattern<EventHandler<FileHashEventArgs>, FileHashEventArgs>(h => engine.FileHashProgress += h, h => engine.FileHashProgress -= h)
                                                .Where(e => e.EventArgs.OldFilePath == OldFilePath)
                                                .Select(e => string.Format("Attempt {0} of {1}", e.EventArgs.Tries + 1, engine.Config.FileMoveRetries)),
                                            Observable.FromEventPattern<EventHandler<FileMoveEventArgs>, FileMoveEventArgs>(h => engine.FileMoveProgress += h, h => engine.FileMoveProgress -= h)
                                                .Where(e => e.EventArgs.OldFilePath == OldFilePath)
                                                .Select(e => string.Format("Attempt {0} of {1}", e.EventArgs.Tries + 1, engine.Config.FileMoveRetries)),
                                            Observable.FromEventPattern<EventHandler<FileErrorEventArgs>, FileErrorEventArgs>(h => engine.FileMoveError += h, h => engine.FileMoveError -= h)
                                                .Where(e => e.EventArgs.OldFilePath == OldFilePath)
                                                .Select(e => string.Format("Attempt {0} of {1}", e.EventArgs.Tries + 1, engine.Config.FileMoveRetries)))
                                        .StartWith(string.Format("Attempt 1 of {0}", engine.Config.FileMoveRetries))
                                        .ToProperty(this, vm => vm.TriesText);

            //source hash
            _sourceFileHash = Observable.FromEventPattern<EventHandler<FileHashEventArgs>, FileHashEventArgs>(h => engine.FileHashProgress += h, h => engine.FileHashProgress -= h)
                                                .Where(e => e.EventArgs.OldFilePath == OldFilePath && e.EventArgs.HashFilePath == OldFilePath)
                                                .Select(e => e.EventArgs.Hash)
                                                .StartWith(string.Empty)
                                                .ToProperty(this, vm => vm.SourceFileHash);

            //destination hash
            _destinationFileHash = Observable.FromEventPattern<EventHandler<FileHashEventArgs>, FileHashEventArgs>(h => engine.FileHashProgress += h, h => engine.FileHashProgress -= h)
                                                .Where(e => e.EventArgs.OldFilePath == OldFilePath && e.EventArgs.HashFilePath == e.EventArgs.FilePath)
                                                .Select(e => e.EventArgs.Hash)
                                                .StartWith(string.Empty)
                                                .ToProperty(this, vm => vm.SourceFileHash);

        }
    }

    public enum FileOperationState
    {
        Detected,
        Moving,
        Verifying,
        Completed,
        Error
    }
}
