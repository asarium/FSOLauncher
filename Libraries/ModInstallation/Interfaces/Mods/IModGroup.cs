using System.Collections.Generic;
using Semver;

namespace ModInstallation.Interfaces.Mods
{
    public interface IModGroup<T> where T : IModification
    {
        string Id { get; }

        IReadOnlyDictionary<SemVersion, T> Versions { get; }
    }
}