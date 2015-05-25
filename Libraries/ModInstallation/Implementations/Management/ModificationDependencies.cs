#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO.Abstractions;
using System.Linq;
using FSOManagement.Annotations;
using FSOManagement.Interfaces.Mod;
using ModInstallation.Exceptions;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Util;
using Splat;
using IDependencyResolver = ModInstallation.Interfaces.IDependencyResolver;

#endregion

namespace ModInstallation.Implementations.Management
{
    [Export(typeof(IModDependencies))]
    public class ModificationDependencies : IModDependencies, IEnableLogger
    {
        private readonly IDependencyResolver _dependencyResolver;

        private readonly IFileSystem _fileSystem;

        private readonly ILocalModManager _localModManager;

        [ImportingConstructor]
        public ModificationDependencies([NotNull] IModInstallationManager modInstallationManager, IDependencyResolver dependencyResolver, IFileSystem fileSystem)
        {
            _dependencyResolver = dependencyResolver;
            _fileSystem = fileSystem;
            _localModManager = modInstallationManager.LocalModManager;
        }

        #region Implementation of IModDependencies

        public int GetSupportScore(ILocalModification mod)
        {
            if (mod is InstalledModification)
            {
                return 1000;
            }

            return int.MinValue;
        }

        public IEnumerable<string> GetModPaths(ILocalModification localMod, string rootPath)
        {
            var installedMod = localMod as InstalledModification;

            if (installedMod == null)
            {
                throw new NotSupportedException("Wrong type given!");
            }

            if (_localModManager.Modifications == null)
            {
                return Enumerable.Empty<string>();
            }

            // Make sure the local mod manager lives in the same directory
            if (!_fileSystem.Path.GetFullPath(_localModManager.PackageDirectory).StartsWith(_fileSystem.Path.GetFullPath(rootPath)))
            {
                this.Log().Error("Local mod manager path '{0}' is not a child of the root path '{1}'!", _localModManager.PackageDirectory, rootPath);
                return Enumerable.Empty<string>();
            }

            var modification = installedMod.Modification;

            IEnumerable<IPackage> dependencies;
            try
            {
                dependencies = _dependencyResolver.ResolveDependencies(modification, _localModManager.Modifications);
            }
            catch (InvalidOperationException)
            {
                // The resolver detected a dependency cycle
                return Enumerable.Empty<string>();
            }
            catch (DependencyException)
            {
                return Enumerable.Empty<string>();
            }

            // Make sure that we actually select the installed mods
            var mods =
                dependencies.GroupBy(p => p.ContainingModification)
                    .Where(x => !modification.Equals(x.Key))
                    .Select(group => _localModManager.Modifications.FirstOrDefault(x => x.Equals(@group.Key)));

            return mods.Select(mod => mod.InstallPath);
        }

        #endregion
    }
}
