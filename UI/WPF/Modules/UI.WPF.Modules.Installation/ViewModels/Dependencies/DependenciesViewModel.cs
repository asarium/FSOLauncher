using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ModInstallation.Interfaces.Mods;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;

namespace UI.WPF.Modules.Installation.ViewModels.Dependencies
{
    public class DependenciesViewModel : ReactiveObjectBase
    {
        private IList<DependencyModViewModel> _modViewModels;

        public DependenciesViewModel(IEnumerable<IPackage> dependencies)
        {
            var modGroups = dependencies.GroupBy(x => x.ContainingModification);

            ModViewModels =
                modGroups.Select(group => new DependencyModViewModel(group.Key, group.Select(x => new DependencyPackageViewModel(x)).ToList())).ToList();
        }

        public IList<DependencyModViewModel> ModViewModels
        {
            get { return _modViewModels; }
            private set { RaiseAndSetIfPropertyChanged(ref _modViewModels, value); }
        }
    }
}