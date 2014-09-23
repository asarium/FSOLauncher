#region Usings

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Net;

#endregion

namespace ModInstallation.Implementations
{
    public class WebJsonRepository : AbstractJsonRepository
    {
        private readonly Uri _location;

        public WebJsonRepository([NotNull] string location) : base(location)
        {
            if (!Uri.TryCreate(location, UriKind.Absolute, out _location))
            {
                throw new ArgumentException(string.Format("Invalid URL: {0}", location));
            }
        }

        [NotNull]
        public IWebClient WebClient { private get; set; }

        protected override async Task<string> GetRepositoryJsonAsync(IProgress<string> reporter, CancellationToken token)
        {
            reporter.Report(string.Format("Retrieving information from " + _location));
            using (var response = await WebClient.GetAsync(_location, token, TimeSpan.FromHours(3)))
            {
                using (var stream = await response.OpenStreamAsync())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
            }
        }
    }
}
