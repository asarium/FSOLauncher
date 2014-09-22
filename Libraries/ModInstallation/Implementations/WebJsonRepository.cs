using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using ModInstallation.Annotations;
using ModInstallation.Interfaces;

namespace ModInstallation.Implementations
{
    [Export(typeof(IModRepository))]
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