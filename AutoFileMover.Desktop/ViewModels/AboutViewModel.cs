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
        private IApplicationContainer _appContainer;

        public AboutViewModel()
        {
            using (var container = new UnityContainer())
            {
                container.Configure(x =>
                {
                    x.AddRegistry<IoCRegistry>();
                });

                _deployment = container.Resolve<IApplicationDeployment>();
                _appContainer = container.Resolve<IApplicationContainer>();
            }

            Initialise(_deployment, _appContainer);
        }

        public AboutViewModel(IApplicationDeployment deployment, IApplicationContainer appContainer)
        {
            _deployment = deployment;
            _appContainer = appContainer;

            Initialise(_deployment, _appContainer);
        }

        private void Initialise(IApplicationDeployment deployment, IApplicationContainer appContainer)
        {
            CheckForUpdate = new ReactiveCommand(this.ObservableForProperty(vm => vm.NetworkDeployed).Select(e => e.Value), false, null);
            CheckForUpdate.Subscribe(x => deployment.CheckForUpdateAsync());
            CheckForUpdate.ThrownExceptions.Subscribe();

            var checkForUpdateObservable = Observable.FromEventPattern<CheckForUpdateCompletedEventHandler, CheckForUpdateCompletedEventArgs>
                                                    (h => deployment.CheckForUpdateCompleted += h,
                                                     h => deployment.CheckForUpdateCompleted -= h);

            var checkForUpdateProgressObservable = Observable.FromEventPattern<DeploymentProgressChangedEventHandler, DeploymentProgressChangedEventArgs>
                                                            (h => deployment.CheckForUpdateProgressChanged += h,
                                                             h => deployment.CheckForUpdateProgressChanged -= h);                                                             

            _availableVersion = checkForUpdateObservable.Select(e => e.EventArgs.AvailableVersion).ToProperty(this, vm => vm.AvailableVersion);
            _updateAvailable = checkForUpdateObservable.Select(e => e.EventArgs.UpdateAvailable).ToProperty(this, vm => vm.UpdateAvailable);
            _updateSizeBytes = checkForUpdateObservable.Select(e => e.EventArgs.UpdateSizeBytes).ToProperty(this, vm => vm.UpdateSizeBytes);
            _minimumRequiredVersion = checkForUpdateObservable.Select(e => e.EventArgs.MinimumRequiredVersion).ToProperty(this, vm => vm.MinimumRequiredVersion);
            _isUpdateRequired = checkForUpdateObservable.Select(e => e.EventArgs.IsUpdateRequired).ToProperty(this, vm => vm.IsUpdateRequired);

            Update = new ReactiveCommand(checkForUpdateObservable.Select(e => e.EventArgs.UpdateAvailable), false, null);
            Update.Subscribe(x => deployment.UpdateAsync());
            Update.ThrownExceptions.Subscribe();

            var updateProgressObservable = Observable.Merge(Observable.FromEventPattern<DeploymentProgressChangedEventHandler, DeploymentProgressChangedEventArgs>
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

            _updateCompleted = updateCompleteObservable.Select(e => true)
                                .ToProperty(this, vm => vm.UpdateCompleted, false);

            //make sure the current version is kept up-to-date
            _currentVersion = updateCompleteObservable.Select(e => deployment.CurrentVersion)
                .ToProperty(this, vm => vm.CurrentVersion, deployment.CurrentVersion);

            //build the in progress flags from all of our observables

            _checkInProgress = Observable.Merge(checkForUpdateObservable.Select(e => false),
                                checkForUpdateProgressObservable.Select(e => true),
                                CheckForUpdate.Select(e => true),
                                Update.Select(e => false))
                            .ToProperty(this, vm => vm.CheckInProgress, false);

            _updateInProgress = Observable.Merge(updateProgressObservable.Select(e => true),
                                updateCompleteObservable.Select(e => false),
                                Update.Select(e => true))
                            .ToProperty(this, vm => vm.UpdateInProgress, false);

            Restart = new ReactiveCommand(updateCompleteObservable.Select(e => true), false, null);
            Restart.Subscribe(x => appContainer.Restart());
            Restart.ThrownExceptions.Subscribe();
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

        private ObservableAsPropertyHelper<bool> _checkInProgress;
        public bool CheckInProgress
        {
            get { return _checkInProgress.Value; }
        }        
        
        private ObservableAsPropertyHelper<bool> _updateInProgress;
        public bool UpdateInProgress
        {
            get { return _updateInProgress.Value; }
        }

        private ObservableAsPropertyHelper<bool> _updateCompleted;
        public bool UpdateCompleted
        {
            get { return _updateCompleted.Value; }
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

        public ReactiveCommand Restart { get; private set; }

    }
}
