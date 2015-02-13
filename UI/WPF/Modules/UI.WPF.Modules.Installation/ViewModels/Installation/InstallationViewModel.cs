#region Usings

using System;
using System.Collections.Generic;
using System.Windows.Input;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels.Installation
{
    public class InstallationViewModel : ReactiveObjectBase
    {
        private InstallationItemParent _installationParent;

        private InstallationItemParent _uninstallationParent;

        public ICommand CloseCommand { get; private set; }

        public InstallationViewModel(Action closeAction)
        {
            var cmd = ReactiveCommand.Create();
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
