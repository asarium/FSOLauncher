#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    public interface IPackage : IEquatable<IPackage>, INotifyPropertyChanged
    {
        [NotNull]
        IModification ContainingModification { get; }

        [CanBeNull]
        string Notes { get; }

        [NotNull]
        string Name { get; }

        PackageStatus Status { get; }

        [NotNull]
        IEnumerable<IModDependency> Dependencies { get; }

        [NotNull]
        IEnumerable<IFileInformation> Files { get; }

        [CanBeNull]
        IEnumerable<IFileListItem> FileList { get; }

        [CanBeNull]
        IEnumerable<IEnvironmentConstraint> EnvironmentConstraints { get; }
    }
}
