using System.Collections.Generic;
using Semver;

namespace ModInstallation.Interfaces.Mods
{
    public interface IModGroup
    {
        string Id { get; }

        IDictionary<SemVersion, IModification> Versions { get; }
    }
}