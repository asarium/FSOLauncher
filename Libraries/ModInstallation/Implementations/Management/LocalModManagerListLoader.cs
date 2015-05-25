#region Usings

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
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
        private readonly ILocalModEnumerator _enumerator;

        private IFileSystem _fileSystem;

        [ImportingConstructor]
        public LocalModManagerListLoader(ILocalModEnumerator enumerator, IFileSystem fileSystem)
        {
            _enumerator = enumerator;
            _fileSystem = fileSystem;
        }

        #region IModListLoader Members

        public async Task<IEnumerable<ILocalModification>> LoadModificationListAsync(string searchFolder)
        {
            return (await _enumerator.FindMods(_fileSystem.Path.Combine(searchFolder, "mods"))).Select(GetLocaModification);
        }

        #endregion

        [NotNull]
        private ILocalModification GetLocaModification([NotNull] IInstalledModification mod)
        {
            return new InstalledModification(mod);
        }
    }
}
