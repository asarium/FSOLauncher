#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Implementations.Mods;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Util;

#endregion

namespace ModInstallation.Implementations
{
    public class RemoteModManager : PropertyChangeBase, IRemoteModManager
    {
        private IEnumerable<IModGroup<IModification>> _modGroups;

        public RemoteModManager()
        {
            Repositories = new List<IModRepository>();
        }

        #region IRemoteModManager Members

        public IEnumerable<IModRepository> Repositories { get; set; }

        public IEnumerable<IModGroup<IModification>> ModGroups
        {
            get { return _modGroups; }
            private set
            {
                if (Equals(value, _modGroups))
                {
                    return;
                }
                _modGroups = value;
                OnPropertyChanged();
            }
        }

        public async Task GetModGroupsAsync(IProgress<string> progressReporter, bool force, CancellationToken token)
        {
            if (_modGroups != null && !force)
            {
                return;
            }

            progressReporter.Report("Starting information retrieval...");

            foreach (var modRepository in Repositories)
            {
                progressReporter.Report(string.Format("Retrieving information from repository '{0}'.", modRepository.Name));

                await modRepository.RetrieveRepositoryInformationAsync(progressReporter, token).ConfigureAwait(false);
            }

            ModGroups =
                Repositories.Where(modRepository => modRepository.Modifications != null)
                    .SelectMany(modRepository => modRepository.Modifications)
                    .GroupBy(x => x.Id)
                    .Select(g => new DefaultModGroup<IModification>(g))
                    .ToList();
        }

        #endregion
    }
}
