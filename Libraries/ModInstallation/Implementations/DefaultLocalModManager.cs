#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        public string InstallPath
        {
            get { return _installPath; }
        }

        #endregion
    }

    [Export(typeof(ILocalModManager))]
    public class DefaultLocalModManager : ILocalModManager, IEnableLogger
    {
        private const string ModInfoFile = "mod.json";

        private readonly ReactiveList<IInstalledModification> _modifications;

        public DefaultLocalModManager()
        {
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
            var packageFile = Path.Combine(GetInstallationDirectory(package.ContainingModification), ModInfoFile);
            Modification modData;

            if (File.Exists(packageFile))
            {
                string content;
                using (var reader = File.OpenText(packageFile))
                {
                    content = await reader.ReadToEndAsync();
                }

                modData = await Task.Run(() => JsonConvert.DeserializeObject<Modification>(content));

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

            var json = await Task.Run(() => JsonConvert.SerializeObject(modData,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                }));

            var directoryName = Path.GetDirectoryName(packageFile);
            if (directoryName != null && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            using (var stream = File.Create(packageFile, 8 * 1024, FileOptions.Asynchronous))
            {
                using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(json);
                }
            }

            var newData = DefaultModification.InitializeFromData(modData);

            if (newData == null)
            {
                return;
            }

            AddModification(newData);
        }

        public async Task ParseLocalModDataAsync()
        {
            _modifications.Clear();

            if (PackageDirectory == null)
            {
                return;
            }

            if (!Directory.Exists(PackageDirectory))
            {
                return;
            }

            foreach (var modDir in Directory.EnumerateDirectories(PackageDirectory))
            {
                foreach (var versionDir in Directory.EnumerateDirectories(modDir))
                {
                    var modFile = Path.Combine(versionDir, ModInfoFile);

                    if (!File.Exists(modFile))
                    {
                        continue;
                    }

                    await ProcessModFile(modFile);
                }
            }
        }

        #endregion

        private void AddModification([NotNull] IModification newData)
        {
            int currentIndex;
            for (currentIndex = 0; currentIndex < _modifications.Count; ++currentIndex)
            {
                if (_modifications[currentIndex].Id == newData.Id)
                {
                    break;
                }
            }

            var installedMod = new InstalledMod(newData, GetInstallationDirectory(newData));
            if (currentIndex >= 0 && currentIndex < _modifications.Count)
            {
                _modifications[currentIndex] = installedMod;
            }
            else
            {
                _modifications.Add(installedMod);
            }
        }

        [NotNull]
        private async Task ProcessModFile([NotNull] string modFile)
        {
            string content;
            try
            {
                using (var stream = new FileStream(modFile, FileMode.Open, FileAccess.Read, FileShare.Read, 8 * 1024, true))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        content = await reader.ReadToEndAsync();
                    }
                }
            }
            catch (IOException e)
            {
                this.Log().ErrorException("Failed to load " + ModInfoFile + " content!", e);
                return;
            }

            Modification mod;
            try
            {
                mod = await Task.Run(() => JsonConvert.DeserializeObject<Modification>(content));
            }
            catch (Exception e)
            {
                this.Log().ErrorException(ModInfoFile + " parsing failed!", e);
                return;
            }

            var modInstance = DefaultModification.InitializeFromData(mod);

            if (modInstance == null)
            {
                return;
            }

            AddModification(modInstance);
        }

        [NotNull]
        private static Modification GetDataClass([NotNull] IPackage package)
        {
            var data = package.ContainingModification.ToDataClass();
            data.packages = data.packages.Where(p => p.name == package.Name);

            return data;
        }

        [NotNull]
        private string GetInstallationDirectory([NotNull] IModification mod)
        {
            if (PackageDirectory == null)
            {
                return Path.Combine(mod.Id, mod.Version.ToString());
            }

            return Path.Combine(PackageDirectory, mod.Id, mod.Version.ToString());
        }
    }
}
