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
        [NotNull]
        IEnumerable<IModRepository> Repositories { get; set; }

        IEnumerable<IModGroup<IModification>> ModGroups { get; }

        /// <summary>
        ///     Retrieves the modification groups, this may fetch the necessary information from the repositories if it is not
        ///     already available.
        /// </summary>
        /// <remarks>This function may run synchronously if the requested information is already present.</remarks>
        /// <param name="progressReporter">The reporter used for notifying a listener of status updates</param>
        /// <param name="force">If set to <c>true</c> the repositories will always be used to retrieve the information.</param>
        /// <param name="token">A cancellation token that can be used to abort the operation</param>
        /// <returns>The modification groups.</returns>
        [NotNull]
        Task GetModGroupsAsync([NotNull] IProgress<string> progressReporter, bool force, CancellationToken token);
    }
}
