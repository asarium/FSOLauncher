using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using FSOManagement.Implementations.Mod;
using FSOManagement.Interfaces.Mod;
using ModInstallation.Annotations;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using ReactiveUI;

namespace ModInstallation.Implementations
{
    [Export(typeof(IModListLoader))]
    public class LocalModManagerListLoader : IModListLoader
    {
        [NotNull, Import]
        private ILocalModManager LocalModManager { get; set; }

        public async Task<IEnumerable<ILocalModification>> LoadModificationListAsync(string searchFolder)
        {
            var currentDirectory = LocalModManager.PackageDirectory;

            var newDirectory = Path.Combine(searchFolder, "mods", "packages");

            if (Path.GetFullPath(currentDirectory) == Path.GetFullPath(newDirectory))
                // No change necessary
                return LocalModManager.Modifications.CreateDerivedCollection(GetLocaModification);

            return null;
        }

        [NotNull]
        private ILocalModification GetLocaModification([NotNull] IInstalledModification mod)
        {
        }
    }
}