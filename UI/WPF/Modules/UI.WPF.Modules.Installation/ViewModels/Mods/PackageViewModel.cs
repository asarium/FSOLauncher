#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Mods;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels.Mods
{
    public class PackageViewModel : ReactiveObjectBase
    {
        private readonly InstallationTabViewModel _installationTabViewModel;

        private bool _installing;

        private bool _selected;

        public PackageViewModel([NotNull] IPackage package, [NotNull] InstallationTabViewModel installationTabViewModel)
        {
            _installationTabViewModel = installationTabViewModel;
            Package = package;

            this.WhenAnyValue(x => x.Selected).Subscribe(SelectedChanged);
        }

        [NotNull]
        public IPackage Package { get; private set; }

        public bool Installing
        {
            get { return _installing; }
            set { RaiseAndSetIfPropertyChanged(ref _installing, value); }
        }

        public bool Selected
        {
            get { return _selected; }
            set { RaiseAndSetIfPropertyChanged(ref _selected, value); }
        }

        private void SelectedChanged(bool selected)
        {
            if (!selected)
            {
                return;
            }

            var modList = GetModList();

            var dependencies = _installationTabViewModel.DependencyResolver.ResolveDependencies(Package, modList, null);

            var packageViewModels = _installationTabViewModel.ModificationViewModels.SelectMany(mod => mod.Packages);
            var packageViewModelList = packageViewModels as IList<PackageViewModel> ?? packageViewModels.ToList();

            foreach (var dependency in dependencies)
            {
                var packageViewModel = packageViewModelList.FirstOrDefault(p => Equals(p.Package, dependency));

                if (packageViewModel != null && ! ReferenceEquals(this, packageViewModel))
                    packageViewModel.Selected = true;
            }
        }

        [NotNull]
        private List<IModification> GetModList()
        {
            var modManager = _installationTabViewModel.ModManager;
            if (modManager.LocalModifications != null && modManager.RemoteModifications != null)
            {
                return modManager.LocalModifications.Concat(modManager.RemoteModifications).ToList();
            }
            if (modManager.RemoteModifications != null)
            {
                return modManager.RemoteModifications.ToList();
            }

            return modManager.LocalModifications != null ? modManager.LocalModifications.ToList() : new List<IModification>();
        }
    }
}
