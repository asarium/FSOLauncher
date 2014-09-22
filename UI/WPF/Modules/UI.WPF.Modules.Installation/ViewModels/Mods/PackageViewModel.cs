#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using ModInstallation.Annotations;
using ModInstallation.Interfaces;
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

        private string _operationMessage;

        private double _operationProgress;

        public PackageViewModel([NotNull] IPackage package, [NotNull] InstallationTabViewModel installationTabViewModel)
        {
            _installationTabViewModel = installationTabViewModel;
            Package = package;

            ProgressReporter = new Progress<IInstallationProgress>(ProgressHandler);

            this.WhenAnyValue(x => x.Selected).Subscribe(SelectedChanged);
        }

        [CanBeNull]
        public string OperationMessage
        {
            get { return _operationMessage; }
            private set { RaiseAndSetIfPropertyChanged(ref _operationMessage, value); }
        }

        public double OperationProgress
        {
            get { return _operationProgress; }
            private set { RaiseAndSetIfPropertyChanged(ref _operationProgress, value); }
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

        [NotNull]
        public IProgress<IInstallationProgress> ProgressReporter { get; private set; }

        private void ProgressHandler([NotNull] IInstallationProgress installationProgress)
        {
            OperationMessage = installationProgress.Message;
            OperationProgress = installationProgress.OverallProgress;
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
                {
                    packageViewModel.Selected = true;
                }
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
