#region Usings

using System.Collections.Generic;
using System.Reactive.Linq;
using ModInstallation.Interfaces.Mods;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels.Dependencies
{
    public class DependencyModViewModel : ReactiveObjectBase
    {
        private string _versionString;

        public DependencyModViewModel(IModification mod, IEnumerable<DependencyPackageViewModel> packages)
        {
            Mod = mod;
            Packages = packages;

            mod.WhenAnyValue(x => x.Version).Select(x => x.ToString()).BindTo(this, x => x.VersionString);
        }

        public IModification Mod { get; private set; }

        public IEnumerable<DependencyPackageViewModel> Packages { get; private set; }

        public string VersionString
        {
            get { return _versionString; }
            private set { RaiseAndSetIfPropertyChanged(ref _versionString, value); }
        }
    }
}
