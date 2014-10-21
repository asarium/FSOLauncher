using ModInstallation.Annotations;
using Semver;

namespace ModInstallation.Interfaces.Mods
{
    public interface IVersionConstraint
    {
        ConstraintType Type { get; }

        [CanBeNull]
        SemVersion Version { get; }
    }
}