#region Usings

using System;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;

#endregion

namespace ModInstallation.Interfaces.Net
{
    public struct DownloadProgress
    {
        public long Current { get; set; }

        public long Total { get; set; }
    }

    public interface IWebClient
    {
        [NotNull]
        Task<string> GetStringAsync([NotNull] Uri uri, CancellationToken token, TimeSpan? cacheDuration);

        [NotNull]
        Task DownloadAsync([NotNull] Uri uri, [NotNull] string destination, [NotNull] Action<DownloadProgress> downloadReporter, CancellationToken token);
    }
}
