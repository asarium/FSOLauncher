#region Usings

using System;
using System.Collections.Generic;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Mods;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels.Mods
{
    public class ModViewModel : ReactiveObjectBase
    {
        private bool _hasDescription;

        private IModification _mod;

        private bool _modChecked;

        private IEnumerable<PackageViewModel> _packages;

        public ModViewModel([NotNull] IModification mod, [NotNull] InstallationTabViewModel installationTabViewModel)
        {
            _mod = mod;

            Packages = mod.Packages.CreateDerivedCollection(p => new PackageViewModel(p, installationTabViewModel));

            mod.WhenAny(x => x.Description, val => !string.IsNullOrEmpty(val.Value)).BindTo(this, x => x.HasDescription);

            this.WhenAnyValue(x => x.ModChecked).Subscribe(selected =>
            {
                if (!selected)
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
                        packageViewModel.Selected = packageViewModel.Package.Status == PackageStatus.Required ||
                                                    packageViewModel.Package.Status == PackageStatus.Recommended;
                    }
                }
            });
        }

        public bool ModChecked
        {
            get { return _modChecked; }
            set { RaiseAndSetIfPropertyChanged(ref _modChecked, value); }
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
    }
}
