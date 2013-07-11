using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Xaml;

namespace AutoFileMover.Desktop.ViewModels
{
    public class AboutViewModel : ReactiveObject
    {
        public Version CurrentVersion
        {
            get 
            {
                var assemblyName = Assembly.GetExecutingAssembly().GetName();

                var version = assemblyName.Version;

                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    version = ApplicationDeployment.CurrentDeployment.CurrentVersion;
                }

                return version;
            }
        }

        public bool NetworkDeployed { get { return ApplicationDeployment.IsNetworkDeployed; } }

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

        public ReactiveCommand Update { get; private set; }

        public AboutViewModel()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {

                var checkForUpdateObservable = Observable.FromEventPattern<CheckForUpdateCompletedEventHandler, CheckForUpdateCompletedEventArgs>
                        (h => ApplicationDeployment.CurrentDeployment.CheckForUpdateCompleted += h,
                         h => ApplicationDeployment.CurrentDeployment.CheckForUpdateCompleted += h);

                checkForUpdateObservable.Select(e => e.EventArgs.AvailableVersion).ToProperty(this, vm => vm.AvailableVersion);
                checkForUpdateObservable.Select(e => e.EventArgs.UpdateAvailable).ToProperty(this, vm => vm.UpdateAvailable);
                checkForUpdateObservable.Select(e => e.EventArgs.UpdateSizeBytes).ToProperty(this, vm => vm.UpdateSizeBytes);
                checkForUpdateObservable.Select(e => e.EventArgs.MinimumRequiredVersion).ToProperty(this, vm => vm.MinimumRequiredVersion);
                checkForUpdateObservable.Select(e => e.EventArgs.IsUpdateRequired).ToProperty(this, vm => vm.IsUpdateRequired);

                Update = new ReactiveCommand(checkForUpdateObservable.Select(e => e.EventArgs.UpdateAvailable));
                Update.Subscribe(x => ApplicationDeployment.CurrentDeployment.UpdateAsync());

                var updateProgressObservable = Observable.Merge(Observable.FromEventPattern<DeploymentProgressChangedEventHandler, DeploymentProgressChangedEventArgs>
                                                                (h => ApplicationDeployment.CurrentDeployment.CheckForUpdateProgressChanged += h,
                                                                 h => ApplicationDeployment.CurrentDeployment.CheckForUpdateProgressChanged += h),
                                                                 Observable.FromEventPattern<DeploymentProgressChangedEventHandler, DeploymentProgressChangedEventArgs>
                                                                (h => ApplicationDeployment.CurrentDeployment.UpdateProgressChanged += h,
                                                                 h => ApplicationDeployment.CurrentDeployment.UpdateProgressChanged += h),
                                                                 Observable.FromEventPattern<DeploymentProgressChangedEventHandler, DeploymentProgressChangedEventArgs>
                                                                (h => ApplicationDeployment.CurrentDeployment.DownloadFileGroupProgressChanged += h,
                                                                 h => ApplicationDeployment.CurrentDeployment.DownloadFileGroupProgressChanged += h));

                //attach these to properties
                //updateProgressObservable.Select(e => e.EventArgs.State);
                //updateProgressObservable.Select(e => e.EventArgs.ProgressPercentage);
                //updateProgressObservable.Select(e => e.EventArgs.BytesCompleted);
                //updateProgressObservable.Select(e => e.EventArgs.BytesTotal);
                //updateProgressObservable.Select(e => e.EventArgs.Group);

                var updateObservable = Observable.FromEventPattern<AsyncCompletedEventHandler, AsyncCompletedEventArgs>
                        (h => ApplicationDeployment.CurrentDeployment.UpdateCompleted += h,
                         h => ApplicationDeployment.CurrentDeployment.UpdateCompleted += h);

                Observable.Merge(checkForUpdateObservable.Select(e => false),
                                    updateProgressObservable.Select(e => true),
                                    updateObservable.Select(e => false),
                                    Update.Select(e => true))
                                .StartWith(true)
                                .ToProperty(this, vm => vm.InProgress);

                //start the check for an update
                ApplicationDeployment.CurrentDeployment.CheckForUpdateAsync();
            }
            else
            {
                Observable.Start(() => false).ToProperty(this, vm => vm.UpdateAvailable);
            }
        }

    }
}
