#region Usings

using System;
using System.Collections.Generic;
using System.Windows.Input;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Modules.Installation.ViewModels.Installation;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels
{
    public class OperationOverviewViewModel : ReactiveObjectBase
    {
        private InstallationItemParent _installationParent;

        private InstallationItemParent _uninstallationParent;

        public OperationOverviewViewModel(Action abortAction, Action continueAction)
        {
            var cmd = ReactiveCommand.Create();
            cmd.Subscribe(_ => abortAction());
            AbortCommand = cmd;

            cmd = ReactiveCommand.Create();
            cmd.Subscribe(_ => continueAction());
            ContinueCommand = cmd;
        }

        public ICommand AbortCommand { get; private set; }

        public ICommand ContinueCommand { get; private set; }

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
