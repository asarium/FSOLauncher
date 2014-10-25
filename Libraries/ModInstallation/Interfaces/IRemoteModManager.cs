#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Mods;

#endregion

namespace ModInstallation.Interfaces
{
    public interface IRemoteModManager : INotifyPropertyChanged
    {
        [CanBeNull]
        IEnumerable<IModification> Modifications { get; }

        [NotNull]
        IEnumerable<IModRepository> Repositories { get; set; }

        [NotNull]
        Task RetrieveInformationAsync([NotNull] IProgress<string> progressReporter, CancellationToken token);
    }
}
