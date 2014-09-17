#region Usings

using System.Linq;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Mods;

#endregion

namespace ModInstallation.Util
{
    public static class PackageExtensions
    {
        public static bool EnvironmentSatisfied([NotNull] this IPackage package)
        {
            return package.EnvironmentConstraints == null || package.EnvironmentConstraints.All(env => env.EnvironmentSatisfied());
        }
    }
}
