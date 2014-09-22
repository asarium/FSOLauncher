#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using Caliburn.Micro;
using ModInstallation.Annotations;
using ModInstallation.Implementations;
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
        private string _managerStatusMessage;

        private IEnumerable<ModViewModel> _modificationViewModels;

        [ImportingConstructor]
        public InstallationTabViewModel()
        {
            DisplayName = "Update/Install mods";

            ModManager = new DefaultModManager();
            ModManager.AddModRepository(new WebJsonRepository("Default", "http://dev.tproxy.de/fs2/all.json"));

            this.WhenAny(x => x.ManagerStatusMessage, val => !string.IsNullOrEmpty(val.Value)).BindTo(this, x => x.HasManagerStatusMessage);
        }

        public bool HasManagerStatusMessage { get; private set; }

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

        private async void UpdateMods([NotNull] IInteractionService interactionService)
        {
            await ModManager.RetrieveInformationAsync(new Progress<string>(msg => ManagerStatusMessage = msg), CancellationToken.None);

            if (ModManager.RemoteModifications != null)
            {
                ModificationViewModels = ModManager.RemoteModifications.CreateDerivedCollection(mod => new ModViewModel(mod));
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

            UpdateMods(InteractionService);
        }
    }
}
