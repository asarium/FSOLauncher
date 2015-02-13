#region Usings

using System.Linq;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Mods;
using Semver;

#endregion

namespace ModInstallation.Util
{
    public static class IModDependencyExtensions
    {
        public static bool VersionMatches([NotNull] this IModDependency dependency, [NotNull] SemVersion version)
        {
            return dependency.VersionConstraints.All(constraint => constraint.VersionMatches(version));
        }
    }
}
