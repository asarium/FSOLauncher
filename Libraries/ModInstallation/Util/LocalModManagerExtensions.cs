#region Usings

using System.Linq;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;

#endregion

namespace ModInstallation.Util
{
    public static class LocalModManagerExtensions
    {
        public static bool IsPackageInstalled(this ILocalModManager This, IPackage package)
        {
            if (This.Modifications == null)
            {
                return false;
            }

            return
                This.Modifications.Where(mod => mod.Equals(package.ContainingModification))
                    .SelectMany(x => x.Packages)
                    .Any(x => x.Name == package.Name);
        }
    }
}
