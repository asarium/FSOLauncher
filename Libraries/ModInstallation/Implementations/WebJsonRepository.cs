using System;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using ModInstallation.Annotations;

namespace ModInstallation.Implementations
{
    public class WebJsonRepository : AbstractJsonRepository
    {
        private readonly string _location;

        public WebJsonRepository([NotNull] string name, [NotNull] string location) : base(name)
        {
            _location = location;
        }

        protected override Task<string> GetRepositoryJsonAsync(IProgress<string> reporter, CancellationToken token)
        {
            reporter.Report(string.Format("Retrieving information from " + _location));
            return _location.GetStringAsync();
        }
    }
}