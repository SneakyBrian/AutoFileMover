using System;
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
        
        private bool _verifyFiles;
        public bool VerifyFiles
        {
            get { return _verifyFiles; }
            set { this.RaiseAndSetIfChanged(ref _verifyFiles, value); }
        }

        private bool _autoStart;
        public bool AutoStart
        {
            get { return _autoStart; }
            set { this.RaiseAndSetIfChanged(ref _autoStart, value); }
        }

        private bool _autoClear;
        public bool AutoClear
        {
            get { return _autoClear; }
            set { this.RaiseAndSetIfChanged(ref _autoClear, value); }
        }

        private ReactiveList<string> _sourcePaths;
        public ReactiveList<string> SourcePaths
        {
            get { return _sourcePaths; }
            set { this.RaiseAndSetIfChanged(ref _sourcePaths, value); }
        }

        private ReactiveList<string> _sourceRegex;
        public ReactiveList<string> SourceRegex
        {
            get { return _sourceRegex; }
            set { this.RaiseAndSetIfChanged(ref _sourceRegex, value); }
        }

        private string _destinationPath;
        public string DestinationPath
        {
            get { return _destinationPath; }
            set { this.RaiseAndSetIfChanged(ref _destinationPath, value); }
        }

        private bool _includeSubdirectories;
        public bool IncludeSubdirectories
        {
            get { return _includeSubdirectories; }
            set { this.RaiseAndSetIfChanged(ref _includeSubdirectories, value); }
        }

        private int _fileMoveRetries;
        public int FileMoveRetries
        {
            get { return _fileMoveRetries; }
            set { this.RaiseAndSetIfChanged(ref _fileMoveRetries, value); }
        }

        private int _concurrentOperations;
        public int ConcurrentOperations
        {
            get { return _concurrentOperations; }
            set { this.RaiseAndSetIfChanged(ref _concurrentOperations, value); }
        }

        private string _newSourcePath;
        public string NewSourcePath
        {
            get { return _newSourcePath; }
            set { this.RaiseAndSetIfChanged(ref _newSourcePath, value); }
        }

        private string _newSourceRegex;
        public string NewSourceRegex
        {
            get { return _newSourceRegex; }
            set { this.RaiseAndSetIfChanged(ref _newSourceRegex, value); }
        }

        private string _selectedSourcePath;
        public string SelectedSourcePath
        {
            get { return _selectedSourcePath; }
            set { this.RaiseAndSetIfChanged(ref _selectedSourcePath, value); }
        }

        private string _selectedSourceRegex;
        public string SelectedSourceRegex
        {
            get { return _selectedSourceRegex; }
            set { this.RaiseAndSetIfChanged(ref _selectedSourceRegex, value); }
        }

        private bool _settingsChanged;
        public bool SettingsChanged
        {
            get { return _settingsChanged; }
            set { this.RaiseAndSetIfChanged(ref _settingsChanged, value); }
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
            this.VerifyFiles = config.VerifyFiles;

            this.DestinationPath = config.DestinationPath;
            this.FileMoveRetries = config.FileMoveRetries;
            this.ConcurrentOperations = config.ConcurrentOperations;
            this.IncludeSubdirectories = config.IncludeSubdirectories;
            this.SourcePaths = new ReactiveList<string>(config.SourcePaths);
            this.SourceRegex = new ReactiveList<string>(config.SourceRegex);

            this.SettingsChanged = false;

            Save = new ReactiveCommand(this.ObservableForProperty(x => x.SettingsChanged).Select(e => e.GetValue()));
            Save.Subscribe(e =>
            {
                config.AutoStart = this.AutoStart;
                config.AutoClear = this.AutoClear;
                config.VerifyFiles = this.VerifyFiles;

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

            Observable.Merge(this.ObservableForProperty(x => x.VerifyFiles).Select(e => true),
                             this.ObservableForProperty(x => x.AutoStart).Select(e => true),
                             this.ObservableForProperty(x => x.AutoClear).Select(e => true),
                             this.ObservableForProperty(x => x.SourcePaths).Select(e => true),
                             this.ObservableForProperty(x => x.SourceRegex).Select(e => true),
                             this.ObservableForProperty(x => x.DestinationPath).Select(e => true),
                             this.ObservableForProperty(x => x.IncludeSubdirectories).Select(e => true),
                             this.ObservableForProperty(x => x.FileMoveRetries).Select(e => true),
                             this.ObservableForProperty(x => x.ConcurrentOperations).Select(e => true),
                             this.ObservableForProperty(x => x.NewSourcePath).Select(e => true),
                             this.ObservableForProperty(x => x.NewSourceRegex).Select(e => true),
                             this.ObservableForProperty(x => x.SelectedSourcePath).Select(e => true),
                             this.ObservableForProperty(x => x.SelectedSourceRegex).Select(e => true))
                .Subscribe(e => this.SettingsChanged = true);
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
