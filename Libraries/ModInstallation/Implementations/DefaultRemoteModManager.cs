#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Util;

#endregion

namespace ModInstallation.Implementations
{
    [Export(typeof(IRemoteModManager))]
    public class DefaultRemoteModManager : PropertyChangeBase, IRemoteModManager
    {
        private IEnumerable<IModification> _modifications;

        public DefaultRemoteModManager()
        {
            Repositories = new List<IModRepository>();
        }

        #region IRemoteModManager Members

        public IEnumerable<IModification> Modifications
        {
            get { return _modifications; }
            private set
            {
                if (Equals(value, _modifications))
                {
                    return;
                }
                _modifications = value;
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

            Modifications =
                Repositories.Where(modRepository => modRepository.Modifications != null)
                    .SelectMany(modRepository => modRepository.Modifications)
                    .ToList();
        }

        #endregion
    }
}
