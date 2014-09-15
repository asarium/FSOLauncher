#region Usings

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Mods;

#endregion

namespace ModInstallation.Interfaces
{
    public interface IModManager
    {
        [CanBeNull]
        IEnumerable<IModification> RemoteModifications { get; }

        [CanBeNull]
        IEnumerable<IModification> LocalModifications { get; }

        [NotNull]
        Task RetrieveInformationAsync([NotNull] IProgress<string> progressReporter, CancellationToken token);

        void AddModRepository([NotNull] IModRepository repo);
    }
}
