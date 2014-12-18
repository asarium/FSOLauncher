#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ModInstallation.Annotations;
using ModInstallation.Implementations;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels.Mods
{
    public class PackageViewModel : ReactiveObjectBase
    {
        private bool _isChangeable;

        private bool _selected;

        private readonly InstallationTabViewModel _installationTabViewModel;

        public PackageViewModel([NotNull] IPackage package, [NotNull] InstallationTabViewModel installationTabViewModel)
        {
            _installationTabViewModel = installationTabViewModel;
            Package = package;

            IsSelectedObservable = this.WhenAnyValue(x => x.Selected).Select(b => new SelectedChangedData(this, b));

            SelectDependenciesCommand = ReactiveCommand.CreateAsyncTask(_ => HandleSelectedChanged());

            IsSelectedObservable.InvokeCommand(SelectDependenciesCommand);

            installationTabViewModel.InteractionEnabledObservable.Select(b => b && package.Status != PackageStatus.Required)
                .BindTo(this, x => x.IsChangeable);
        }

        [NotNull]
        private ReactiveCommand<Unit> SelectDependenciesCommand { get; set; }

        [NotNull]
        public IObservable<SelectedChangedData> IsSelectedObservable { get; private set; }
        
        [NotNull]
        public IPackage Package { get; private set; }


        public bool Selected
        {
            get { return _selected; }
            set { RaiseAndSetIfPropertyChanged(ref _selected, value); }
        }

        public bool IsChangeable
        {
            get { return _isChangeable; }
            private set { RaiseAndSetIfPropertyChanged(ref _isChangeable, value); }
        }
        
        [NotNull]
        private async Task HandleSelectedChanged()
        {
            if (!Selected)
            {
                return;
            }

            var modList = GetModList();

            bool failed;

            var packageViewModels = _installationTabViewModel.ModificationViewModels.SelectMany(mod => mod.Packages);
            var packageViewModelList = packageViewModels as IList<PackageViewModel> ?? packageViewModels.ToList();

            try
            {
                var dependencies = _installationTabViewModel.DependencyResolver.ResolveDependencies(Package, modList, null);

                foreach (var dependency in dependencies)
                {
                    var packageViewModel = packageViewModelList.FirstOrDefault(p => Equals(p.Package, dependency));

                    if (packageViewModel != null && !ReferenceEquals(this, packageViewModel))
                    {
                        packageViewModel.Selected = true;
                    }
                }

                failed = false;
            }
            catch (InvalidOperationException)
            {
                // await can't be used in a catch block (at least not in this version of C#)
                failed = true;
            }

            if (!failed)
            {
                return;
            }

            var result =
                await
                    UserError.Throw(new UserError("Failed to resolve dependencies",
                        "The installer was unable to resolve the dependencies of the selected package.\n" +
                        "Do you want to continue with your current selection?\n" + "(This will probably cause issues later)",
                        new[]
                        {
                            new RecoveryCommand("Continue", _ => RecoveryOptionResult.RetryOperation)
                            {
                                IsDefault = true
                            },
                            new RecoveryCommand("Abort", _ => RecoveryOptionResult.CancelOperation)
                        }));

            switch (result)
            {
                case RecoveryOptionResult.RetryOperation:
                    break;
                default:
                    Selected = false;
                    break;
            }
        }

        [NotNull]
        private IEnumerable<IModification> GetModList()
        {
            var modManager = _installationTabViewModel.RemoteModManager;

            return modManager.Modifications != null ? modManager.Modifications.ToList() : new List<IModification>();
        }
    }
}
