#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
    [Export(typeof(IRemoteModManager))]
    public class DefaultRemoteModManager : PropertyChangeBase, IRemoteModManager
    {
        private IEnumerable<IModGroup> _modificationGroups;

        public DefaultRemoteModManager()
        {
            Repositories = new List<IModRepository>();
        }

        #region IRemoteModManager Members

        public IEnumerable<IModGroup> ModificationGroups
        {
            get { return _modificationGroups; }
            private set
            {
                if (Equals(value, _modificationGroups))
                {
                    return;
                }
                _modificationGroups = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<IModRepository> Repositories { get; set; }

        public async Task RetrieveInformationAsync(IProgress<string> progressReporter, CancellationToken token)
        {
            progressReporter.Report("Starting information retrieval...");

            foreach (var modRepository in Repositories)
            {
                progressReporter.Report(string.Format("Retrieving information from repository '{0}'.", modRepository.Name));

                await modRepository.RetrieveRepositoryInformationAsync(progressReporter, token);
            }

            ModificationGroups =
                Repositories.Where(modRepository => modRepository.Modifications != null)
                    .SelectMany(modRepository => modRepository.Modifications)
                    .GroupBy(x => x.Id)
                    .Select(g => new DefaultModGroup(g))
                    .ToList();
        }

        #endregion
    }
}
