using System.Collections;
using System.Collections.Generic;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Mods;

namespace ModInstallation.Interfaces
{
    public interface IDependencyResolver
    {
        [NotNull]
        IEnumerable<IPackage> ResolveDependencies([NotNull] IPackage package, [NotNull] IEnumerable<IModification> allModifications);
    }
}