using System;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;

namespace ModInstallation.Interfaces.Net
{
    public interface IWebClient
    {
        [NotNull]
        Task<IResponse> GetAsync([NotNull] Uri uri, CancellationToken token, TimeSpan? cacheDuration = null);
    }
}