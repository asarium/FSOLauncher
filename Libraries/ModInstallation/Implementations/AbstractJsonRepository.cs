#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Implementations.DataClasses;
using ModInstallation.Implementations.Mods;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using Newtonsoft.Json;

#endregion

namespace ModInstallation.Implementations
{
    public abstract class AbstractJsonRepository : IModRepository
    {
        private readonly JsonSerializer _jsonSerializer;

        private List<IModification> _modifications;

        protected AbstractJsonRepository([NotNull] string name)
        {
            Name = name;

            _jsonSerializer = new JsonSerializer();
        }

        #region IModRepository Members

        public string Name { get; private set; }

        public IEnumerable<IModification> Modifications
        {
            get { return _modifications; }
        }

        public async Task RetrieveRepositoryInformationAsync(IProgress<string> progressReporter, CancellationToken token)
        {
            var jsonContent = await GetRepositoryJsonAsync(progressReporter, token);

            if (token.IsCancellationRequested)
            {
                return;
            }

            // This may take a while so do this in the background
            var repoData = await Task.Run(() => _jsonSerializer.Deserialize<Repository>(new JsonTextReader(new StringReader(jsonContent))), token);

            _modifications = repoData.mods == null
                ? null
                : new List<IModification>(repoData.mods.Select(mod => DefaultModification.InitializeFromData(mod)));
        }

        #endregion

        [NotNull]
        protected abstract Task<string> GetRepositoryJsonAsync([NotNull] IProgress<string> reporter, CancellationToken token);
    }
}
