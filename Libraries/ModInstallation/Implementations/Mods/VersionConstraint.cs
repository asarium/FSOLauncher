using System;
using ModInstallation.Annotations;
using Semver;

namespace ModInstallation.Implementations.Mods
{
    public enum ConstraintType
    {
        Any,

        LessThan,

        LessThanEqual,

        Equal,

        GreaterThanEqual,

        GreaterThan,

        NotEqual
    }

    public sealed class VersionConstraint
    {
        public VersionConstraint(ConstraintType type, [CanBeNull] SemVersion version)
        {
            Type = type;
            Version = version;
        }

        public ConstraintType Type { get; private set; }

        [CanBeNull]
        public SemVersion Version { get; private set; }

        public bool VersionMatches([NotNull] SemVersion ver)
        {
            switch (Type)
            {
                case ConstraintType.Any:
                    return true;
                case ConstraintType.LessThan:
                    return ver < Version;
                case ConstraintType.LessThanEqual:
                    return ver <= Version;
                case ConstraintType.Equal:
                    return ver ==Version;
                case ConstraintType.GreaterThanEqual:
                    return ver >= Version;
                case ConstraintType.GreaterThan:
                    return ver > Version;
                case ConstraintType.NotEqual:
                    return ver != Version;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool Equals([NotNull] VersionConstraint other)
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