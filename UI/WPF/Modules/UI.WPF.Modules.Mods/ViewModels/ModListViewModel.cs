﻿#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using FSOManagement.Annotations;
using FSOManagement.Implementations.Mod;
using FSOManagement.Interfaces.Mod;
using ModInstallation.Implementations.Management;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;

#endregion

namespace UI.WPF.Modules.Mods.ViewModels
{
    public class ModListViewModel : ReactiveObjectBase
    {
        private readonly IReadOnlyReactiveList<ModViewModel> _filteredViewModels;

        private string _displayString;

        public ModListViewModel([NotNull] IReactiveCollection<ILocalModification> mods, [NotNull] IObservable<string> filterObservable)
        {
            DisplayString = mods.Any() ? GetDisplayString(mods.First()) : "";

            var viewModels = mods.CreateDerivedCollection(mod => GetViewModel(mod, filterObservable));

            _filteredViewModels = viewModels.CreateDerivedCollection(model => model, model => model.Visible, null, filterObservable);

            HasModsObservable = _filteredViewModels.CountChanged.Select(c => c > 0);

            // When this changes we might need to update our display string
            HasModsObservable.Subscribe(_ => DisplayString = mods.Any() ? GetDisplayString(mods.First()) : "");
        }

        [NotNull]
        public IObservable<bool> HasModsObservable { get; private set; }

        [NotNull]
        public string DisplayString
        {
            get { return _displayString; }
            private set { RaiseAndSetIfPropertyChanged(ref _displayString, value); }
        }

        [NotNull]
        public IEnumerable<ModViewModel> ModViewModels
        {
            get { return _filteredViewModels; }
        }

        [NotNull]
        private static ModViewModel GetViewModel([NotNull] ILocalModification mod, [NotNull] IObservable<string> filterObservable)
        {
            if (mod is IniModification)
            {
                return new IniModViewModel(filterObservable, mod as IniModification);
            }

            if (mod is InstalledModification)
            {
                return new InstalledModViewModel(filterObservable, mod as InstalledModification);
            }

            throw new ArgumentOutOfRangeException("mod");
        }

        [NotNull]
        private static string GetDisplayString([NotNull] ILocalModification testMod)
        {
            if (testMod is IniModification)
            {
                return "Installed";
            }

            if (testMod is InstalledModification)
            {
                return "Downloaded";
            }

            throw new ArgumentOutOfRangeException(testMod.GetType().FullName);
        }

        public void OnActiveModChanged([CanBeNull] ILocalModification activeMod)
        {
            if (activeMod == null)
            {
                foreach (var modViewModel in ModViewModels)
                {
                    modViewModel.IsActiveMod = false;
                }

                return;
            }


            foreach (var modViewModel in ModViewModels)
            {
                modViewModel.IsActiveMod = Equals(modViewModel.Mod, activeMod);
            }
        }

        public void OnPrimaryModificationsChanged([CanBeNull] IList<ILocalModification> localModifications)
        {
//            throw new NotImplementedException();
        }

        public void OnSecondaryModificationsChanged([CanBeNull] IList<ILocalModification> localModifications)
        {
//            throw new NotImplementedException();
        }
    }
}