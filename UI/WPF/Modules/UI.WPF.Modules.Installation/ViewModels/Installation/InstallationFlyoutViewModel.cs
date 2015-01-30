#region Usings

using System;
using System.Collections.Generic;
using System.Windows.Input;
using ReactiveUI.Legacy;
using UI.WPF.Launcher.Common.Classes;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels.Installation
{
    public class InstallationFlyoutViewModel : ReactiveObjectBase
    {
        private InstallationItemParent _installationParent;

        private InstallationItemParent _uninstallationParent;

        public ICommand CloseCommand { get; private set; }

        public InstallationFlyoutViewModel(Action closeAction)
        {
            var cmd = new ReactiveCommand();
            cmd.Subscribe(_ => closeAction());
            CloseCommand = cmd;
        }

        public InstallationItemParent InstallationParent
        {
            get { return _installationParent; }
            private set { RaiseAndSetIfPropertyChanged(ref _installationParent, value); }
        }

        public InstallationItemParent UninstallationParent
        {
            get { return _uninstallationParent; }
            private set { RaiseAndSetIfPropertyChanged(ref _uninstallationParent, value); }
        }

        public IEnumerable<InstallationItem> InstallationItems
        {
            set { InstallationParent = new InstallationItemParent(null, value); }
        }

        public IEnumerable<InstallationItem> UninstallationItems
        {
            set { UninstallationParent = new InstallationItemParent(null, value); }
        }
    }
}
