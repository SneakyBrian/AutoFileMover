﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using AutoFileMover.Core.Interfaces;
using AutoFileMover.Desktop.Interfaces;
using AutoFileMover.Desktop.IoC;
using Microsoft.Practices.Unity;
using ReactiveUI;
using ReactiveUI.Xaml;
using UnityConfiguration;

namespace AutoFileMover.Desktop.ViewModels
{
    public class ConfigViewModel : ReactiveObject
    {
        private IApplicationConfig _config;

        private bool _AutoStart;
        public bool AutoStart
        {
            get { return _AutoStart; }
            set { this.RaiseAndSetIfChanged(x => x.AutoStart, value); }
        }

        private bool _AutoClear;
        public bool AutoClear
        {
            get { return _AutoClear; }
            set { this.RaiseAndSetIfChanged(x => x.AutoClear, value); }
        }

        private ReactiveCollection<string> _SourcePaths;
        public ReactiveCollection<string> SourcePaths
        {
            get { return _SourcePaths; }
            set { this.RaiseAndSetIfChanged(x => x.SourcePaths, value); }
        }

        private ReactiveCollection<string> _SourceRegex;
        public ReactiveCollection<string> SourceRegex
        {
            get { return _SourceRegex; }
            set { this.RaiseAndSetIfChanged(x => x.SourceRegex, value); }
        }

        private string _DestinationPath;
        public string DestinationPath
        {
            get { return _DestinationPath; }
            set { this.RaiseAndSetIfChanged(x => x.DestinationPath, value); }
        }

        private bool _IncludeSubdirectories;
        public bool IncludeSubdirectories
        {
            get { return _IncludeSubdirectories; }
            set { this.RaiseAndSetIfChanged(x => x.IncludeSubdirectories, value); }
        }

        private int _FileMoveRetries;
        public int FileMoveRetries
        {
            get { return _FileMoveRetries; }
            set { this.RaiseAndSetIfChanged(x => x.FileMoveRetries, value); }
        }

        private int _ConcurrentOperations;
        public int ConcurrentOperations
        {
            get { return _ConcurrentOperations; }
            set { this.RaiseAndSetIfChanged(x => x.ConcurrentOperations, value); }
        }

        private string _NewSourcePath;
        public string NewSourcePath
        {
            get { return _NewSourcePath; }
            set { this.RaiseAndSetIfChanged(x => x.NewSourcePath, value); }
        }

        private string _NewSourceRegex;
        public string NewSourceRegex
        {
            get { return _NewSourceRegex; }
            set { this.RaiseAndSetIfChanged(x => x.NewSourceRegex, value); }
        }

        private string _SelectedSourcePath;
        public string SelectedSourcePath
        {
            get { return _SelectedSourcePath; }
            set { this.RaiseAndSetIfChanged(x => x.SelectedSourcePath, value); }
        }

        private string _SelectedSourceRegex;
        public string SelectedSourceRegex
        {
            get { return _SelectedSourceRegex; }
            set { this.RaiseAndSetIfChanged(x => x.SelectedSourceRegex, value); }
        }
        
        public ReactiveCommand Save { get; set; }
        public ReactiveCommand AddSourcePath { get; set; }
        public ReactiveCommand RemoveSourcePath { get; set; }
        public ReactiveCommand AddSourceRegex { get; set; }
        public ReactiveCommand RemoveSourceRegex { get; set; }

        public ConfigViewModel(IApplicationConfig config)
        {
            _config = config;
            
            Initialise(_config);
        }

        private void Initialise(IApplicationConfig config)
        {
            this.AutoStart = config.AutoStart;
            this.AutoClear = config.AutoClear;

            this.DestinationPath = config.DestinationPath;
            this.FileMoveRetries = config.FileMoveRetries;
            this.ConcurrentOperations = config.ConcurrentOperations;
            this.IncludeSubdirectories = config.IncludeSubdirectories;
            this.SourcePaths = new ReactiveCollection<string>(config.SourcePaths);
            this.SourceRegex = new ReactiveCollection<string>(config.SourceRegex);

            Save = new ReactiveCommand();
            Save.Subscribe(e =>
            {
                config.AutoStart = this.AutoStart;
                config.AutoClear = this.AutoClear;

                config.DestinationPath = this.DestinationPath;
                config.FileMoveRetries = this.FileMoveRetries;
                config.ConcurrentOperations = this.ConcurrentOperations;
                config.IncludeSubdirectories = this.IncludeSubdirectories;
                config.SourcePaths = this.SourcePaths;
                config.SourceRegex = this.SourceRegex;
            });

            AddSourcePath = new ReactiveCommand(this.ObservableForProperty(x => x.NewSourcePath)
                                                    .Delay(TimeSpan.FromMilliseconds(500))
                                                    .Select(e => e.GetValue())
                                                    .Where(v => !string.IsNullOrWhiteSpace(v))
                                                    .Select(v => Directory.Exists(v))
                                                    .StartWith(false));
            AddSourcePath.Subscribe(e => 
            {
                this.SourcePaths.Add(this.NewSourcePath);
                this.NewSourcePath = string.Empty;
            });

            RemoveSourcePath = new ReactiveCommand(this.ObservableForProperty(x => x.SelectedSourcePath).Select(e => !string.IsNullOrWhiteSpace(e.GetValue())));
            RemoveSourcePath.Subscribe(e => 
            {
                this.SourcePaths.Remove(this.SelectedSourcePath);
                this.SelectedSourcePath = null;
            });

            AddSourceRegex = new ReactiveCommand(this.ObservableForProperty(x => x.NewSourceRegex)
                                                    .Delay(TimeSpan.FromMilliseconds(500))
                                                    .Select(e => e.GetValue())
                                                    .Select(v => IsValidRegex(v))
                                                    .StartWith(false));
            AddSourceRegex.Subscribe(e =>
            {
                this.SourceRegex.Add(this.NewSourceRegex);
                this.NewSourceRegex = string.Empty;
            });

            RemoveSourceRegex = new ReactiveCommand(this.ObservableForProperty(x => x.SelectedSourceRegex).Select(e => !string.IsNullOrWhiteSpace(e.GetValue())));
            RemoveSourceRegex.Subscribe(e =>
            {
                this.SourceRegex.Remove(this.SelectedSourceRegex);
                this.SelectedSourceRegex = null;
            });

            this.NewSourcePath = "";
            this.SelectedSourcePath = "";
            this.NewSourceRegex = "";
            this.SelectedSourceRegex = "";
        }

        private static bool IsValidRegex(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern)) return false;

            try
            {
                Regex.Match("", pattern);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }
    }
}
