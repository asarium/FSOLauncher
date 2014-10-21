#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using FSOManagement.Annotations;
using FSOManagement.Interfaces.Mod;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Util;
using Splat;
using IDependencyResolver = ModInstallation.Interfaces.IDependencyResolver;

#endregion

namespace ModInstallation.Implementations.Management
{
    public class ModificationDependencies : IModDependencies
    {
        private readonly IDependencyResolver _dependencyResolver;

        private readonly ILocalModManager _localModManager;

        private readonly IInstalledModification _mod;

        public ModificationDependencies([NotNull] IInstalledModification mod, [NotNull] ILocalModManager localModManager)
        {
            _mod = mod;
            _localModManager = localModManager;

            _dependencyResolver = Locator.Current.GetService<IDependencyResolver>();
        }

        #region IModDependencies Members

        public IEnumerable<string> GetPrimaryDependencies(string rootPath)
        {
            // There are no primary dependencies in this case
            return Enumerable.Empty<string>();
        }

        public IEnumerable<string> GetSecondayDependencies(string rootPath)
        {
            if (_localModManager.Modifications == null)
            {
                return Enumerable.Empty<string>();
            }

            IEnumerable<IPackage> dependencies;
            try
            {
                dependencies = _dependencyResolver.ResolveDependencies(_mod, _localModManager.Modifications);
            }
            catch (InvalidOperationException)
            {
                // The resolver detected a dependency cycle
                return Enumerable.Empty<string>();
            }

            var mods = dependencies.GroupBy(p => p.ContainingModification).Select(group => group.Key).Cast<IInstalledModification>();

            return mods.Select(mod => mod.InstallPath);
        }

        #endregion
    }
}
