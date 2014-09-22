using System.Collections.Generic;
using System.ComponentModel;
using ModInstallation.Annotations;
using Semver;

namespace ModInstallation.Interfaces.Mods
{
    public interface IModification : INotifyPropertyChanged
    {
        [NotNull]
        string Title { get; }

        [NotNull]
        SemVersion Version { get; }

        [NotNull]
        string Id { get; }

        [NotNull]
        IEnumerable<IPackage> Packages { get; }

        [CanBeNull]
        string Description { get;}
    }
}