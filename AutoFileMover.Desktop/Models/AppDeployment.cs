using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoFileMover.Desktop.Interfaces;

namespace AutoFileMover.Desktop.Models
{
    public class AppDeployment : IApplicationDeployment
    {

        public AppDeployment()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment.CurrentDeployment.CheckForUpdateCompleted += CurrentDeployment_CheckForUpdateCompleted;
                ApplicationDeployment.CurrentDeployment.CheckForUpdateProgressChanged += CurrentDeployment_CheckForUpdateProgressChanged;
                ApplicationDeployment.CurrentDeployment.DownloadFileGroupCompleted += CurrentDeployment_DownloadFileGroupCompleted;
                ApplicationDeployment.CurrentDeployment.DownloadFileGroupProgressChanged += CurrentDeployment_DownloadFileGroupProgressChanged;
                ApplicationDeployment.CurrentDeployment.UpdateCompleted += CurrentDeployment_UpdateCompleted;
                ApplicationDeployment.CurrentDeployment.UpdateProgressChanged += CurrentDeployment_UpdateProgressChanged;
            }
        }

        ~AppDeployment()
        {
            try
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    ApplicationDeployment.CurrentDeployment.CheckForUpdateCompleted -= CurrentDeployment_CheckForUpdateCompleted;
                    ApplicationDeployment.CurrentDeployment.CheckForUpdateProgressChanged -= CurrentDeployment_CheckForUpdateProgressChanged;
                    ApplicationDeployment.CurrentDeployment.DownloadFileGroupCompleted -= CurrentDeployment_DownloadFileGroupCompleted;
                    ApplicationDeployment.CurrentDeployment.DownloadFileGroupProgressChanged -= CurrentDeployment_DownloadFileGroupProgressChanged;
                    ApplicationDeployment.CurrentDeployment.UpdateCompleted -= CurrentDeployment_UpdateCompleted;
                    ApplicationDeployment.CurrentDeployment.UpdateProgressChanged -= CurrentDeployment_UpdateProgressChanged;
                }
            }
            finally { }
        }

        void CurrentDeployment_UpdateProgressChanged(object sender, System.Deployment.Application.DeploymentProgressChangedEventArgs e)
        {
            var handler = UpdateProgressChanged;
            if (handler != null)
            {
                handler(this, new AutoFileMover.Desktop.Interfaces.DeploymentProgressChangedEventArgs(e.BytesCompleted, e.BytesTotal, e.Group, e.State, e.ProgressPercentage, e.UserState));
            }
        }

        void CurrentDeployment_UpdateCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var handler = UpdateCompleted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void CurrentDeployment_DownloadFileGroupProgressChanged(object sender, System.Deployment.Application.DeploymentProgressChangedEventArgs e)
        {
            var handler = DownloadFileGroupProgressChanged;
            if (handler != null)
            {
                handler(this, new AutoFileMover.Desktop.Interfaces.DeploymentProgressChangedEventArgs(e.BytesCompleted, e.BytesTotal, e.Group, e.State, e.ProgressPercentage, e.UserState));
            }
        }

        void CurrentDeployment_DownloadFileGroupCompleted(object sender, System.Deployment.Application.DownloadFileGroupCompletedEventArgs e)
        {
            var handler = DownloadFileGroupCompleted;
            if (handler != null)
            {
                handler(this, new AutoFileMover.Desktop.Interfaces.DownloadFileGroupCompletedEventArgs(e.Group, e.Error, e.Cancelled, e.UserState));
            }
        }

        void CurrentDeployment_CheckForUpdateProgressChanged(object sender, System.Deployment.Application.DeploymentProgressChangedEventArgs e)
        {
            var handler = CheckForUpdateProgressChanged;
            if (handler != null)
            {
                handler(this, new AutoFileMover.Desktop.Interfaces.DeploymentProgressChangedEventArgs(e.BytesCompleted, e.BytesTotal, e.Group, e.State, e.ProgressPercentage, e.UserState));
            }
        }

        void CurrentDeployment_CheckForUpdateCompleted(object sender, System.Deployment.Application.CheckForUpdateCompletedEventArgs e)
        {
            var handler = CheckForUpdateCompleted;
            if (handler != null)
            {
                handler(this, new AutoFileMover.Desktop.Interfaces.CheckForUpdateCompletedEventArgs(e.UpdateAvailable ? e.AvailableVersion : CurrentVersion, 
                                                                                                    e.UpdateAvailable ? e.IsUpdateRequired : false, 
                                                                                                    e.UpdateAvailable ? e.MinimumRequiredVersion : CurrentVersion, 
                                                                                                    e.UpdateAvailable, 
                                                                                                    e.UpdateAvailable ? e.UpdateSizeBytes : 0, 
                                                                                                    e.Error, e.Cancelled, e.UserState));
            }
        }

        public Uri ActivationUri
        {
            get 
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    return ApplicationDeployment.CurrentDeployment.ActivationUri; 
                }
                else
                {
                    return null;
                }
            }
        }

        public Version CurrentVersion
        {
            get 
            { 
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    return ApplicationDeployment.CurrentDeployment.CurrentVersion;
                }
                else
                {
                    return Assembly.GetEntryAssembly().GetName().Version;
                }
            }
        }

        public string DataDirectory
        {
            get 
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    return ApplicationDeployment.CurrentDeployment.DataDirectory;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool IsFirstRun
        {
            get 
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    return ApplicationDeployment.CurrentDeployment.IsFirstRun;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsNetworkDeployed
        {
            get { return ApplicationDeployment.IsNetworkDeployed; }
        }

        public DateTime TimeOfLastUpdateCheck
        {
            get 
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    return ApplicationDeployment.CurrentDeployment.TimeOfLastUpdateCheck;
                }
                else
                {
                    return new DateTime();
                }
            }
        }

        public string UpdatedApplicationFullName
        {
            get 
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    return ApplicationDeployment.CurrentDeployment.UpdatedApplicationFullName;
                }
                else
                {
                    return Assembly.GetEntryAssembly().GetName().Name;
                }
            }
        }

        public Version UpdatedVersion
        {
            get 
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    return ApplicationDeployment.CurrentDeployment.UpdatedVersion;
                }
                else
                {
                    return Assembly.GetEntryAssembly().GetName().Version;
                }
            }
        }

        public Uri UpdateLocation
        {
            get 
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    return ApplicationDeployment.CurrentDeployment.UpdateLocation;
                }
                else
                {
                    return null;
                }
            }
        }

        public event AutoFileMover.Desktop.Interfaces.CheckForUpdateCompletedEventHandler CheckForUpdateCompleted;

        public event AutoFileMover.Desktop.Interfaces.DeploymentProgressChangedEventHandler CheckForUpdateProgressChanged;

        public event AutoFileMover.Desktop.Interfaces.DownloadFileGroupCompletedEventHandler DownloadFileGroupCompleted;

        public event AutoFileMover.Desktop.Interfaces.DeploymentProgressChangedEventHandler DownloadFileGroupProgressChanged;

        public event AsyncCompletedEventHandler UpdateCompleted;

        public event AutoFileMover.Desktop.Interfaces.DeploymentProgressChangedEventHandler UpdateProgressChanged;

        public UpdateCheckInfo CheckForDetailedUpdate()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                return ApplicationDeployment.CurrentDeployment.CheckForDetailedUpdate();
            }
            else
            {
                return null;
            }
        }

        public UpdateCheckInfo CheckForDetailedUpdate(bool persistUpdateCheckResult)
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                return ApplicationDeployment.CurrentDeployment.CheckForDetailedUpdate(persistUpdateCheckResult);
            }
            else
            {
                return null;
            }
        }

        public bool CheckForUpdate()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                return ApplicationDeployment.CurrentDeployment.CheckForUpdate();
            }
            else
            {
                return false;
            }
        }

        public bool CheckForUpdate(bool persistUpdateCheckResult)
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                return ApplicationDeployment.CurrentDeployment.CheckForUpdate(persistUpdateCheckResult);
            }
            else
            {
                return false;
            }
        }

        public void CheckForUpdateAsync()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment.CurrentDeployment.CheckForUpdateAsync();
            }
        }

        public void CheckForUpdateAsyncCancel()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment.CurrentDeployment.CheckForUpdateAsyncCancel();
            }
        }

        public void DownloadFileGroup(string groupName)
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment.CurrentDeployment.DownloadFileGroup(groupName);
            }
        }

        public void DownloadFileGroupAsync(string groupName)
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment.CurrentDeployment.DownloadFileGroupAsync(groupName);
            }
        }

        public void DownloadFileGroupAsync(string groupName, object userState)
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment.CurrentDeployment.DownloadFileGroupAsync(groupName, userState);
            }
        }

        public void DownloadFileGroupAsyncCancel(string groupName)
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment.CurrentDeployment.DownloadFileGroupAsyncCancel(groupName);
            }
        }

        public bool IsFileGroupDownloaded(string groupName)
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                return ApplicationDeployment.CurrentDeployment.IsFileGroupDownloaded(groupName);
            }
            else
            {
                return false;
            }
        }

        public bool Update()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                return ApplicationDeployment.CurrentDeployment.Update();
            }
            else
            {
                return false;
            }
        }

        public void UpdateAsync()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment.CurrentDeployment.UpdateAsync();
            }
        }

        public void UpdateAsyncCancel()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment.CurrentDeployment.UpdateAsyncCancel();
            }
        }
    }
}
