#region Usings

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
        private IModification _mod;

        private bool _hasDescription;

        private IEnumerable<PackageViewModel> _packages;

        public ModViewModel([NotNull] IModification mod, [NotNull] InstallationTabViewModel installationTabViewModel)
        {
            _mod = mod;

            mod.WhenAny(x => x.Description, val => !string.IsNullOrEmpty(val.Value)).BindTo(this, x => x.HasDescription);

            Packages = mod.Packages.CreateDerivedCollection(p => new PackageViewModel(p, installationTabViewModel));
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
