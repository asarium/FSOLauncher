#region Usings

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Modules.Installation.Interfaces;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels.Installation
{
    public class InstallationViewModel : ReactiveObjectBase, IInstallationState
    {
        private InstallationItemParent _installationParent;

        private InstallationItemParent _uninstallationParent;

        public ReactiveCommand<object> CloseCommand { get; private set; }

        private readonly TaskCompletionSource<StateResult> _resultTcs;

        public InstallationViewModel()
        {
            _resultTcs = new TaskCompletionSource<StateResult>();

            CloseCommand =  ReactiveCommand.Create();
            CloseCommand.Subscribe(_ => _resultTcs.TrySetResult(StateResult.Abort));
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

        #region Implementation of IInstallationState

        public Task<StateResult> GetResult()
        {
            return _resultTcs.Task;
        }

        #endregion
    }
}
