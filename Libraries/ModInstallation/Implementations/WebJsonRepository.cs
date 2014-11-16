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
        public WebJsonRepository([NotNull] string location) : base(location)
        {
        }

        [NotNull]
        public IWebClient WebClient { private get; set; }

        protected override async Task<string> GetRepositoryJsonAsync(Uri location, IProgress<string> reporter, CancellationToken token)
        {
            reporter.Report(string.Format("Retrieving information from " + location));
            using (var response = await WebClient.GetAsync(location, token, TimeSpan.FromHours(3)))
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
