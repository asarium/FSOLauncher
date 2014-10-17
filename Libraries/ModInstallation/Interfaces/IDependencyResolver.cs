#region Usings

using System.Collections.Generic;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Mods;

#endregion

namespace ModInstallation.Interfaces
{
    public interface IDependencyResolver
    {
        [NotNull]
        IEnumerable<IPackage> ResolveDependencies([NotNull] IPackage package, [NotNull] IEnumerable<IModification> allModifications,
            [CanBeNull] IErrorHandler handler);
    }
}
