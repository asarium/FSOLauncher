#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Implementations.DataClasses;
using ModInstallation.Implementations.Mods;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using Newtonsoft.Json;
using Splat;

#endregion

namespace ModInstallation.Implementations
{
    [Export(typeof(ILocalModEnumerator))]
    public class LocalModEnumerator : ILocalModEnumerator, IEnableLogger
    {
        private readonly IFileSystem _fileSystem;

        [ImportingConstructor]
        public LocalModEnumerator(IFileSystem fFileSystem)
        {
            _fileSystem = fFileSystem;
        }

        #region Implementation of ILocalModEnumerator

        public async Task<IEnumerable<IInstalledModification>> FindMods(string searchPath)
        {
            if (searchPath == null)
            {
                return Enumerable.Empty<IInstalledModification>();
            }

            if (!_fileSystem.Directory.Exists(searchPath))
            {
                return Enumerable.Empty<IInstalledModification>();
            }
            var list = new List<IInstalledModification>();
            foreach (var modDir in _fileSystem.Directory.EnumerateDirectories(searchPath))
            {
                foreach (var versionDir in _fileSystem.Directory.EnumerateDirectories(modDir))
                {
                    var modFile = _fileSystem.Path.Combine(versionDir, LocalModManager.ModInfoFile);

                    if (!_fileSystem.File.Exists(modFile))
                    {
                        continue;
                    }

                    var installedModification = await ProcessModFile(modFile).ConfigureAwait(false);

                    if (installedModification != null)
                    {
                        list.Add(installedModification);
                    }
                }
            }

            return list;
        }

        [NotNull]
        private async Task<IInstalledModification> ProcessModFile([NotNull] string modFile)
        {
            string content;
            try
            {
                using (var stream = _fileSystem.File.Open(modFile, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        content = await reader.ReadToEndAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (IOException e)
            {
                this.Log().ErrorException("Failed to load " + modFile + " content!", e);
                return null;
            }

            Modification mod;
            try
            {
                mod = await Task.Run(() => JsonConvert.DeserializeObject<Modification>(content)).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.Log().ErrorException(modFile + " parsing failed!", e);
                return null;
            }

            var modInstance = DefaultModification.InitializeFromData(mod);

            if (modInstance == null)
            {
                return null;
            }

            return new InstalledMod(modInstance, _fileSystem.Path.GetDirectoryName(modFile));
        }

        #endregion
    }
}
