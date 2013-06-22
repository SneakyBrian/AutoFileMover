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

        private IEnumerable<string> _SourcePaths;
        public IEnumerable<string> SourcePaths
        {
            get { return _SourcePaths; }
            set { this.RaiseAndSetIfChanged(x => x.SourcePaths, value); }
        }

        private IEnumerable<string> _SourceRegex;
        public IEnumerable<string> SourceRegex
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

        private ObservableAsPropertyHelper<bool> _dirty;
        public bool Dirty
        {
            get { return _dirty.Value; }
        }

        public ReactiveCommand Save { get; set; }

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
        }

        public ConfigViewModel(IConfig config)
        {
            _config = config;
        }

        private void Initialise(IConfig config)
        {
            Save = new ReactiveCommand();
            Save.Subscribe(e =>
            {
                _config.DestinationPath = this.DestinationPath;
                _config.FileMoveRetries = this.FileMoveRetries;
                _config.IncludeSubdirectories = this.IncludeSubdirectories;
                _config.SourcePaths = this.SourcePaths;
                _config.SourceRegex = this.SourceRegex;
            });

            _dirty = Observable.Merge(this.ObservableForProperty(vm => vm.SourcePaths).Select(e => true),
                                        this.ObservableForProperty(vm => vm.SourceRegex).Select(e => true),
                                        this.ObservableForProperty(vm => vm.DestinationPath).Select(e => true),
                                        this.ObservableForProperty(vm => vm.IncludeSubdirectories).Select(e => true),
                                        this.ObservableForProperty(vm => vm.FileMoveRetries).Select(e => true),
                                        Save.Select(e => false))
                                        .DistinctUntilChanged()
                                        .ToProperty(this, vm => vm.Dirty);

        }
    }
}
