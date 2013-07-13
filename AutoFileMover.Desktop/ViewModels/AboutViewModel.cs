using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reflection;
using AutoFileMover.Desktop.Interfaces;
using AutoFileMover.Desktop.IoC;
using Microsoft.Practices.Unity;
using ReactiveUI;
using ReactiveUI.Xaml;
using UnityConfiguration;

namespace AutoFileMover.Desktop.ViewModels
{
    public class AboutViewModel : ReactiveObject
    {
        private IApplicationDeployment _deployment;

        public AboutViewModel()
        {
            using (var container = new UnityContainer())
            {
                container.Configure(x =>
                {
                    x.AddRegistry<IoCRegistry>();
                });

                _deployment = container.Resolve<IApplicationDeployment>();
            }

            Initialise(_deployment);
        }

        public AboutViewModel(IApplicationDeployment deployment)
        {
            _deployment = deployment;

            Initialise(_deployment);
        }

        private void Initialise(IApplicationDeployment deployment)
        {
            CheckForUpdate = new ReactiveCommand(this.ObservableForProperty(vm => vm.NetworkDeployed).Select(e => e.Value));
            CheckForUpdate.Subscribe(x => deployment.CheckForUpdateAsync());

            var checkForUpdateObservable = Observable.FromEventPattern<CheckForUpdateCompletedEventHandler, CheckForUpdateCompletedEventArgs>
                    (h => deployment.CheckForUpdateCompleted += h,
                     h => deployment.CheckForUpdateCompleted += h);

            checkForUpdateObservable.Select(e => e.EventArgs.AvailableVersion).ToProperty(this, vm => vm.AvailableVersion);
            checkForUpdateObservable.Select(e => e.EventArgs.UpdateAvailable).ToProperty(this, vm => vm.UpdateAvailable);
            checkForUpdateObservable.Select(e => e.EventArgs.UpdateSizeBytes).ToProperty(this, vm => vm.UpdateSizeBytes);
            checkForUpdateObservable.Select(e => e.EventArgs.MinimumRequiredVersion).ToProperty(this, vm => vm.MinimumRequiredVersion);
            checkForUpdateObservable.Select(e => e.EventArgs.IsUpdateRequired).ToProperty(this, vm => vm.IsUpdateRequired);

            Update = new ReactiveCommand(checkForUpdateObservable.Select(e => e.EventArgs.UpdateAvailable));
            Update.Subscribe(x => deployment.UpdateAsync());

            var updateProgressObservable = Observable.Merge(Observable.FromEventPattern<DeploymentProgressChangedEventHandler, DeploymentProgressChangedEventArgs>
                                                            (h => deployment.CheckForUpdateProgressChanged += h,
                                                             h => deployment.CheckForUpdateProgressChanged += h),
                                                             Observable.FromEventPattern<DeploymentProgressChangedEventHandler, DeploymentProgressChangedEventArgs>
                                                            (h => deployment.UpdateProgressChanged += h,
                                                             h => deployment.UpdateProgressChanged += h),
                                                             Observable.FromEventPattern<DeploymentProgressChangedEventHandler, DeploymentProgressChangedEventArgs>
                                                            (h => deployment.DownloadFileGroupProgressChanged += h,
                                                             h => deployment.DownloadFileGroupProgressChanged += h));

            updateProgressObservable.Select(e => e.EventArgs.State).ToProperty(this, vm => vm.DeploymentProgressState);
            updateProgressObservable.Select(e => e.EventArgs.ProgressPercentage).ToProperty(this, vm => vm.ProgressPercentage);
            updateProgressObservable.Select(e => e.EventArgs.BytesCompleted).ToProperty(this, vm => vm.BytesCompleted);
            updateProgressObservable.Select(e => e.EventArgs.BytesTotal).ToProperty(this, vm => vm.BytesTotal);
            updateProgressObservable.Select(e => e.EventArgs.Group).ToProperty(this, vm => vm.Group);

            var updateCompleteObservable = Observable.FromEventPattern<AsyncCompletedEventHandler, AsyncCompletedEventArgs>
                    (h => deployment.UpdateCompleted += h,
                     h => deployment.UpdateCompleted += h);

            //make sure the current version is kept up-to-date
            updateCompleteObservable.Select(e => deployment.CurrentVersion)
                .StartWith(deployment.CurrentVersion)
                .ToProperty(this, vm => vm.CurrentVersion);

            //build the in progress flag from all of ouyr observables
            Observable.Merge(checkForUpdateObservable.Select(e => false),
                                updateProgressObservable.Select(e => true),
                                updateCompleteObservable.Select(e => false),
                                Update.Select(e => true))
                            .StartWith(true)
                            .ToProperty(this, vm => vm.InProgress);

        }

        public bool NetworkDeployed { get { return _deployment.IsNetworkDeployed; } }

        private ObservableAsPropertyHelper<Version> _CurrentVersion;
        public Version CurrentVersion
        {
            get { return _CurrentVersion.Value; }
        }

        private ObservableAsPropertyHelper<Version> _AvailableVersion;
        public Version AvailableVersion
        {
            get { return _AvailableVersion.Value; }
        }

        private ObservableAsPropertyHelper<Version> _MinimumRequiredVersion;
        public Version MinimumRequiredVersion
        {
            get { return _MinimumRequiredVersion.Value; }
        }

        private ObservableAsPropertyHelper<bool> _UpdateAvailable;
        public bool UpdateAvailable
        {
            get { return _UpdateAvailable.Value; }
        }

        private ObservableAsPropertyHelper<bool> _IsUpdateRequired;
        public bool IsUpdateRequired
        {
            get { return _IsUpdateRequired.Value; }
        }

        private ObservableAsPropertyHelper<long> _UpdateSizeBytes;
        public long UpdateSizeBytes
        {
            get { return _UpdateSizeBytes.Value; }
        }

        private ObservableAsPropertyHelper<bool> _InProgress;
        public bool InProgress
        {
            get { return _InProgress.Value; }
        }

        private ObservableAsPropertyHelper<System.Deployment.Application.DeploymentProgressState> _DeploymentProgressState;
        public System.Deployment.Application.DeploymentProgressState DeploymentProgressState
        {
            get { return _DeploymentProgressState.Value; }
        }

        private ObservableAsPropertyHelper<int> _ProgressPercentage;
        public int ProgressPercentage
        {
            get { return _ProgressPercentage.Value; }
        }

        private ObservableAsPropertyHelper<long> _BytesCompleted;
        public long BytesCompleted
        {
            get { return _BytesCompleted.Value; }
        }

        private ObservableAsPropertyHelper<long> _BytesTotal;
        public long BytesTotal
        {
            get { return _BytesTotal.Value; }
        }

        private ObservableAsPropertyHelper<string> _Group;
        public string Group
        {
            get { return _Group.Value; }
        }

        public ReactiveCommand CheckForUpdate { get; private set; }

        public ReactiveCommand Update { get; private set; }

    }
}
