using System.Deployment.Application;
using UI.WPF.Launcher.Common.Services;

namespace UI.WPF.Modules.Update.Services
{
    internal class UpdateStatus : IUpdateStatus
    {
        public UpdateStatus(CheckForUpdateCompletedEventArgs eventArgs)
        {
            if (eventArgs == null)
            {
                Version = null;
                UpdateAvailable = false;
                IsRequired = false;
            }
            else
            {
                var version = eventArgs.AvailableVersion;

                Version = new UpdateVersion(version.Major, version.Minor, version.Revision, version.Build);
                UpdateAvailable = eventArgs.UpdateAvailable;
                IsRequired = eventArgs.IsUpdateRequired;
            }
        }

        public UpdateStatus(UpdateVersion version, bool updateAvailable, bool isRequired)
        {
            Version = version;
            UpdateAvailable = updateAvailable;
            IsRequired = isRequired;
        }

        #region IUpdateStatus Members

        public UpdateVersion Version { get; private set; }

        public bool UpdateAvailable { get; private set; }

        public bool IsRequired { get; private set; }

        #endregion
    }
}