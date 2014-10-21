using System.Collections.Generic;
using System.Linq;
using ModInstallation.Annotations;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;

namespace ModInstallation.Util
{
    public static class IDependencyResolverExtensions
    {
        [NotNull]
        public static IEnumerable<IPackage> ResolveDependencies([NotNull] this IDependencyResolver This, [NotNull] IModification modification,
            [NotNull] IEnumerable<IModification> allModifications,
            [CanBeNull] IErrorHandler handler = null)
        {
            var dependencies = modification.Packages.Select(p => This.ResolveDependencies(p, allModifications, handler)).ToList();
            if (!dependencies.Any())
            {
                return Enumerable.Empty<IPackage>();
            }

            return dependencies.Aggregate(Enumerable.Empty<IPackage>(), (current, otherPackages) => current.Union(otherPackages));
        }
    }
}