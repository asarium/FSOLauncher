#region Usings

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Mods;

#endregion

namespace ModInstallation.Interfaces
{
    public interface IDownloadProgress
    {
        [NotNull]
        Uri CurrentUri { get; }

        long CurrentBytes { get; }

        long TotalBytes { get; }

        double Speed { get; }

        bool VerifyingFile { get; }

        double VerificationProgress { get; }

        bool WaitingForSlot { get; }
    }

    public interface IFileDownloader
    {
        [NotNull]
        string DownloadDirectory { get; set; }

        int MaxConcurrentDownloads { get; set; }

        [NotNull]
        Task<FileInfo> DownloadFileAsync([NotNull] IFileInformation package, [NotNull] IProgress<IDownloadProgress> progressReporter,
            CancellationToken cancellationToken);
    }
}
