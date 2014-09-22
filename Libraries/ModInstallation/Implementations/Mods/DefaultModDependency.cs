#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using ModInstallation.Annotations;
using ModInstallation.Implementations.DataClasses;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Util;
using Semver;

#endregion

namespace ModInstallation.Implementations.Mods
{
    public class DefaultModDependency : PropertyChangeBase, IModDependency
    {
        private List<VersionConstraint> _versionConstraints;

        #region IModDependency Members

        public IEnumerable<string> PackageNames { get; private set; }

        public string ModId { get; private set; }

        public bool VersionMatches(SemVersion version)
        {
            if (_versionConstraints == null)
            {
                return true;
            }

            return _versionConstraints.All(constraint => constraint.VersionMatches(version));
        }

        #endregion

        [NotNull]
        public static DefaultModDependency InitializeFromData([NotNull] Dependency dep)
        {
            var newInstance = new DefaultModDependency
            {
                ModId = dep.id,
                _versionConstraints = ParseVersionString(dep.version).ToList(),
                PackageNames = dep.packages != null ? dep.packages.ToList() : Enumerable.Empty<string>()
            };

            return newInstance;
        }

        [NotNull]
        public static IEnumerable<VersionConstraint> ParseVersionString([NotNull] string version)
        {
            var parts = version.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(str => str.Trim());

            return parts.Select(ParseVersionConstraint).Where(c => c != null);
        }

        [CanBeNull]
        public static VersionConstraint ParseVersionConstraint([NotNull] string str)
        {
            if (str == "*")
            {
                return new VersionConstraint(ConstraintType.Any, null);
            }

            SemVersion ver;
            if (SemVersion.TryParse(str, out ver))
            {
                return new VersionConstraint(ConstraintType.Equal, ver);
            }

            // Make sure that no exception is thrown
            if (str.Length < 2)
            {
                return null;
            }

            var type = ConstraintType.Any;
            if (str.StartsWith("<"))
            {
                if (str.StartsWith("<="))
                {
                    type = ConstraintType.LessThanEqual;
                    str = str.Substring(2);
                }
                else
                {
                    type = ConstraintType.LessThan;
                    str = str.Substring(1);
                }
            }

            if (str.StartsWith("=="))
            {
                type = ConstraintType.Equal;
                str = str.Substring(2);
            }

            if (str.StartsWith(">"))
            {
                if (str.StartsWith(">="))
                {
                    type = ConstraintType.GreaterThanEqual;
                    str = str.Substring(2);
                }
                else
                {
                    type = ConstraintType.GreaterThan;
                    str = str.Substring(1);
                }
            }

            if (str.StartsWith("!="))
            {
                type = ConstraintType.NotEqual;
                str = str.Substring(2);
            }

            return SemVersion.TryParse(str, out ver) ? new VersionConstraint(type, ver) : null;
        }
    }
}
