#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using FSOManagement.Annotations;
using FSOManagement.Interfaces.Mod;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Launcher.Common.Interfaces;

#endregion

namespace UI.WPF.Modules.Mods.ViewModels
{
    [Export(typeof(ILauncherTab)), ExportMetadata("Priority", 0)]
    public sealed class ModTabViewModel : Screen, ILauncherTab
    {
        private readonly IObservable<string> _filterObservable;

        private readonly IProfileManager _profileManager;

        private string _filterString;

        private IReadOnlyReactiveList<ModListViewModel> _modLists;

        private ModManagerViewModel _modManagerVm;

        [ImportingConstructor]
        public ModTabViewModel([NotNull] IProfileManager profileManager, IMessageBus messageBus)
        {
            _profileManager = profileManager;
            DisplayName = "Mods";

            InitializeLoadMods(profileManager, messageBus);

            _filterObservable = this.WhenAnyValue(x => x.FilterString);

            profileManager.WhenAnyValue(x => x.CurrentProfile.ModActivationManager.ActiveMod).Subscribe(OnActiveModChanged);

            profileManager.WhenAnyValue(x => x.CurrentProfile.ModActivationManager.ActivatedMods).Subscribe(OnActivatedModsChanged);
        }

        [CanBeNull]
        public string FilterString
        {
            get { return _filterString; }
            set
            {
                if (value == _filterString)
                {
                    return;
                }
                _filterString = value;
                NotifyOfPropertyChange();
            }
        }

        public ModManagerViewModel ModManagerVm
        {
            get { return _modManagerVm; }
            private set
            {
                if (Equals(value, _modManagerVm))
                {
                    return;
                }
                _modManagerVm = value;
                NotifyOfPropertyChange();
            }
        }

        private void InitializeLoadMods([NotNull] IProfileManager profileManager, IMessageBus bus)
        {
            profileManager.WhenAnyValue(x => x.CurrentProfile.SelectedTotalConversion.ModManager)
                .Select(x => new ModManagerViewModel(x, _filterObservable, bus))
                .BindTo(this, x => x.ModManagerVm);

            this.WhenAnyObservable(x => x.ModManagerVm.RefreshModsCommand).Subscribe(_ => UpdateViewModelStatus());
        }

        private void UpdateViewModelStatus()
        {
            if (_profileManager.CurrentProfile == null)
            {
                return;
            }

            OnActiveModChanged(_profileManager.CurrentProfile.ModActivationManager.ActiveMod);

            OnActivatedModsChanged(_profileManager.CurrentProfile.ModActivationManager.ActivatedMods);
        }

        private void OnActiveModChanged([CanBeNull] ILocalModification activeMod)
        {
            if (_modLists == null)
            {
                return;
            }

            foreach (var modListViewModel in _modLists)
            {
                modListViewModel.OnActiveModChanged(activeMod);
            }
        }

        private void OnActivatedModsChanged([CanBeNull] IEnumerable<ILocalModification> modifications)
        {
            if (_modLists == null)
            {
                return;
            }
            if (modifications == null)
            {
                return;
            }

            var localModifications = modifications as IList<ILocalModification> ?? modifications.ToList();
            foreach (var modListViewModel in _modLists)
            {
                modListViewModel.OnSecondaryModificationsChanged(localModifications);
            }
        }
        private void OnPrimaryModificationsChanged([CanBeNull] IEnumerable<ILocalModification> modifications)
        {
            if (_modLists == null)
            {
                return;
            }
            if (modifications == null)
            {
                return;
            }

            var localModifications = modifications as IList<ILocalModification> ?? modifications.ToList();
            foreach (var modListViewModel in _modLists)
            {
                modListViewModel.OnPrimaryModificationsChanged(localModifications);
            }
        }

        private void OnSecondaryModificationsChanged([CanBeNull] IEnumerable<ILocalModification> modifications)
        {
            if (_modLists == null)
            {
                return;
            }
            if (modifications == null)
            {
                return;
            }

            var localModifications = modifications as IList<ILocalModification> ?? modifications.ToList();
            foreach (var modListViewModel in _modLists)
            {
                modListViewModel.OnSecondaryModificationsChanged(localModifications);
            }
        }
    }
}
