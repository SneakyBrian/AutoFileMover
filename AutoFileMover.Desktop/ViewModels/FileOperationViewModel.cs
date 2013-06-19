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
        public string FilePath { get; private set; }
        public string OldFilePath { get; private set; }
        public FileOperationState State { get; private set; }
        public int Percentage { get; private set; }
        public Exception Error { get; private set; }

        public FileOperationViewModel(string fileName, string oldFileName, IEngine engine)
        {
            //these properties don't change
            FilePath = fileName;
            OldFilePath = oldFileName;

            //state property
            Observable.Merge(Observable.FromEventPattern<EventHandler<FileEventArgs>, FileEventArgs>(h => engine.FileMoveStarted += h, h => engine.FileMoveStarted -= h)
                                        .Where(e => e.EventArgs.FilePath == FilePath)
                                        .Select(e => FileOperationState.Moving),
                                Observable.FromEventPattern<EventHandler<FileEventArgs>, FileEventArgs>(h => engine.FileMoveCompleted += h, h => engine.FileMoveCompleted -= h)
                                        .Where(e => e.EventArgs.FilePath == FilePath)
                                        .Select(e => FileOperationState.Completed),
                                Observable.FromEventPattern<EventHandler<FileErrorEventArgs>, FileErrorEventArgs>(h => engine.FileMoveError += h, h => engine.FileMoveError -= h)
                                        .Where(e => e.EventArgs.FilePath == FilePath)
                                        .Select(e => FileOperationState.Error))
                                .StartWith(FileOperationState.Detected)
                                .ToProperty(this, vm => vm.State);

            //percentage
            Observable.FromEventPattern<EventHandler<FileMoveEventArgs>, FileMoveEventArgs>(h => engine.FileMoveProgress += h, h => engine.FileMoveProgress -= h)
                        .Where(e => e.EventArgs.FilePath == FilePath)
                        .Select(e => e.EventArgs.Percentage)
                        .ToProperty(this, vm => vm.Percentage);

            //error
            Observable.FromEventPattern<EventHandler<FileErrorEventArgs>, FileErrorEventArgs>(h => engine.FileMoveError += h, h => engine.FileMoveError -= h)
                .Where(e => e.EventArgs.FilePath == FilePath)
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
