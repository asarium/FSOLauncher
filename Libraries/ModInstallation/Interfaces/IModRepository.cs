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
    public interface IModRepository : INotifyPropertyChanged
    {
        [NotNull]
        string Name { get; }

        [CanBeNull]
        IEnumerable<IModification> Modifications { get; }

        [NotNull]
        Task RetrieveRepositoryInformationAsync([NotNull] IProgress<string> progressReporter, CancellationToken token);
    }
}
