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
    [Export(typeof(IModManager))]
    public class DefaultModManager : PropertyChangeBase, IModManager
    {
        private readonly List<IModRepository> _repositories = new List<IModRepository>();

        #region IModManager Members

        public IEnumerable<IModification> RemoteModifications { get; private set; }

        public IEnumerable<IModification> LocalModifications { get; private set; }

        public async Task RetrieveInformationAsync(IProgress<string> progressReporter, CancellationToken token)
        {
            progressReporter.Report("Starting information retrieval...");

            foreach (var modRepository in _repositories)
            {
                progressReporter.Report(string.Format("Retrieving information from repository '{0}'.", modRepository.Name));

                var repository = modRepository;

                var reporter =
                    new Progress<string>(
                        message =>
                            progressReporter.Report(string.Format("Retrieving information from repository '{0}': {1}", repository.Name, message)));

                await modRepository.RetrieveRepositoryInformationAsync(reporter, token);
            }

            RemoteModifications =
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
