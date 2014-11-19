﻿#region Usings

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
using UI.WPF.Modules.Installation.ViewModels.Mods;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels
{
    [Export(typeof(ILauncherTab)), ExportMetadata("Priority", 3)]
    public sealed class InstallationTabViewModel : Screen, ILauncherTab
    {
        private bool _hasManagerStatusMessage;

        private string _managerStatusMessage;

        private IEnumerable<ModViewModel> _modificationViewModels;

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

            var updateModsCommand = ReactiveCommand.CreateAsyncTask(_ => UpdateMods());
        }

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
        public IEnumerable<ModViewModel> ModificationViewModels
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

        [NotNull, Import]
        public IInteractionService InteractionService { get; private set; }

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

            var packageViewModels = ModificationViewModels.SelectMany(mod => mod.Packages).Where(pack => pack.Selected);

            var installTasks = packageViewModels.Select(packageViewModel => packageViewModel.Install(PackageInstaller));

            await Task.WhenAll(installTasks);
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
