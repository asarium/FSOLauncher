#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using ModInstallation.Annotations;

#endregion

namespace ModInstallation.Interfaces.Mods
{
    public interface IFileInformation : INotifyPropertyChanged
    {
        [CanBeNull]
        string FileName { get; }

        [CanBeNull]
        IEnumerable<IFileVerifier> FileVerifiers { get; }

        [CanBeNull]
        string Destination { get; }

        [NotNull]
        IEnumerable<Uri> DownloadUris { get; }
    }
}
