#region Usings

using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Mods;
using Splat;

#endregion

namespace ModInstallation.Util
{
    public static class IModificationExtensions
    {
        [NotNull]
        public static async Task<IBitmap> LoadLogoBitmap([NotNull] this IModification mod)
        {
            if (mod.LogoUri == null)
            {
                return null;
            }

            return await BlobCache.LocalMachine.LoadImageFromUrl(mod.LogoUri.ToString());
        }
    }
}
