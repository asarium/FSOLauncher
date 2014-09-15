#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;

#endregion

namespace ModInstallation.Implementations
{
    public class DefaultModManager : IModManager
    {
        private readonly List<IModRepository> _repositories = new List<IModRepository>();

        #region IModManager Members

        public IEnumerable<IModification> RemoteModifications
        {
            get
            {
                return
                    _repositories.Where(modRepository => modRepository.Modifications != null).SelectMany(modRepository => modRepository.Modifications);
            }
        }

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
