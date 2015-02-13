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
using ModInstallation.Util;
using ReactiveUI;
using Semver;
using Splat;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Common.Services;
using UI.WPF.Modules.Installation.ViewModels.Installation;
using UI.WPF.Modules.Installation.ViewModels.Mods;
using IDependencyResolver = ModInstallation.Interfaces.IDependencyResolver;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels
{
    public enum InstallationViewModelState
    {
        /// <summary>
        ///     The initial view shown that shows which packages are available
        /// </summary>
        PackagesOverview,

        /// <summary>
        ///     Shows the operations that will be executed
        /// </summary>
        OperationsPreview,

        /// <summary>
        ///     Shows the actual progress of the individual operations
        /// </summary>
        OperationsProgress
    }

    [Export(typeof(ILauncherTab)), ExportMetadata("Priority", 3)]
    public sealed class InstallationTabViewModel : Screen, ILauncherTab, IEnableLogger
    {
        private bool _hasManagerStatusMessage;

        private bool _installationInProgress;

        private double _installationProgress;

        private InstallationViewModel _installationViewModel;

        private bool _interactionEnabled;

        private string _managerStatusMessage;

        private IReadOnlyReactiveList<ModGroupViewModel> _modGroupViewModels;

        private OperationOverviewViewModel _operationOverviewViewModel;

        private InstallationViewModelState _state;

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

            var installModsCommand = ReactiveCommand.Create();
            installModsCommand.Subscribe(_ => InstallMods());

            InstallModsCommand = installModsCommand;

            this.WhenAny(x => x.ManagerStatusMessage, val => !string.IsNullOrEmpty(val.Value)).BindTo(this, x => x.HasManagerStatusMessage);

            ProfileManager.WhenAnyValue(x => x.CurrentProfile.SelectedTotalConversion.RootFolder).Subscribe(rootFolder =>
            {
                PackageInstaller.InstallationDirectory = rootFolder;
                LocalModManager.PackageDirectory = Path.Combine(rootFolder, "mods");
            });

            this.WhenAnyValue(x => x.InstallationInProgress).Select(b => !b).BindTo(this, x => x.InteractionEnabled);
            InteractionEnabledObservable = this.WhenAnyValue(x => x.InteractionEnabled);
            UpdateModsCommand = ReactiveCommand.CreateAsyncTask(_ => UpdateMods());

            InstallationInProgress = false;
            InstallationViewModel = new InstallationViewModel(() => State = InstallationViewModelState.PackagesOverview);

            State = InstallationViewModelState.PackagesOverview;
        }

        public IEnumerable<ModViewModel> ModificationViewModels
        {
            get { return ModGroupViewModels.Where(x => x.CurrentMod != null).Select(x => x.CurrentMod); }
        }

        public OperationOverviewViewModel OperationOverviewViewModel
        {
            get { return _operationOverviewViewModel; }
            set
            {
                if (Equals(value, _operationOverviewViewModel))
                {
                    return;
                }
                _operationOverviewViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        public InstallationViewModelState State
        {
            get { return _state; }
            private set
            {
                if (value == _state)
                {
                    return;
                }
                _state = value;
                NotifyOfPropertyChange();
            }
        }

        public InstallationViewModel InstallationViewModel
        {
            get { return _installationViewModel; }
            private set
            {
                if (Equals(value, _installationViewModel))
                {
                    return;
                }
                _installationViewModel = value;
                NotifyOfPropertyChange();
            }
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
        public IReadOnlyReactiveList<ModGroupViewModel> ModGroupViewModels
        {
            get { return _modGroupViewModels; }
            private set
            {
                if (Equals(value, _modGroupViewModels))
                {
                    return;
                }
                _modGroupViewModels = value;
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

        [NotNull, Import]
        private ITaskbarController TaskbarController { get; set; }

        private async void InstallMods()
        {
            if (InstallationInProgress)
            {
                State = InstallationViewModelState.OperationsProgress;
                return;
            }

            if (ProfileManager.CurrentProfile == null)
            {
                return;
            }

            if (ProfileManager.CurrentProfile.SelectedTotalConversion == null)
            {
                return;
            }

            State = InstallationViewModelState.OperationsPreview;

            var installationItems = GetInstallationItems().ToList();
            var uninstallationItems = GetUninstallationItems().ToList();

            if (!await ShouldContinueInstallation(installationItems, uninstallationItems))
            {
                // User cancelled installation
                State = InstallationViewModelState.PackagesOverview;
                return;
            }

            InstallationInProgress = true;
            TaskbarController.ProgressbarVisible = true;
            TaskbarController.ProgressvarValue = 0.0;

            try
            {
                InstallationViewModel.InstallationItems = installationItems;
                InstallationViewModel.UninstallationItems = uninstallationItems;
                State = InstallationViewModelState.OperationsProgress;

                using (InstallationViewModel.InstallationParent.WhenAnyValue(x => x.Progress).Subscribe(p =>
                {
                    TaskbarController.ProgressvarValue = p;
                    InstallationProgress = p;
                }))
                {
                    await InstallationViewModel.InstallationParent.Install();
                }
            }
            finally
            {
                State = InstallationViewModelState.PackagesOverview;
                InstallationInProgress = false;
                TaskbarController.ProgressbarVisible = false;
                TaskbarController.ProgressvarValue = 0.0;
            }
        }

        private Task<bool> ShouldContinueInstallation(IEnumerable<InstallationItem> installationItems,
            IEnumerable<InstallationItem> uninstallationItems)
        {
            var tcs = new TaskCompletionSource<bool>();

            OperationOverviewViewModel = new OperationOverviewViewModel(() => tcs.TrySetResult(false), () => tcs.TrySetResult(true))
            {
                InstallationItems = installationItems,
                UninstallationItems = uninstallationItems
            };

            return tcs.Task;
        }

        private IEnumerable<InstallationItem> GetUninstallationItems()
        {
            return Enumerable.Empty<InstallationItem>();
        }

        private bool PackageSelector(PackageViewModel model)
        {
            return model.Selected && !LocalModManager.IsPackageInstalled(model.Package);
        }

        private IEnumerable<InstallationItem> GetInstallationItems()
        {
            return ModificationViewModels.Where(modvm => modvm.Packages.Any(PackageSelector)).Select(GetModInstallationItem);
        }

        private InstallationItem GetModInstallationItem(ModViewModel modvm)
        {
            return new InstallationItemParent(modvm.Mod.Title, modvm.Packages.Where(PackageSelector).Select(GetPackageInstallationItem));
        }

        private InstallationItem GetPackageInstallationItem(PackageViewModel packvm)
        {
            return new PackageInstallationItem(packvm.Package, PackageInstaller, LocalModManager);
        }

        [NotNull]
        private async Task UpdateMods()
        {
            ManagerStatusMessage = "Parsing local mod information...";
            await LocalModManager.ParseLocalModDataAsync();

            await RemoteModManager.RetrieveInformationAsync(new Progress<string>(msg => ManagerStatusMessage = msg), CancellationToken.None);

            if (RemoteModManager.ModificationGroups != null)
            {
                ModGroupViewModels = RemoteModManager.ModificationGroups.CreateDerivedCollection(group => new ModGroupViewModel(group, this));
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

            if (RemoteModManager.ModificationGroups == null)
            {
                UpdateModsCommand.Execute(null);
            }
        }
    }
}
