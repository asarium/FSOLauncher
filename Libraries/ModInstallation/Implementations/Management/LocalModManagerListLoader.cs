#region Usings

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

#endregion

namespace ModInstallation.Implementations.Management
{
    [Export(typeof(IModListLoader))]
    public class LocalModManagerListLoader : IModListLoader
    {
        [NotNull, Import]
        private ILocalModManager LocalModManager { get; set; }

        #region IModListLoader Members

        public async Task<IReadOnlyReactiveList<ILocalModification>> LoadModificationListAsync(string searchFolder)
        {
            var currentDirectory = LocalModManager.PackageDirectory;

            var newDirectory = Path.Combine(searchFolder, "mods");

            if (currentDirectory != null && Path.GetFullPath(currentDirectory) == Path.GetFullPath(newDirectory))
            {
                // No change necessary
                return LocalModManager.Modifications.CreateDerivedCollection(GetLocaModification);
            }

            LocalModManager.PackageDirectory = newDirectory;
            await LocalModManager.ParseLocalModDataAsync();

            return LocalModManager.Modifications.CreateDerivedCollection(GetLocaModification);
        }

        #endregion

        [NotNull]
        private ILocalModification GetLocaModification([NotNull] IInstalledModification mod)
        {
           return new InstalledModification(mod)
            {
                ModRootPath = mod.InstallPath,
                Dependencies = new ModificationDependencies(mod, LocalModManager)
            };
        }
    }
}
