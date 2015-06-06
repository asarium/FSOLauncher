#region Usings

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using ModInstallation.Interfaces.Mods;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Modules.Installation.Interfaces;
using UI.WPF.Modules.Installation.ViewModels.Dependencies;
using UI.WPF.Modules.Installation.ViewModels.Installation;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels.Operations
{
    public class OperationsOverviewViewModel : ReactiveObjectBase, IInstallationState
    {
        private InstallationItemParent _installationParent;

        private InstallationItemParent _uninstallationParent;
        
        private readonly TaskCompletionSource<StateResult> _resultTcs;

        public OperationsOverviewViewModel()
        {
            _resultTcs = new TaskCompletionSource<StateResult>();

            var cmd = ReactiveCommand.Create();
            cmd.Subscribe(_ => _resultTcs.TrySetResult(StateResult.Abort));
            AbortCommand = cmd;

            cmd = ReactiveCommand.Create();
            cmd.Subscribe(_ => _resultTcs.TrySetResult(StateResult.Continue));
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

        public Task<StateResult> GetResult()
        {
            return _resultTcs.Task;
        }
    }
}
