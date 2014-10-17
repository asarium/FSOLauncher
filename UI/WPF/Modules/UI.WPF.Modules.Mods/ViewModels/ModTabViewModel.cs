#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using Caliburn.Micro;
using FSOManagement.Annotations;
using FSOManagement.Implementations.Mod;
using FSOManagement.Interfaces.Mod;
using ReactiveUI;
using UI.WPF.Launcher.Common.Interfaces;

#endregion

namespace UI.WPF.Modules.Mods.ViewModels
{
    [Export(typeof(ILauncherTab)), ExportMetadata("Priority", 1)]
    public sealed class ModTabViewModel : Screen, ILauncherTab
    {
        private readonly IProfileManager _profileManager;

        private ICollectionView _downloadedModificationsView;

        private string _filterString;

        private ICollectionView _standardModificationsView;

        [ImportingConstructor]
        public ModTabViewModel([NotNull] IProfileManager profileManager)
        {
            _profileManager = profileManager;
            DisplayName = "Mods";

            InitializeLoadMods(profileManager);

            InitializeStandardMods(profileManager);

            InitializeDownloadedMods(profileManager);

            this.WhenAnyValue(x => x.FilterString).Subscribe(_ =>
            {
                if (StandardModificationsView != null)
                {
                    StandardModificationsView.Refresh();
                }

                if (DownloadedModificationsView != null)
                {
                    DownloadedModificationsView.Refresh();
                }
            });

            profileManager.WhenAnyValue(x => x.CurrentProfile.ModActivationManager.PrimaryModifications).Subscribe(OnPrimaryModificationsChanges);

            profileManager.WhenAnyValue(x => x.CurrentProfile.ModActivationManager.ActiveMod).Subscribe(OnActiveModChanged);

            profileManager.WhenAnyValue(x => x.CurrentProfile.ModActivationManager.SecondaryModifications).Subscribe(OnSecondaryModificationsChanges);
        }

        [CanBeNull]
        public ICollectionView StandardModificationsView
        {
            get { return _standardModificationsView; }
            private set
            {
                if (Equals(value, _standardModificationsView))
                {
                    return;
                }
                _standardModificationsView = value;
                NotifyOfPropertyChange();
            }
        }

        [CanBeNull]
        public ICollectionView DownloadedModificationsView
        {
            get { return _downloadedModificationsView; }
            private set
            {
                if (Equals(value, _downloadedModificationsView))
                {
                    return;
                }
                _downloadedModificationsView = value;
                NotifyOfPropertyChange();
            }
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

            OnPrimaryModificationsChanges(_profileManager.CurrentProfile.ModActivationManager.PrimaryModifications);

            OnSecondaryModificationsChanges(_profileManager.CurrentProfile.ModActivationManager.SecondaryModifications);
        }

        private void InitializeDownloadedMods([NotNull] IProfileManager profileManager)
        {
        }

        private void InitializeStandardMods([NotNull] IProfileManager profileManager)
        {
            profileManager.WhenAny(x => x.CurrentProfile.SelectedTotalConversion.ModManager.Modifications,
                val => CreateStandardModificationsView(val.Value)).BindTo(this, x => x.StandardModificationsView);
        }

        private void OnActiveModChanged([CanBeNull] ILocalModification activeMod)
        {
            if (StandardModificationsView == null)
            {
                return;
            }

            if (activeMod == null)
            {
                // If the active mod is null, activate the first mod and deactivate the others.
                var first = StandardModificationsView.Cast<ModViewModel>().FirstOrDefault();

                if (first != null)
                {
                    first.IsActiveMod = true;
                }

                StandardModificationsView.Cast<ModViewModel>().Skip(1).Apply(mod => mod.IsActiveMod = false);

                return;
            }

            StandardModificationsView.Cast<ModViewModel>().Apply(view => view.IsActiveMod = view.Mod.ModRootPath == activeMod.ModRootPath);
        }

        private void OnPrimaryModificationsChanges([CanBeNull] IEnumerable<ILocalModification> modifications)
        {
            if (StandardModificationsView == null)
            {
                return;
            }

            if (modifications == null)
            {
                return;
            }

            StandardModificationsView.Cast<ModViewModel>()
                .Apply(view => view.IsPrimaryMod = modifications.Any(mod => mod.ModRootPath == view.Mod.ModRootPath));
        }

        private void OnSecondaryModificationsChanges([CanBeNull] IEnumerable<ILocalModification> modifications)
        {
            if (StandardModificationsView == null)
            {
                return;
            }

            if (modifications == null)
            {
                return;
            }

            StandardModificationsView.Cast<ModViewModel>()
                .Apply(view => view.IsSecondaryMod = modifications.Any(mod => mod.ModRootPath == view.Mod.ModRootPath));
        }

        private bool FilterModification([NotNull] object item)
        {
            var modViewModel = item as ModViewModel;

            if (modViewModel == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(FilterString))
            {
                return true;
            }

            if (modViewModel.DisplayName.IndexOf(FilterString, StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }

        [NotNull]
        private ICollectionView CreateStandardModificationsView([NotNull] IEnumerable<ILocalModification> value)
        {
            var viewModelCollection = value.CreateDerivedCollection(CreateModViewModel, mod => mod is IniModification);

            var collectionView = CollectionViewSource.GetDefaultView(viewModelCollection);

            if (collectionView.CanFilter)
            {
                collectionView.Filter = FilterModification;
            }

            return collectionView;
        }

        private void ViewModelsChanged()
        {
        }

        [CanBeNull]
        private static ModViewModel CreateModViewModel([NotNull] ILocalModification mod)
        {
            var modification = (IniModification) mod;

            var modViewModel = new ModViewModel(modification);

            return modViewModel;
        }
    }
}
