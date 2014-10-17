using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using FSOManagement.Interfaces.Mod;
using ModInstallation.Interfaces;

namespace FSOManagement.Implementations.Mod
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
                return null;

            return null;
        }
    }
}