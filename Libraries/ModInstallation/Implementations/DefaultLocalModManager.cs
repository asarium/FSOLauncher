#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FSOManagement.Util;
using ModInstallation.Annotations;
using ModInstallation.Implementations.DataClasses;
using ModInstallation.Implementations.Mods;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Util;
using Newtonsoft.Json;
using ReactiveUI;
using Semver;
using Splat;

#endregion

namespace ModInstallation.Implementations
{
    internal class InstalledMod : IInstalledModification
    {
        private readonly string _installPath;

        private readonly IModification _wrappedModification;

        public InstalledMod([NotNull] IModification wrappedModification, [NotNull] string installPath)
        {
            _wrappedModification = wrappedModification;
            _installPath = installPath;
        }

        #region IInstalledModification Members

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { _wrappedModification.PropertyChanged += value; }
            remove { _wrappedModification.PropertyChanged -= value; }
        }

        public string Title
        {
            get { return _wrappedModification.Title; }
        }

        public SemVersion Version
        {
            get { return _wrappedModification.Version; }
        }

        public string Id
        {
            get { return _wrappedModification.Id; }
        }

        public string Commandline
        {
            get { return _wrappedModification.Commandline; }
        }

        public string FolderName
        {
            get { return _wrappedModification.FolderName; }
        }

        public IEnumerable<IPackage> Packages
        {
            get { return _wrappedModification.Packages; }
        }

        public string Description
        {
            get { return _wrappedModification.Description; }
        }

        public Uri LogoUri
        {
            get { return _wrappedModification.LogoUri; }
        }

        public IPostInstallActions PostInstallActions
        {
            get { return _wrappedModification.PostInstallActions; }
        }

        public string InstallPath
        {
            get { return _installPath; }
        }

        #region Implementation of IEquatable<IModification>

        public bool Equals(IModification other)
        {
            return _wrappedModification.Equals(other);
        }

        #endregion

        #endregion
    }

    [Export(typeof(ILocalModManager))]
    public class DefaultLocalModManager : ILocalModManager, IEnableLogger
    {
        private const string ModInfoFile = "mod.json";

        private readonly IFileSystem _fileSystem;

        private readonly ReactiveList<IInstalledModification> _modifications;

        [ImportingConstructor]
        public DefaultLocalModManager(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _modifications = new ReactiveList<IInstalledModification>();
        }

        #region ILocalModManager Members

        public IEnumerable<IInstalledModification> Modifications
        {
            get { return _modifications; }
        }

        public string PackageDirectory { get; set; }

        public async Task AddPackageAsync(IPackage package)
        {
            var packageFile = _fileSystem.Path.Combine(package.ContainingModification.GetInstallationPath(PackageDirectory), ModInfoFile);
            Modification modData;

            if (_fileSystem.File.Exists(packageFile))
            {
                modData = await _fileSystem.ParseJSONFile<Modification>(packageFile).ConfigureAwait(false);

                if (modData.packages.Any(p => p.name == package.Name))
                {
                    // the package is already present...
                    return;
                }

                // add the package
                modData.packages = modData.packages.Concat(package.ToDataClass().AsEnumerable());
            }
            else
            {
                modData = GetDataClass(package);
            }

            await UpdateModData(modData, packageFile).ConfigureAwait(false);
        }

        public async Task ParseLocalModDataAsync()
        {
            await Observable.Start(() => _modifications.Clear(), RxApp.MainThreadScheduler);

            if (PackageDirectory == null)
            {
                return;
            }

            if (!_fileSystem.Directory.Exists(PackageDirectory))
            {
                return;
            }

            foreach (var modDir in _fileSystem.Directory.EnumerateDirectories(PackageDirectory))
            {
                foreach (var versionDir in _fileSystem.Directory.EnumerateDirectories(modDir))
                {
                    var modFile = _fileSystem.Path.Combine(versionDir, ModInfoFile);

                    if (!_fileSystem.File.Exists(modFile))
                    {
                        continue;
                    }

                    await ProcessModFile(modFile).ConfigureAwait(false);
                }
            }
        }

        public async Task RemovePackageAsync(IPackage package)
        {
            if (!_modifications.Any(mod => mod.Equals(package.ContainingModification) && mod.Packages.Contains(package)))
            {
                // Mod is not known, nothing to do here
                return;
            }

            var packageFile = _fileSystem.Path.Combine(package.ContainingModification.GetInstallationPath(PackageDirectory), ModInfoFile);

            if (!_fileSystem.File.Exists(packageFile))
            {
                await RemoveModification(package.ContainingModification).ConfigureAwait(false);
                return;
            }

            var currentData = await _fileSystem.ParseJSONFile<Modification>(packageFile).ConfigureAwait(false);

            // Update the data
            currentData.packages = currentData.packages.Where(p => !string.Equals(p.name, package.Name)).ToList();

            if (!currentData.packages.Any())
            {
                // If there are no packages left delete the mod file
                await _fileSystem.File.DeleteAsync(packageFile).ConfigureAwait(false);
                await RemoveModification(package.ContainingModification).ConfigureAwait(false);
            }
            else
            {
                await UpdateModData(currentData, packageFile).ConfigureAwait(false);
            }
        }

        #endregion

        private async Task UpdateModData(Modification modData, string packageFile)
        {
            var json = await Task.Run(() => JsonConvert.SerializeObject(modData,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                })).ConfigureAwait(false);

            var directoryName = _fileSystem.Path.GetDirectoryName(packageFile);
            if (directoryName != null && !_fileSystem.Directory.Exists(directoryName))
            {
                _fileSystem.Directory.CreateDirectory(directoryName);
            }

            using (var stream = _fileSystem.File.Create(packageFile))
            {
                using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(json).ConfigureAwait(false);
                }
            }

            var newData = DefaultModification.InitializeFromData(modData);

            if (newData == null)
            {
                return;
            }

            await AddModification(newData).ConfigureAwait(false);
        }

        private async Task RemoveModification([NotNull] IModification mod)
        {
            // Remove the mod on the main thread
            await Observable.Start(() => _modifications.RemoveAt(_modifications.IndexOf(m => m.Equals(mod))), RxApp.MainThreadScheduler);
        }

        private async Task AddModification([NotNull] IModification newData)
        {
            // Always execute this on the UI thread
            await Observable.Start(() =>
            {
                int currentIndex;
                for (currentIndex = 0; currentIndex < _modifications.Count; ++currentIndex)
                {
                    if (_modifications[currentIndex].Id == newData.Id)
                    {
                        break;
                    }
                }

                var installedMod = new InstalledMod(newData, newData.GetInstallationPath(PackageDirectory));
                if (currentIndex >= 0 && currentIndex < _modifications.Count)
                {
                    _modifications[currentIndex] = installedMod;
                }
                else
                {
                    _modifications.Add(installedMod);
                }
            },
                RxApp.MainThreadScheduler);
        }

        [NotNull]
        private async Task ProcessModFile([NotNull] string modFile)
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
                return;
            }

            Modification mod;
            try
            {
                mod = await Task.Run(() => JsonConvert.DeserializeObject<Modification>(content)).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.Log().ErrorException(modFile + " parsing failed!", e);
                return;
            }

            var modInstance = DefaultModification.InitializeFromData(mod);

            if (modInstance == null)
            {
                return;
            }

            await AddModification(modInstance).ConfigureAwait(false);
        }

        [NotNull]
        private static Modification GetDataClass([NotNull] IPackage package)
        {
            var data = package.ContainingModification.ToDataClass();
            data.packages = data.packages.Where(p => p.name == package.Name);

            return data;
        }
    }
}
