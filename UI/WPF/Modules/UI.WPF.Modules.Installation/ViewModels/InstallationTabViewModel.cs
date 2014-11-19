#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using ModInstallation.Annotations;
using ModInstallation.Interfaces;
using ReactiveUI;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Common.Services;
using UI.WPF.Modules.Installation.ViewModels.Mods;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels
{
    [Export(typeof(ILauncherTab)), ExportMetadata("Priority", 3)]
    public sealed class InstallationTabViewModel : Screen, ILauncherTab
    {
        private bool _hasManagerStatusMessage;

        private bool _installationInProgress;

        private double _installationProgress;

        private string _managerStatusMessage;

        private IReadOnlyReactiveList<ModViewModel> _modificationViewModels;

        private bool _interactionEnabled;

        [ImportingConstructor]
        public InstallationTabViewModel([NotNull] IRepositoryFactory repositoryFactory,
            [NotNull] IRemoteModManager remoteManager,
            [NotNull] ILocalModManager localManager,
            [NotNull] IProfileManager profileManager,
            [NotNull] IPackageInstaller packageInstaller,
            [NotNull] ILauncherViewModel launcherVm)
        {
            DisplayName = "Update/Install mods";

            ProfileManager = profileManager;

            RemoteModManager = remoteManager;
            RemoteModManager.Repositories = launcherVm.ModRepositories.CreateDerivedCollection(vm => vm.Repository);

            LocalModManager = localManager;

            PackageInstaller = packageInstaller;

            InstallModsCommand = ReactiveCommand.CreateAsyncTask(_ => InstallMods());

            this.WhenAny(x => x.ManagerStatusMessage, val => !string.IsNullOrEmpty(val.Value)).BindTo(this, x => x.HasManagerStatusMessage);

            ProfileManager.WhenAnyValue(x => x.CurrentProfile.SelectedTotalConversion.RootFolder).Subscribe(rootFolder =>
            {
                PackageInstaller.InstallationDirectory = Path.Combine(rootFolder, "mods");
                LocalModManager.PackageDirectory = Path.Combine(rootFolder, "mods", "packages");
            });

            this.WhenAnyValue(x => x.InstallationInProgress).Select(b => !b).BindTo(this, x => x.InteractionEnabled);
            InteractionEnabledObservable = this.WhenAnyValue(x => x.InteractionEnabled);
            UpdateModsCommand = ReactiveCommand.CreateAsyncTask(_ => UpdateMods());

            InstallationInProgress = false;
        }

        [NotNull]
        public ICommand UpdateModsCommand { get; private set; }

        [NotNull]
        public ILocalModManager LocalModManager { get; private set; }

        [NotNull]
        public ICommand InstallModsCommand { get; private set; }

        [NotNull]
        private IProfileManager ProfileManager { get; set; }

        [NotNull]
        private IPackageInstaller PackageInstaller { get; set; }

        [NotNull, Import]
        public IDependencyResolver DependencyResolver { get; private set; }

        public double InstallationProgress
        {
            get { return _installationProgress; }
            private set
            {
                if (value.Equals(_installationProgress))
                {
                    return;
                }
                _installationProgress = value;
                NotifyOfPropertyChange();
            }
        }

        public bool HasManagerStatusMessage
        {
            get { return _hasManagerStatusMessage; }
            private set
            {
                if (value.Equals(_hasManagerStatusMessage))
                {
                    return;
                }
                _hasManagerStatusMessage = value;
                NotifyOfPropertyChange();
            }
        }

        [CanBeNull]
        public string ManagerStatusMessage
        {
            get { return _managerStatusMessage; }
            private set
            {
                if (value == _managerStatusMessage)
                {
                    return;
                }
                _managerStatusMessage = value;
                NotifyOfPropertyChange();
            }
        }

        [NotNull]
        public IRemoteModManager RemoteModManager { get; private set; }

        [NotNull]
        public IReadOnlyReactiveList<ModViewModel> ModificationViewModels
        {
            get { return _modificationViewModels; }
            private set
            {
                if (Equals(value, _modificationViewModels))
                {
                    return;
                }
                _modificationViewModels = value;
                NotifyOfPropertyChange();
            }
        }

        public bool InstallationInProgress
        {
            get { return _installationInProgress; }
            private set
            {
                if (value.Equals(_installationInProgress))
                {
                    return;
                }
                _installationInProgress = value;
                NotifyOfPropertyChange();
            }
        }

        public bool InteractionEnabled
        {
            get { return _interactionEnabled; }
            private set
            {
                if (value.Equals(_interactionEnabled))
                {
                    return;
                }
                _interactionEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        [NotNull]
        public IObservable<bool> InteractionEnabledObservable { get; private set; }

        [NotNull,Import]
        private ITaskbarController TaskbarController { get; set; }

        [NotNull]
        private async Task InstallMods()
        {
            if (ProfileManager.CurrentProfile == null)
            {
                return;
            }

            if (ProfileManager.CurrentProfile.SelectedTotalConversion == null)
            {
                return;
            }

            InstallationInProgress = true;
            TaskbarController.ProgressbarVisible = true;
            TaskbarController.ProgressvarValue = 0.0;

            var progressDict = new Dictionary<PackageViewModel, double>();
            try
            {
                var packageViewModels = ModificationViewModels.SelectMany(mod => mod.Packages).Where(pack => pack.Selected);

                var installTasks = packageViewModels.Select(packageViewModel => packageViewModel.Install(PackageInstaller,
                    progress =>
                    {
                        progressDict[packageViewModel] = progress;
                        InstallationProgress = progressDict.Aggregate(0.0, (sum, pair) => sum + pair.Value) / progressDict.Count;
                        TaskbarController.ProgressvarValue = InstallationProgress;
                    }));

                await Task.WhenAll(installTasks);
            }
            finally
            {
                InstallationInProgress = false;
                TaskbarController.ProgressbarVisible = false;
                TaskbarController.ProgressvarValue = 0.0;
            }
        }

        [NotNull]
        private async Task UpdateMods()
        {
            await RemoteModManager.RetrieveInformationAsync(new Progress<string>(msg => ManagerStatusMessage = msg), CancellationToken.None);

            if (RemoteModManager.Modifications != null)
            {
                ModificationViewModels = RemoteModManager.Modifications.CreateDerivedCollection(mod => new ModViewModel(mod, this));
            }
            else
            {
                var result =
                    await
                        UserError.Throw(new UserError("Mod information retrieval failed!",
                            "There was an error while retrieving the modification information.\n" +
                            "Please check if your internet connection is working.",
                            new[]
                            {
                                new RecoveryCommand("Retry", _ => RecoveryOptionResult.RetryOperation)
                                {
                                    IsDefault = true
                                },
                                new RecoveryCommand("Cancel", _ => RecoveryOptionResult.CancelOperation)
                            }));

                switch (result)
                {
                    case RecoveryOptionResult.CancelOperation:
                        break;
                    case RecoveryOptionResult.RetryOperation:
                        UpdateModsCommand.Execute(null);
                        break;
                }
            }

            ManagerStatusMessage = null;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            if (RemoteModManager.Modifications == null)
            {
                UpdateModsCommand.Execute(null);
            }
        }
    }
}
