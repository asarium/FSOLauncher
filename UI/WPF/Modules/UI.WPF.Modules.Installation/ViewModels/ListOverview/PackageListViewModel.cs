#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ModInstallation.Exceptions;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Util;
using ReactiveUI;
using Splat;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Launcher.Common.Services;
using UI.WPF.Modules.Installation.Interfaces;
using UI.WPF.Modules.Installation.ViewModels.Installation;
using UI.WPF.Modules.Installation.ViewModels.Mods;
using IDependencyResolver = ModInstallation.Interfaces.IDependencyResolver;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels.ListOverview
{
    public class PackageListViewModel : ReactiveObjectBase, IInstallationState
    {
        private readonly IDependencyResolver _dependencyResolver;

        private readonly IInteractionService _interactionService;

        private readonly IInstallationManager _manager;

        private readonly IModInstallationManager _modInstallation;

        public PackageListViewModel(IInstallationManager manager,
            IModInstallationManager modInstallation,
            IInteractionService interactionService = null,
            IDependencyResolver dependencyResolver = null)
        {
            _manager = manager;
            _modInstallation = modInstallation;
            _interactionService = interactionService ?? Locator.Current.GetService<IInteractionService>();
            _dependencyResolver = dependencyResolver ?? Locator.Current.GetService<IDependencyResolver>();
            ModGroupViewModels = manager.ModGroups.CreateDerivedCollection(x => new ModGroupViewModel(x, modInstallation));

            StartInstallationCommand = ReactiveCommand.CreateAsyncTask(async _ => await StartInstallation());
        }

        public IReactiveCommand StartInstallationCommand { get; private set; }

        public IReadOnlyReactiveList<ModGroupViewModel> ModGroupViewModels { get; private set; }

        private IEnumerable<InstallationItem> GetUninstallationItems()
        {
            return
                ModGroupViewModels.Select(x => x.CurrentMod)
                    .Where(modVm => modVm.Packages.Any(UninstallPackageSelector))
                    .Select(
                        modVm =>
                            new InstallationItemParent(modVm.Mod.Title,
                                modVm.Packages.Where(UninstallPackageSelector).Select(x => new PackageUninstallationItem(x.Package, _modInstallation))));
        }

        private async Task StartInstallation()
        {
            var installationItems = GetInstallationItems().ToList();
            var uninstallationItems = GetUninstallationItems().ToList();

            var dependencies = await GetPackageDependencies();

            await _manager.ExecuteChanges(installationItems.Concat(GetDependencyItems(dependencies)), uninstallationItems);
        }

        private IEnumerable<InstallationItem> GetDependencyItems(IEnumerable<IPackage> dependencies)
        {
            return
                dependencies.GroupBy(x => x.ContainingModification)
                    .Select(g => new InstallationItemParent(g.Key.Title, g.Select(p => new PackageInstallationItem(p, _modInstallation))));
        }

        private bool PackageSelector(PackageViewModel model)
        {
            return model.Selected && !_modInstallation.LocalModManager.IsPackageInstalled(model.Package);
        }

        private InstallationItem GetModInstallationItem(ModViewModel modvm)
        {
            return new InstallationItemParent(modvm.Mod.Title, modvm.Packages.Where(PackageSelector).Select(GetPackageInstallationItem));
        }

        private InstallationItem GetPackageInstallationItem(PackageViewModel packvm)
        {
            return new PackageInstallationItem(packvm.Package, _modInstallation);
        }

        private IEnumerable<InstallationItem> GetInstallationItems()
        {
            if (ModGroupViewModels == null)
            {
                return Enumerable.Empty<InstallationItem>();
            }

            return ModGroupViewModels.Select(g => g.CurrentMod).Where(modvm => modvm.Packages.Any(PackageSelector)).Select(GetModInstallationItem);
        }

        private async Task<IEnumerable<IPackage>> ResolveDependencies(Task<IProgressController> controllerTask)
        {
            if (_manager.ModGroups == null)
            {
                return Enumerable.Empty<IPackage>();
            }

            var result = Enumerable.Empty<IPackage>();
            var localMods = (_modInstallation.LocalModManager.Modifications ?? Enumerable.Empty<IModification>()).ToList();

            var allMods = localMods;
            var remoteMods = _manager.ModGroups.SelectMany(g => g.Versions.Values);

            allMods = allMods.Concat(remoteMods).ToList();

            IProgressController controller = null;

            var modsToProcess = ModGroupViewModels.Where(x => x.IsSelected != null && x.IsSelected.Value).Select(x => x.CurrentMod).ToList();

            for (var i = 0; i < modsToProcess.Count; ++i)
            {
                var mod = modsToProcess[i];

                if (controller == null && controllerTask.IsCompleted)
                {
                    controller = await controllerTask;
                }

                if (controller != null)
                {
                    controller.Progress = i / (double) modsToProcess.Count;
                    controller.Message = string.Format("Resolving dependencies for {0}...", mod.Mod.Title);
                }

                var packages = mod.Packages.Where(x => x.Selected).Select(x => x.Package);

                foreach (var package in packages)
                {
                    if (_modInstallation.LocalModManager.IsPackageInstalled(package))
                    {
                        // Skip already installed packages
                        continue;
                    }

                    IEnumerable<IPackage> dependencies = null;
                    if (localMods.Count > 0)
                    {
                        try
                        {
                            dependencies = _dependencyResolver.ResolveDependencies(package, localMods);
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
                            dependencies = _dependencyResolver.ResolveDependencies(package, allMods);
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
                    result =
                        result.Union(dependencies.Where(p => !_modInstallation.LocalModManager.IsPackageInstalled(p) && !ReferenceEquals(package1, p)));
                }
            }

            return result;
        }

        private async Task<IList<IPackage>> GetPackageDependencies()
        {
            // This loop will be terminated when a valid result has been found
            while (true)
            {
                var progressControllerTask = _interactionService.OpenProgressDialogAsync("Resolving depedencies", "");
                try
                {
                    // Do this in a background thread as it might take some time...
                    ModDependencyException exception;
                    try
                    {
                        var result = await Task.Run(async () => (await ResolveDependencies(progressControllerTask)).ToList()).ConfigureAwait(false);

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
                    await (await progressControllerTask).CloseAsync();
                }
            }
        }

        #region Implementation of IInstallationState

        public Task<StateResult> GetResult()
        {
            // Never completes
            return new TaskCompletionSource<StateResult>().Task;
        }

        private bool UninstallPackageSelector(PackageViewModel model)
        {
            return !model.Selected && _modInstallation.LocalModManager.IsPackageInstalled(model.Package);
        }

        #endregion
    }
}
