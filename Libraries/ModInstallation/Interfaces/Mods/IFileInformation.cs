#region Usings

using System;
using System.Collections.Generic;
using ModInstallation.Annotations;

#endregion

namespace ModInstallation.Interfaces.Mods
{
    public interface IFileInformation
    {
        [CanBeNull]
        IEnumerable<IFileVerifier> FileVerifiers { get; }

        [CanBeNull]
        string Destination { get; }

        [NotNull]
        IEnumerable<Uri> DownloadUris { get; }
    }
}
