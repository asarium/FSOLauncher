using System.Collections.Generic;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Mods;
using ReactiveUI;

namespace ModInstallation.Interfaces
{
    public interface ILocalModManager
    {
        [CanBeNull]
        IEnumerable<IInstalledModification> Modifications { get; }

        [CanBeNull]
        string PackageDirectory { get; set; }

        [NotNull]
        Task AddPackageAsync([NotNull] IPackage package);

        [NotNull]
        Task ParseLocalModDataAsync();

        Task RemovePackageAsync(IPackage package);
    }
}