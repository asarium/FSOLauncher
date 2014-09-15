﻿#region Usings

using System.Collections.Generic;
using ModInstallation.Annotations;
using Semver;

#endregion

namespace ModInstallation.Interfaces.Mods
{
    public interface IModDependency
    {
        [NotNull]
        IEnumerable<string> PackageNames { get; }

        [NotNull]
        string ModId { get; }

        bool VersionMatches([NotNull] SemVersion version);
    }
}
