#region Usings

using System;
using System.Reactive.Linq;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Util;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Modules.Installation.Interfaces;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels.Mods
{
    public class PackageViewModel : ReactiveObjectBase
    {
        private bool _isChangeable;

        private bool _selected;

        public PackageViewModel([NotNull] IPackage package, [NotNull] InstallationTabViewModel tabViewModel)
        {
            Package = package;

            IsSelectedObservable = this.WhenAnyValue(x => x.Selected).Select(b => new SelectedChangedData(this, b));

            IsChangeable = package.Status != PackageStatus.Required;

            Selected = tabViewModel.LocalModManager.IsPackageInstalled(package);
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
