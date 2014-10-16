using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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

        public async Task<IEnumerable<IModification>> LoadModificationListAsync(string searchFolder)
        {
            var currentDirectory = LocalModManager.PackageDirectory;

            return null;
        }
    }
}