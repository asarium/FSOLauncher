#region Usings

using System;
using System.Reactive.Linq;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Util;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels.Mods
{
    public class PackageViewModel : ReactiveObjectBase
    {
        private bool _isChangeable;

        private bool _selected;

        public PackageViewModel([NotNull] IPackage package, [NotNull] InstallationTabViewModel installationTabViewModel)
        {
            Package = package;

            IsSelectedObservable = this.WhenAnyValue(x => x.Selected).Select(b => new SelectedChangedData(this, b));

            installationTabViewModel.InteractionEnabledObservable.Select(b => b && package.Status != PackageStatus.Required)
                .BindTo(this, x => x.IsChangeable);

            Selected = installationTabViewModel.LocalModManager.IsPackageInstalled(package);
        }

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
    }
}
