using System;
using System.Deployment.Application;
using UI.WPF.Launcher.Common.Services;

namespace UI.WPF.Modules.Update.Services
{
    internal class UpdateStatus : IUpdateStatus
    {
        public UpdateStatus(Version version, bool updateAvailable, bool isRequired)
        {
            Version = version;
            UpdateAvailable = updateAvailable;
            IsRequired = isRequired;
        }

        public UpdateStatus()
        {
            Version = null;
            UpdateAvailable = false;
            IsRequired = false;
        }

        #region IUpdateStatus Members

        public Version Version { get; private set; }

        public bool UpdateAvailable { get; private set; }

        public bool IsRequired { get; private set; }

        #endregion
    }
}