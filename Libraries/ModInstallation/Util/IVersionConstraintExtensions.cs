#region Usings

using System;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Mods;
using Semver;

#endregion

namespace ModInstallation.Util
{
    public static class IVersionConstraintExtensions
    {
        public static bool VersionMatches([NotNull] this IVersionConstraint constraint, [NotNull] SemVersion ver)
        {
            switch (constraint.Type)
            {
                case ConstraintType.Any:
                    return true;
                case ConstraintType.LessThan:
                    return ver < constraint.Version;
                case ConstraintType.LessThanEqual:
                    return ver <= constraint.Version;
                case ConstraintType.Equal:
                    return ver == constraint.Version;
                case ConstraintType.GreaterThanEqual:
                    return ver >= constraint.Version;
                case ConstraintType.GreaterThan:
                    return ver > constraint.Version;
                case ConstraintType.NotEqual:
                    return ver != constraint.Version;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
