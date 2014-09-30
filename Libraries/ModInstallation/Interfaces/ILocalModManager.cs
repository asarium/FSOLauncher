using System.Collections.Generic;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Mods;

namespace ModInstallation.Interfaces
{
    public interface ILocalModManager
    {
        [CanBeNull]
        IEnumerable<IModification> Modifications { get; }

        [NotNull]
        string PackageDirectory { get; set; }

        [NotNull]
        Task AddPackageAsync([NotNull] IPackage package);

        [NotNull]
        Task ParseLocalModDataAsync();
    }
}