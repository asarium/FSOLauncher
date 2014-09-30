#region Usings

using System.Collections.Generic;
using System.ComponentModel;
using ModInstallation.Annotations;
using Semver;

#endregion

namespace ModInstallation.Interfaces.Mods
{
    public interface IModDependency : INotifyPropertyChanged
    {
        [NotNull]
        IEnumerable<string> PackageNames { get; }

        [NotNull]
        string ModId { get; }

        [NotNull]
        IEnumerable<IVersionConstraint> VersionConstraints { get; }
    }
}
