#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using ModInstallation.Annotations;
using ModInstallation.Implementations;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
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

        private string _managerStatusMessage;

        private IEnumerable<ModViewModel> _modificationViewModels;

        [NotNull]
        public ICommand InstallModsCommand { get; private set; }

        [NotNull,Import]
        private IProfileManager ProfileManager { get; set; }

        [NotNull,Import]
        public IPackageInstaller PackageInstaller { get; private set; }

        [ImportingConstructor]
        public InstallationTabViewModel()
        {
            DisplayName = "Update/Install mods";

            ModManager = new DefaultModManager();
            ModManager.AddModRepository(new WebJsonRepository("Default", "http://dev.tproxy.de/fs2/all.json"));

            InstallModsCommand = ReactiveCommand.CreateAsyncTask(_ => InstallMods());

            this.WhenAny(x => x.ManagerStatusMessage, val => !string.IsNullOrEmpty(val.Value)).BindTo(this, x => x.HasManagerStatusMessage);
        }

        [NotNull]
        private async Task InstallMods()
        {
            if (ProfileManager.CurrentProfile == null)
                return;

            if (ProfileManager.CurrentProfile.SelectedTotalConversion == null)
                return;

            PackageInstaller.InstallationDirectory = Path.Combine(ProfileManager.CurrentProfile.SelectedTotalConversion.RootFolder, "mods");

            var packageViewModels = ModificationViewModels.SelectMany(mod => mod.Packages).Where(pack => pack.Selected);

            await Task.WhenAll(InstallAllMods(packageViewModels, CancellationToken.None));
        }

        [NotNull]
        private IEnumerable<Task> InstallAllMods([NotNull] IEnumerable<PackageViewModel> viewModels, CancellationToken token)
        {
            foreach (var packageViewModel in viewModels)
            {
                packageViewModel.Installing = true;

                yield return PackageInstaller.InstallPackageAsync(packageViewModel.Package, packageViewModel.ProgressReporter, token);
            }
        }

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
        public IModManager ModManager { get; private set; }

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
        private IInteractionService InteractionService { get; set; }

        private async void UpdateMods()
        {
            await ModManager.RetrieveInformationAsync(new Progress<string>(msg => ManagerStatusMessage = msg), CancellationToken.None);

            if (ModManager.RemoteModifications != null)
            {
                ModificationViewModels = ModManager.RemoteModifications.CreateDerivedCollection(mod => new ModViewModel(mod, this));
            }
            else
            {
                await InteractionService.ShowMessage(MessageType.Error, "Retrieval failed!", "The mod information download failed!");
            }

            ManagerStatusMessage = null;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            UpdateMods();
        }
    }
}
