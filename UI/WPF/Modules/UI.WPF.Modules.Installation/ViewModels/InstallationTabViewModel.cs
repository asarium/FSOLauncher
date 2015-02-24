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
using ModInstallation.Exceptions;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Util;
using ReactiveUI;
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

    internal class ModDependencyException : DependencyException
    {
        public ModDependencyException(IModification mod) : base(mod.Title)
        {
            Mod = mod;
        }

        public IModification Mod { get; private set; }
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

        [CanBeNull]
        public IEnumerable<ModViewModel> ModificationViewModels
        {
            get
            {
                return ModGroupViewModels == null ? null : ModGroupViewModels.Where(x => x.CurrentMod != null).Select(x => x.CurrentMod);
            }
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

        [CanBeNull]
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

        [NotNull, Import]
        private IInteractionService InteractionService { get; set; }

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

            var dependentPackages = await GetPackageDependencies().ConfigureAwait(true);

            var installationItems = GetInstallationItems().ToList();
            var uninstallationItems = GetUninstallationItems().ToList();

            if (!await ShouldContinueInstallation(dependentPackages, installationItems, uninstallationItems).ConfigureAwait(true))
            {
                // User cancelled installation
                State = InstallationViewModelState.PackagesOverview;
                return;
            }

            installationItems = GetDependencyItems(dependentPackages).Concat(installationItems).ToList();

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
                    await
                        Task.WhenAll(InstallationViewModel.UninstallationParent.Install(), InstallationViewModel.InstallationParent.Install())
                            .ConfigureAwait(true);
                }
            }
            finally
            {
                InstallationInProgress = false;
                TaskbarController.ProgressbarVisible = false;
                TaskbarController.ProgressvarValue = 0.0;
            }

            await InstallationViewModel.WaitforCloseAsync();
            State = InstallationViewModelState.PackagesOverview;
        }

        private IEnumerable<InstallationItem> GetDependencyItems(IEnumerable<IPackage> dependencies)
        {
            var modGroups = dependencies.GroupBy(x => x.ContainingModification);

            return
                modGroups.Select(
                    group =>
                        new InstallationItemParent(group.Key.Title,
                            group.Select(x => new PackageInstallationItem(x, PackageInstaller, LocalModManager)).ToList())).ToList();
        }

        private async Task<IList<IPackage>> GetPackageDependencies()
        {
            // This loop will be terminated when a valid result has been found
            while (true)
            {
                ManagerStatusMessage = "Checking installation dependencies...";
                try
                {
                    // Do this in a background thread as it might take some time...
                    ModDependencyException exception;
                    try
                    {
                        var result = await Task.Run(() => ResolveDependencies().ToList()).ConfigureAwait(false);

                        return result;
                    }
                    catch (ModDependencyException e)
                    {
                        exception = e;
                    }

                    var error = new UserError("Dependency not available",
                        string.Format(
                            "A dependency for the mod {0} is not available. This might only be a temporary issue.\n" +
                            "Please contact the mod author if this issue persists.",
                            exception.Mod.Title),
                        new[]
                        {
                            new RecoveryCommand("Remove mod", _ => RecoveryOptionResult.RetryOperation)
                            {
                                IsDefault = true
                            },
                            new RecoveryCommand("Cancel", _ => RecoveryOptionResult.CancelOperation),
                        });

                    var recovery = await UserError.Throw(error);

                    switch (recovery)
                    {
                        case RecoveryOptionResult.CancelOperation:
                            return null;
                        case RecoveryOptionResult.RetryOperation:
                            if (ModGroupViewModels != null)
                            {
                                var viewModel = ModGroupViewModels.FirstOrDefault(group => @group.Group.Id == exception.Mod.Id);

                                if (viewModel != null)
                                {
                                    viewModel.IsSelected = false;
                                }
                            }
                            break;
                        case RecoveryOptionResult.FailOperation:
                            throw exception;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                finally
                {
                    ManagerStatusMessage = null;
                }
            }
        }

        private IEnumerable<IPackage> ResolveDependencies()
        {
            if (ModGroupViewModels == null || ModificationViewModels == null)
            {
                return Enumerable.Empty<IPackage>();
            }

            var result = Enumerable.Empty<IPackage>();
            var localMods = (LocalModManager.Modifications ?? Enumerable.Empty<IModification>()).ToList();

            var allMods = localMods;
            var remoteMods = ModGroupViewModels.SelectMany(model => model.Group.Versions.Values);

            allMods = allMods.Concat(remoteMods).ToList();

            foreach (var mod in ModificationViewModels)
            {
                ManagerStatusMessage = string.Format("Processing dependencies for {0}...", mod.Mod.Title);

                var packages = mod.Packages.Where(x => x.Selected).Select(x => x.Package);

                foreach (var package in packages)
                {
                    if (LocalModManager.IsPackageInstalled(package))
                    {
                        // Skip already installed packages
                        continue;
                    }

                    IEnumerable<IPackage> dependencies = null;
                    if (localMods.Count > 0)
                    {
                        try
                        {
                            dependencies = DependencyResolver.ResolveDependencies(package, localMods);
                        }
                        catch (DependencyException)
                        {
                            // Could not satisfy dependencies with local mods...
                        }
                    }

                    if (dependencies == null)
                    {
                        try
                        {
                            // This might throw if a dependency could not be found, it is handled in the calling function
                            dependencies = DependencyResolver.ResolveDependencies(package, allMods);
                        }
                        catch (DependencyException)
                        {
                            throw new ModDependencyException(mod.Mod);
                        }
                        catch (InvalidOperationException)
                        {
                            // That's not good, there is a cyclic dependency! Thow a dependency exception to let the user decide what to do
                            throw new ModDependencyException(mod.Mod);
                        }
                    }

                    // Only return packages that are not already installed or are already being installed (the current package)
                    var package1 = package;
                    result = result.Union(dependencies.Where(p => !LocalModManager.IsPackageInstalled(p) && !ReferenceEquals(package1, p)));
                }
            }

            return result;
        }

        private Task<bool> ShouldContinueInstallation(IEnumerable<IPackage> dependentPackages,
            IEnumerable<InstallationItem> installationItems,
            IEnumerable<InstallationItem> uninstallationItems)
        {
            var tcs = new TaskCompletionSource<bool>();

            State = InstallationViewModelState.OperationsPreview;
            OperationOverviewViewModel = new OperationOverviewViewModel(dependentPackages, () => tcs.TrySetResult(false), () => tcs.TrySetResult(true))
            {
                InstallationItems = installationItems,
                UninstallationItems = uninstallationItems
            };

            return tcs.Task;
        }

        private IEnumerable<InstallationItem> GetUninstallationItems()
        {
            if (ModificationViewModels == null)
            {
                return Enumerable.Empty<InstallationItem>();
            }

            return
                ModificationViewModels.Where(modVm => modVm.Packages.Any(UninstallPackageSelector))
                    .Select(
                        modVm =>
                            new InstallationItemParent(modVm.Mod.Title,
                                modVm.Packages.Where(UninstallPackageSelector)
                                    .Select(x => new PackageUninstallationItem(x.Package, PackageInstaller, LocalModManager))));
        }

        private bool UninstallPackageSelector(PackageViewModel model)
        {
            return !model.Selected && LocalModManager.IsPackageInstalled(model.Package);
        }

        private bool PackageSelector(PackageViewModel model)
        {
            return model.Selected && !LocalModManager.IsPackageInstalled(model.Package);
        }

        private IEnumerable<InstallationItem> GetInstallationItems()
        {
            if (ModificationViewModels == null)
            {
                return Enumerable.Empty<InstallationItem>();
            }

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
            await LocalModManager.ParseLocalModDataAsync().ConfigureAwait(false);

            var modGroups = await
                RemoteModManager.GetModGroupsAsync(new Progress<string>(msg => ManagerStatusMessage = msg), true, CancellationToken.None)
                    .ConfigureAwait(false);

            if (modGroups != null)
            {
                ModGroupViewModels = modGroups.CreateDerivedCollection(group => new ModGroupViewModel(group, this));
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

            if (ModGroupViewModels == null)
            {
                UpdateModsCommand.Execute(null);
            }
        }
    }
}
