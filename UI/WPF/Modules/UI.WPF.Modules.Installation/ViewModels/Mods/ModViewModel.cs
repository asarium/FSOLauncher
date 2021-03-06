﻿#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using FSOManagement.Annotations;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Modules.Installation.Interfaces;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels.Mods
{
    public class ModViewModel : ReactiveObjectBase
    {
        private bool _hasDescription;

        private IModification _mod;

        private bool? _modSelected;

        private IEnumerable<PackageViewModel> _packages;

        public ModViewModel([NotNull] IModification mod, [NotNull] IModInstallationManager modInstallationManager)
        {
            _mod = mod;

            Packages = mod.Packages.CreateDerivedCollection(p => new PackageViewModel(p, modInstallationManager));
            Packages.Select(x => x.IsSelectedObservable).CombineLatest().Select(IsSelectedTransform).BindTo(this, x => x.ModSelected);

            mod.WhenAny(x => x.Description, val => !string.IsNullOrEmpty(val.Value)).BindTo(this, x => x.HasDescription);

            this.WhenAnyValue(x => x.ModSelected).Subscribe(selected =>
            {
                if (selected == null)
                {
                    return;
                }

                if (!selected.Value)
                {
                    foreach (var packageViewModel in Packages)
                    {
                        packageViewModel.Selected = false;
                    }
                }
                else
                {
                    foreach (var packageViewModel in Packages)
                    {
                        if (packageViewModel.Package.Status == PackageStatus.Required || packageViewModel.Package.Status == PackageStatus.Recommended)
                        {
                            packageViewModel.Selected = true;
                        }
                    }
                }
            });
        }

        public bool? ModSelected
        {
            get { return _modSelected; }
            set { RaiseAndSetIfPropertyChanged(ref _modSelected, value); }
        }

        public bool HasDescription
        {
            get { return _hasDescription; }
            private set { RaiseAndSetIfPropertyChanged(ref _hasDescription, value); }
        }

        [NotNull]
        public IModification Mod
        {
            get { return _mod; }
            private set { RaiseAndSetIfPropertyChanged(ref _mod, value); }
        }

        [NotNull]
        public IEnumerable<PackageViewModel> Packages
        {
            get { return _packages; }
            private set { RaiseAndSetIfPropertyChanged(ref _packages, value); }
        }

        private static bool? IsSelectedTransform(IList<SelectedChangedData> arg)
        {
            if (arg.All(d => d.Selected || d.ViewModel.Package.Status == PackageStatus.Optional))
            {
                return true;
            }

            if (!arg.Any(d => d.Selected))
            {
                return false;
            }

            return null;
        }
    }
}
