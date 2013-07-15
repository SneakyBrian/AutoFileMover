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
                     h => deployment.CheckForUpdateCompleted -= h);

            _availableVersion = checkForUpdateObservable.Select(e => e.EventArgs.AvailableVersion).ToProperty(this, vm => vm.AvailableVersion);
            _updateAvailable = checkForUpdateObservable.Select(e => e.EventArgs.UpdateAvailable).ToProperty(this, vm => vm.UpdateAvailable);
            _updateSizeBytes = checkForUpdateObservable.Select(e => e.EventArgs.UpdateSizeBytes).ToProperty(this, vm => vm.UpdateSizeBytes);
            _minimumRequiredVersion = checkForUpdateObservable.Select(e => e.EventArgs.MinimumRequiredVersion).ToProperty(this, vm => vm.MinimumRequiredVersion);
            _isUpdateRequired = checkForUpdateObservable.Select(e => e.EventArgs.IsUpdateRequired).ToProperty(this, vm => vm.IsUpdateRequired);

            Update = new ReactiveCommand(checkForUpdateObservable.Select(e => e.EventArgs.UpdateAvailable));
            Update.Subscribe(x => deployment.UpdateAsync());

            var updateProgressObservable = Observable.Merge(Observable.FromEventPattern<DeploymentProgressChangedEventHandler, DeploymentProgressChangedEventArgs>
                                                            (h => deployment.CheckForUpdateProgressChanged += h,
                                                             h => deployment.CheckForUpdateProgressChanged -= h),
                                                             Observable.FromEventPattern<DeploymentProgressChangedEventHandler, DeploymentProgressChangedEventArgs>
                                                            (h => deployment.UpdateProgressChanged += h,
                                                             h => deployment.UpdateProgressChanged -= h),
                                                             Observable.FromEventPattern<DeploymentProgressChangedEventHandler, DeploymentProgressChangedEventArgs>
                                                            (h => deployment.DownloadFileGroupProgressChanged += h,
                                                             h => deployment.DownloadFileGroupProgressChanged -= h));

            _deploymentProgressState = updateProgressObservable.Select(e => e.EventArgs.State).ToProperty(this, vm => vm.DeploymentProgressState);
            _progressPercentage = updateProgressObservable.Select(e => e.EventArgs.ProgressPercentage).ToProperty(this, vm => vm.ProgressPercentage);
            _bytesCompleted = updateProgressObservable.Select(e => e.EventArgs.BytesCompleted).ToProperty(this, vm => vm.BytesCompleted);
            _bytesTotal = updateProgressObservable.Select(e => e.EventArgs.BytesTotal).ToProperty(this, vm => vm.BytesTotal);
            _group = updateProgressObservable.Select(e => e.EventArgs.Group).ToProperty(this, vm => vm.Group);

            var updateCompleteObservable = Observable.FromEventPattern<AsyncCompletedEventHandler, AsyncCompletedEventArgs>
                    (h => deployment.UpdateCompleted += h,
                     h => deployment.UpdateCompleted -= h);

            //make sure the current version is kept up-to-date
            _currentVersion = updateCompleteObservable.Select(e => deployment.CurrentVersion)
                .ToProperty(this, vm => vm.CurrentVersion, deployment.CurrentVersion);

            //build the in progress flag from all of our observables
            _inProgress = Observable.Merge(checkForUpdateObservable.Select(e => false),
                                updateProgressObservable.Select(e => true),
                                updateCompleteObservable.Select(e => false),
                                Update.Select(e => true))
                            .ToProperty(this, vm => vm.InProgress, false);
        }

        public bool NetworkDeployed { get { return _deployment.IsNetworkDeployed; } }

        private ObservableAsPropertyHelper<Version> _currentVersion;
        public Version CurrentVersion
        {
            get { return _currentVersion.Value; }
        }

        private ObservableAsPropertyHelper<Version> _availableVersion;
        public Version AvailableVersion
        {
            get { return _availableVersion.Value; }
        }

        private ObservableAsPropertyHelper<Version> _minimumRequiredVersion;
        public Version MinimumRequiredVersion
        {
            get { return _minimumRequiredVersion.Value; }
        }

        private ObservableAsPropertyHelper<bool> _updateAvailable;
        public bool UpdateAvailable
        {
            get { return _updateAvailable.Value; }
        }

        private ObservableAsPropertyHelper<bool> _isUpdateRequired;
        public bool IsUpdateRequired
        {
            get { return _isUpdateRequired.Value; }
        }

        private ObservableAsPropertyHelper<long> _updateSizeBytes;
        public long UpdateSizeBytes
        {
            get { return _updateSizeBytes.Value; }
        }

        private ObservableAsPropertyHelper<bool> _inProgress;
        public bool InProgress
        {
            get { return _inProgress.Value; }
        }

        private ObservableAsPropertyHelper<System.Deployment.Application.DeploymentProgressState> _deploymentProgressState;
        public System.Deployment.Application.DeploymentProgressState DeploymentProgressState
        {
            get { return _deploymentProgressState.Value; }
        }

        private ObservableAsPropertyHelper<int> _progressPercentage;
        public int ProgressPercentage
        {
            get { return _progressPercentage.Value; }
        }

        private ObservableAsPropertyHelper<long> _bytesCompleted;
        public long BytesCompleted
        {
            get { return _bytesCompleted.Value; }
        }

        private ObservableAsPropertyHelper<long> _bytesTotal;
        public long BytesTotal
        {
            get { return _bytesTotal.Value; }
        }

        private ObservableAsPropertyHelper<string> _group;
        public string Group
        {
            get { return _group.Value; }
        }

        public ReactiveCommand CheckForUpdate { get; private set; }

        public ReactiveCommand Update { get; private set; }

    }
}
