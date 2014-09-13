#region Usings

using System.Collections.Generic;
using ModInstallation.Annotations;

#endregion

namespace ModInstallation.Interfaces.Mods
{
    public enum PackageStatus
    {
        Required,

        Recommended,

        Optional
    }

    public interface IPackage
    {
        [NotNull]
        IModification ContainingModification { get; }

        [CanBeNull]
        string Notes { get; }

        [NotNull]
        string Name { get; }

        PackageStatus Status { get; }

        [CanBeNull]
        IEnumerable<IModDependency> Dependencies { get; }

        [NotNull]
        IEnumerable<IFileInformation> Files { get; }

        [CanBeNull]
        IEnumerable<IEnvironmentConstraint> EnvironmentConstraints { get; }
    }
}
