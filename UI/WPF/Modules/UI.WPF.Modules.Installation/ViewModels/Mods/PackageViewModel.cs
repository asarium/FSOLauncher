#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
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

        private string _operationMessage;

        private double _operationProgress;

        private bool _progressIdeterminate;

        private bool _selected;

        public PackageViewModel([NotNull] IPackage package, [NotNull] InstallationTabViewModel installationTabViewModel)
        {
            _installationTabViewModel = installationTabViewModel;
            Package = package;

            ProgressReporter = new Progress<IInstallationProgress>(ProgressHandler);

            this.WhenAnyValue(x => x.Selected).Subscribe(SelectedChanged);

            var cancelCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.Installing));
            cancelCommand.Subscribe(_ =>
            {
                if (TokenSource != null)
                {
                    TokenSource.Cancel();
                }
            });

            CancelCommand = cancelCommand;
        }

        public bool ProgressIdeterminate
        {
            get { return _progressIdeterminate; }
            private set { RaiseAndSetIfPropertyChanged(ref _progressIdeterminate, value); }
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
        public ICommand CancelCommand { get; private set; }

        [CanBeNull]
        public CancellationTokenSource TokenSource { get; private set; }

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

            if (installationProgress.OverallProgress < 0.01 && installationProgress.SubProgress < 0.0)
            {
                // If the operations is just starting and we get an indeterminate sub progress then reflect that in the UI
                ProgressIdeterminate = true;
            }
            else
            {
                ProgressIdeterminate = false;
            }
        }

        [NotNull]
        public async Task Install([NotNull] IPackageInstaller installer)
        {
            if (Installing)
            {
                return;
            }

            if (TokenSource != null)
            {
                TokenSource.Dispose();
            }

            TokenSource = new CancellationTokenSource();

            Installing = true;
            try
            {
                await installer.InstallPackageAsync(Package, ProgressReporter, TokenSource.Token);

                await _installationTabViewModel.LocalModManager.AddPackageAsync(Package);

                OperationMessage = null;
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                Installing = false;
            }
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

                if (packageViewModel != null && !ReferenceEquals(this, packageViewModel))
                {
                    packageViewModel.Selected = true;
                }
            }
        }

        [NotNull]
        private List<IModification> GetModList()
        {
            var modManager = _installationTabViewModel.RemoteModManager;
            if (modManager.Modifications != null && modManager.Modifications != null)
            {
                return modManager.Modifications.Concat(modManager.Modifications).ToList();
            }
            if (modManager.Modifications != null)
            {
                return modManager.Modifications.ToList();
            }

            return modManager.Modifications != null ? modManager.Modifications.ToList() : new List<IModification>();
        }
    }
}
