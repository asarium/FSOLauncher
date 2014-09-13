using System.Collections.Generic;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;

namespace ModInstallation.Implementations
{
    public class DefaultDependencyResolver : IDependencyResolver
    {
        public IEnumerable<IPackage> ResolveDependencies(IPackage package, IEnumerable<IModification> allModifications)
        {
            throw new System.NotImplementedException();
        }
    }
}