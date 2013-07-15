﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AutoFileMover.Core.Interfaces;
using AutoFileMover.Desktop.IoC;
using Microsoft.Practices.Unity;
using ReactiveUI;
using ReactiveUI.Xaml;
using UnityConfiguration;

namespace AutoFileMover.Desktop.ViewModels
{
    public class TrayIconViewModel : ReactiveObject
    {
        private IEngine _engine;

        private ObservableAsPropertyHelper<string> _icon;
        public string Icon
        {
            get { return _icon.Value; }
        }

        public ReactiveCommand ShowWindow { get; private set; }
        public ReactiveCommand Exit { get; private set; }

        public TrayIconViewModel(IEngine engine)
        {
            _engine = engine;

            Initialise(_engine);
        }

        public void Initialise(IEngine engine)
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

            ShowWindow = new ReactiveCommand();
            ShowWindow.Subscribe(x => 
            {
                App.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
                App.Current.MainWindow.Activate();
            });
            
            Exit = new ReactiveCommand();
            Exit.Subscribe(x =>
            {
                App.Current.Shutdown();
            });
        }
    }
}
