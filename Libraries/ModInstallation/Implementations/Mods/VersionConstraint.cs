using System;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Mods;
using Semver;

namespace ModInstallation.Implementations.Mods
{
    public sealed class VersionConstraint : IVersionConstraint
    {
        public VersionConstraint(ConstraintType type, [CanBeNull] SemVersion version)
        {
            Type = type;
            Version = version;
        }

        public ConstraintType Type { get; private set; }

        public SemVersion Version { get; private set; }

        private bool Equals([NotNull] IVersionConstraint other)
        {
            return Type == other.Type && Equals(Version, other.Version);
        }

        public override bool Equals([CanBeNull] object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((VersionConstraint) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Type * 397) ^ (Version != null ? Version.GetHashCode() : 0);
            }
        }

        public static bool operator ==(VersionConstraint left, VersionConstraint right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(VersionConstraint left, VersionConstraint right)
        {
            return !Equals(left, right);
        }
    }
}