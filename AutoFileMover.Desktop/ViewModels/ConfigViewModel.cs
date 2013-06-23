using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using AutoFileMover.Core.Interfaces;
using AutoFileMover.Desktop.IoC;
using Microsoft.Practices.Unity;
using ReactiveUI;
using ReactiveUI.Xaml;
using UnityConfiguration;

namespace AutoFileMover.Desktop.ViewModels
{
    public class ConfigViewModel : ReactiveObject
    {
        private IConfig _config;

        private bool _AutoStart;
        public bool AutoStart
        {
            get { return _AutoStart; }
            set { this.RaiseAndSetIfChanged(x => x.AutoStart, value); }
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

        public ConfigViewModel()
        {
            using (var container = new UnityContainer())
            {
                container.Configure(x =>
                {
                    x.AddRegistry<IoCRegistry>();
                });

                _config = container.Resolve<IConfig>();
            }

            Initialise(_config);
        }

        public ConfigViewModel(IConfig config)
        {
            _config = config;
            
            Initialise(_config);
        }

        private void Initialise(IConfig config)
        {
            this.AutoStart = Properties.Settings.Default.AutoStart;

            this.DestinationPath = config.DestinationPath;
            this.FileMoveRetries = config.FileMoveRetries;
            this.IncludeSubdirectories = config.IncludeSubdirectories;
            this.SourcePaths = new ReactiveCollection<string>(config.SourcePaths);
            this.SourceRegex = new ReactiveCollection<string>(config.SourceRegex);

            Save = new ReactiveCommand();
            Save.Subscribe(e =>
            {
                Properties.Settings.Default.AutoStart = this.AutoStart;
                Properties.Settings.Default.Save();

                config.DestinationPath = this.DestinationPath;
                config.FileMoveRetries = this.FileMoveRetries;
                config.IncludeSubdirectories = this.IncludeSubdirectories;
                config.SourcePaths = this.SourcePaths;
                config.SourceRegex = this.SourceRegex;
            });

            AddSourcePath = new ReactiveCommand(this.ObservableForProperty(x => x.NewSourcePath).Select(e => !string.IsNullOrWhiteSpace(e.GetValue())));
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

            AddSourceRegex = new ReactiveCommand(this.ObservableForProperty(x => x.NewSourceRegex).Select(e => !string.IsNullOrWhiteSpace(e.GetValue())));
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
        }
    }
}
