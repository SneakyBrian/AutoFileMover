using System;
using System.ComponentModel;
using System.Deployment.Application;

namespace AutoFileMover.Desktop.Interfaces
{
    public delegate void CheckForUpdateCompletedEventHandler(object sender, CheckForUpdateCompletedEventArgs e);
    public delegate void DeploymentProgressChangedEventHandler(object sender, DeploymentProgressChangedEventArgs e);
    public delegate void DownloadFileGroupCompletedEventHandler(object sender, DownloadFileGroupCompletedEventArgs e);

    public interface IApplicationDeployment
    {
        Uri ActivationUri { get; }
        Version CurrentVersion { get; }
        string DataDirectory { get; }
        bool IsFirstRun { get; }
        bool IsNetworkDeployed { get; }
        DateTime TimeOfLastUpdateCheck { get; }
        string UpdatedApplicationFullName { get; }
        Version UpdatedVersion { get; }
        Uri UpdateLocation { get; }

        event CheckForUpdateCompletedEventHandler CheckForUpdateCompleted;
        event DeploymentProgressChangedEventHandler CheckForUpdateProgressChanged;
        event DownloadFileGroupCompletedEventHandler DownloadFileGroupCompleted;
        event DeploymentProgressChangedEventHandler DownloadFileGroupProgressChanged;
        event AsyncCompletedEventHandler UpdateCompleted;
        event DeploymentProgressChangedEventHandler UpdateProgressChanged;

        UpdateCheckInfo CheckForDetailedUpdate();
        UpdateCheckInfo CheckForDetailedUpdate(bool persistUpdateCheckResult);

        bool CheckForUpdate();
        bool CheckForUpdate(bool persistUpdateCheckResult);
        void CheckForUpdateAsync();
        void CheckForUpdateAsyncCancel();
        void DownloadFileGroup(string groupName);
        void DownloadFileGroupAsync(string groupName);
        void DownloadFileGroupAsync(string groupName, object userState);
        void DownloadFileGroupAsyncCancel(string groupName);
        bool IsFileGroupDownloaded(string groupName);
        bool Update();
        void UpdateAsync();
        void UpdateAsyncCancel();
    }

    public class CheckForUpdateCompletedEventArgs : AsyncCompletedEventArgs
    {
        public Version AvailableVersion { get; private set; }
        public bool IsUpdateRequired { get; private set; }
        public Version MinimumRequiredVersion { get; private set; }
        public bool UpdateAvailable { get; private set; }
        public long UpdateSizeBytes { get; private set; }

        public CheckForUpdateCompletedEventArgs(Version availableVersion, bool isUpdateRequired, Version minimumRequiredVersion, bool updateAvailable, long updateSizeBytes, Exception error, bool cancelled, object userState)
            : base(error, cancelled, userState)
        {
            AvailableVersion = availableVersion;
            IsUpdateRequired = isUpdateRequired;
            MinimumRequiredVersion = minimumRequiredVersion;
            UpdateAvailable = updateAvailable;
            UpdateSizeBytes = updateSizeBytes;
        }
    }

    public class DeploymentProgressChangedEventArgs : ProgressChangedEventArgs
    {
        public long BytesCompleted { get; private set; }
        public long BytesTotal { get; private set; }
        public string Group { get; private set; }
        public DeploymentProgressState State { get; private set; }

        public DeploymentProgressChangedEventArgs(long bytesCompleted, long bytesTotal, string group, DeploymentProgressState state, int progressPercentage, object userState)
            : base(progressPercentage, userState)
        {
            BytesCompleted = bytesCompleted;
            BytesTotal = bytesTotal;
            Group = group;
            State = state;
        }
    }

    public class DownloadFileGroupCompletedEventArgs : AsyncCompletedEventArgs
    {
        public string Group { get; private set; }

        public DownloadFileGroupCompletedEventArgs(string group, Exception error, bool cancelled, object userState)
            : base(error, cancelled, userState)
        {
            Group = group;
        }
    }
}
