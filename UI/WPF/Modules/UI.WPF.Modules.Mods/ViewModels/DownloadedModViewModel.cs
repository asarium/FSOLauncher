#region Usings

using System.Windows.Media.Imaging;
using FSOManagement.Annotations;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Util;
using Splat;
using UI.WPF.Launcher.Common.Classes;

#endregion

namespace UI.WPF.Modules.Mods.ViewModels
{
    public class DownloadedModViewModel : ReactiveObjectBase
    {
        private readonly IModification _modification;

        private BitmapSource _logoSource;

        public DownloadedModViewModel([NotNull] IModification modification)
        {
            _modification = modification;

            LoadLogo();
        }

        [NotNull]
        public IModification Modification
        {
            get { return _modification; }
        }

        [CanBeNull]
        public BitmapSource LogoSource
        {
            get { return _logoSource; }
            private set { RaiseAndSetIfPropertyChanged(ref _logoSource, value); }
        }

        private async void LoadLogo()
        {
            var source = await _modification.LoadLogoBitmap();

            LogoSource = source == null ? null : source.ToNative();
        }
    }
}
