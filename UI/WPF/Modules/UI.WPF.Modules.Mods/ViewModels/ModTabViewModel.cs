#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using Caliburn.Micro;
using FSOManagement;
using ReactiveUI;
using UI.WPF.Launcher.Common.Interfaces;

#endregion

namespace UI.WPF.Modules.Mods.ViewModels
{
    [Export(typeof(ILauncherTab)), ExportMetadata("Priority", 1)]
    public sealed class ModTabViewModel : Screen, ILauncherTab
    {
        private string _filterString;

        private ICollectionView _modificationsView;

        private CancellationTokenSource _readInisCancellationTokenSource;

        private Task _readModInisTask;

        [ImportingConstructor]
        public ModTabViewModel(IProfileManager profileManager)
        {
            DisplayName = "Mods";

            profileManager.WhenAny(x => x.CurrentProfile.SelectedTotalConversion.ModManager.Modifications, val => CreateModificationsView(val.Value))
                .BindTo(this, x => x.ModificationsView);

            profileManager.WhenAnyValue(x => x.CurrentProfile.ModActivationManager.PrimaryModifications).Subscribe(OnPrimaryModificationsChanges);

            profileManager.WhenAnyValue(x => x.CurrentProfile.ModActivationManager.ActiveMod).Subscribe(OnActiveModChanged);

            profileManager.WhenAnyValue(x => x.CurrentProfile.ModActivationManager.SecondaryModifications).Subscribe(OnSecondaryModificationsChanges);
        }

        public ICollectionView ModificationsView
        {
            get { return _modificationsView; }
            private set
            {
                if (Equals(value, _modificationsView))
                {
                    return;
                }
                _modificationsView = value;
                NotifyOfPropertyChange();

                if (_readModInisTask != null)
                {
                    // Canel the previous task
                    _readInisCancellationTokenSource.Cancel();

                    _readInisCancellationTokenSource = null;
                    _readModInisTask = null;
                }

                _readInisCancellationTokenSource = new CancellationTokenSource();
                _readModInisTask =
                    Task.WhenAll(_modificationsView.Cast<ModViewModel>()
                        .Select(model => model.ReadModIniAsync(_readInisCancellationTokenSource.Token)));
            }
        }

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

                _modificationsView.Refresh();
            }
        }

        private void OnActiveModChanged(Modification activeMod)
        {
            if (ModificationsView == null)
            {
                return;
            }

            if (activeMod == null)
            {
                // If the active mod is null, activate the first mod and deactivate the others.
                var first = ModificationsView.Cast<ModViewModel>().FirstOrDefault();

                if (first != null)
                {
                    first.IsActiveMod = true;
                }

                ModificationsView.Cast<ModViewModel>().Skip(1).Apply(mod => mod.IsActiveMod = false);

                return;
            }

            ModificationsView.Cast<ModViewModel>().Apply(view => view.IsActiveMod = view.Mod == activeMod);
        }

        private void OnPrimaryModificationsChanges(IEnumerable<Modification> modifications)
        {
            if (ModificationsView == null)
            {
                return;
            }

            if (modifications == null)
            {
                return;
            }

            ModificationsView.Cast<ModViewModel>().Apply(view => view.IsPrimaryMod = modifications.Any(mod => mod == view.Mod));
        }

        private void OnSecondaryModificationsChanges(IEnumerable<Modification> modifications)
        {
            if (ModificationsView == null)
            {
                return;
            }

            if (modifications == null)
            {
                return;
            }

            ModificationsView.Cast<ModViewModel>().Apply(view => view.IsSecondaryMod = modifications.Any(mod => mod == view.Mod));
        }

        private bool FilterModification(object item)
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

        private ICollectionView CreateModificationsView(IEnumerable<Modification> value)
        {
            var viewModelCollection = value.CreateDerivedCollection(CreateModViewModel);

            var collectionView = CollectionViewSource.GetDefaultView(viewModelCollection);

            if (collectionView.CanFilter)
            {
                collectionView.Filter = FilterModification;
            }

            return collectionView;
        }

        private static ModViewModel CreateModViewModel(Modification mod)
        {
            var modViewModel = new ModViewModel(mod);

            IoC.BuildUp(modViewModel);

            return modViewModel;
        }
    }
}
