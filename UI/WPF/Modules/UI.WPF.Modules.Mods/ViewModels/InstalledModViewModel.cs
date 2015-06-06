#region Usings

using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Akavache;
using FSOManagement.Annotations;
using ModInstallation.Implementations.Management;
using Splat;

#endregion

namespace UI.WPF.Modules.Mods.ViewModels
{
    internal class InstalledModViewModel : ModViewModel<InstalledModification>
    {
        public InstalledModViewModel([NotNull] IObservable<string> filterObservable, [NotNull] InstalledModification mod)
            : base(mod, filterObservable)
        {
        }

        protected override bool IsVisible(string filterString)
        {
            if (string.IsNullOrEmpty(filterString))
            {
                return true;
            }

            return ModInstance.Modification.Title.IndexOf(filterString, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }
        public override int CompareTo(ModViewModel<InstalledModification> other)
        {
            return string.Compare(ModInstance.Modification.Title, other.ModInstance.Modification.Title, StringComparison.CurrentCulture);
        }

        protected override async Task<IBitmap> LoadLogoAsync()
        {
            var uri = ModInstance.Modification.LogoUri;

            if (uri == null)
            {
                return null;
            }

            return await BlobCache.LocalMachine.LoadImageFromUrl(uri.ToString());
        }
    }
}
