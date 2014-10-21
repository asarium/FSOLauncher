#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Caliburn.Micro;
using FSOManagement.Annotations;
using FSOManagement.Interfaces.Mod;
using ReactiveUI;
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

        [ImportingConstructor]
        public ModTabViewModel([NotNull] IProfileManager profileManager)
        {
            _profileManager = profileManager;
            DisplayName = "Mods";

            InitializeLoadMods(profileManager);

            InitializeModLists(profileManager);

            _filterObservable = this.WhenAnyValue(x => x.FilterString);

            profileManager.WhenAnyValue(x => x.CurrentProfile.ModActivationManager.PrimaryModifications).Subscribe(OnPrimaryModificationsChanged);

            profileManager.WhenAnyValue(x => x.CurrentProfile.ModActivationManager.ActiveMod).Subscribe(OnActiveModChanged);

            profileManager.WhenAnyValue(x => x.CurrentProfile.ModActivationManager.SecondaryModifications).Subscribe(OnSecondaryModificationsChanged);
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

        [CanBeNull]
        public IReadOnlyReactiveList<ModListViewModel> ModLists
        {
            get { return _modLists; }
            private set
            {
                if (Equals(value, _modLists))
                {
                    return;
                }
                _modLists = value;
                NotifyOfPropertyChange();
            }
        }

        private void InitializeLoadMods([NotNull] IProfileManager profileManager)
        {
            profileManager.WhenAnyValue(x => x.CurrentProfile.SelectedTotalConversion.ModManager).Subscribe(LoadMods);
        }

        private async void LoadMods([NotNull] IModManager manager)
        {
            await manager.RefreshModsAsync();

            UpdateViewModelStatus();
        }

        private void UpdateViewModelStatus()
        {
            if (_profileManager.CurrentProfile == null)
            {
                return;
            }

            OnActiveModChanged(_profileManager.CurrentProfile.ModActivationManager.ActiveMod);

            OnPrimaryModificationsChanged(_profileManager.CurrentProfile.ModActivationManager.PrimaryModifications);

            OnSecondaryModificationsChanged(_profileManager.CurrentProfile.ModActivationManager.SecondaryModifications);
        }

        private void InitializeModLists([NotNull] IProfileManager profileManager)
        {
            profileManager.WhenAny(x => x.CurrentProfile.SelectedTotalConversion.ModManager.ModificationLists, val => CreateModListsView(val.Value))
                .BindTo(this, x => x.ModLists);
        }

        [NotNull]
        private IReadOnlyReactiveList<ModListViewModel> CreateModListsView(
            [NotNull] IEnumerable<IReadOnlyReactiveList<ILocalModification>> value)
        {
            var viewModels = value.CreateDerivedCollection(mods => new ModListViewModel(mods, _filterObservable));
            
            // This feels like a hack but I don't know how to do it better...
            var resetSubject = new Subject<bool>();
            viewModels.CreateDerivedCollection(x => x.HasModsObservable.ObserveOn(RxApp.MainThreadScheduler).Subscribe(resetSubject.OnNext));

            return viewModels.CreateDerivedCollection(x => x, x => x.ModViewModels.Any(), null, resetSubject);
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
