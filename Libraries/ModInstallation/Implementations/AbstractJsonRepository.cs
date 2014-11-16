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
using ModInstallation.Util;
using Newtonsoft.Json;

#endregion

namespace ModInstallation.Implementations
{
    public abstract class AbstractJsonRepository : PropertyChangeBase, IModRepository
    {
        private readonly Uri _initialLocation;

        private readonly JsonSerializer _jsonSerializer;

        private List<IModification> _modifications;

        protected AbstractJsonRepository([NotNull] string name)
        {
            Name = name;

            _jsonSerializer = new JsonSerializer();

            if (!Uri.TryCreate(name, UriKind.Absolute, out _initialLocation))
            {
                throw new ArgumentException("name is not a valid URI!");
            }
        }

        #region IModRepository Members

        public string Name { get; private set; }

        public IEnumerable<IModification> Modifications
        {
            get { return _modifications; }
        }

        public async Task RetrieveRepositoryInformationAsync(IProgress<string> progressReporter, CancellationToken token)
        {
            var locationQueue = new Queue<Uri>();
            locationQueue.Enqueue(_initialLocation);

            var modifications = new List<IModification>();
            while (locationQueue.Count > 0)
            {
                var location = locationQueue.Dequeue();
                var jsonContent = await GetRepositoryJsonAsync(location, progressReporter, token);

                if (token.IsCancellationRequested)
                {
                    return;
                }

                // This may take a while so do this in the background
                var repoData = await Task.Run(() => _jsonSerializer.Deserialize<Repository>(new JsonTextReader(new StringReader(jsonContent))), token);

                if (repoData.mods != null)
                {
                    modifications.AddRange(repoData.mods.Select(mod => DefaultModification.InitializeFromData(mod)));
                }

                if (repoData.includes == null)
                {
                    continue;
                }

                foreach (var uri in repoData.includes.Where(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute)).Select(uri => new Uri(uri)))
                {
                    locationQueue.Enqueue(uri);
                }
            }

            _modifications = modifications;
        }

        #endregion

        [NotNull]
        protected abstract Task<string> GetRepositoryJsonAsync([NotNull] Uri repoLocation,
            [NotNull] IProgress<string> reporter,
            CancellationToken token);
    }
}
