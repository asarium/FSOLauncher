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
        private readonly List<IModRepository> _repositories = new List<IModRepository>();

        private IEnumerable<IModification> _modifications;

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

        public async Task RetrieveInformationAsync(IProgress<string> progressReporter, CancellationToken token)
        {
            progressReporter.Report("Starting information retrieval...");

            foreach (var modRepository in _repositories)
            {
                progressReporter.Report(string.Format("Retrieving information from repository '{0}'.", modRepository.Name));

                await modRepository.RetrieveRepositoryInformationAsync(progressReporter, token);
            }

            Modifications =
                _repositories.Where(modRepository => modRepository.Modifications != null)
                    .SelectMany(modRepository => modRepository.Modifications)
                    .ToList();
        }

        public void AddModRepository(IModRepository repo)
        {
            if (repo == null)
            {
                throw new ArgumentNullException("repo");
            }

            _repositories.Add(repo);
        }

        #endregion
    }
}
