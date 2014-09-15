#region Usings

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Exceptions;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;

#endregion

namespace ModInstallation.Implementations
{
    internal class DefaultDownloadProgress : IDownloadProgress
    {
        #region IDownloadProgress Members

        public Uri CurrentUri { get; set; }

        public long CurrentBytes { get; set; }

        public long TotalBytes { get; set; }

        public double Speed { get; set; }

        public bool VerifyingFile { get; set; }

        public double VerificationProgress { get; set; }

        #endregion
    }

    public class DefaultFileDownloader : IFileDownloader
    {
        private const long BufferSize = 81920L;

        private readonly string _downloadRoot;

        public DefaultFileDownloader([NotNull] string downloadRoot)
        {
            _downloadRoot = downloadRoot;

            if (!Directory.Exists(downloadRoot))
            {
                Directory.CreateDirectory(downloadRoot);
            }
        }

        #region IFileDownloader Members

        public async Task<FileInfo> DownloadFileAsync(IFileInformation fileInfo, IProgress<IDownloadProgress> progressReporter,
            CancellationToken cancellationToken)
        {
            using (var client = new HttpClient())
            {
                foreach (var uri in fileInfo.DownloadUris)
                {
                    var progressStatus = new DefaultDownloadProgress {CurrentUri = uri, CurrentBytes = 0, Speed = 0, TotalBytes = 0};
                    progressReporter.Report(progressStatus);

                    using (var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            continue;
                        }

                        var outputFilePath = await DownloadFile(progressReporter, cancellationToken, response, progressStatus, uri);

                        if (fileInfo.FileVerifiers == null)
                        {
                            return new FileInfo(outputFilePath);
                        }

                        await VerifyDownloadedFile(fileInfo, progressReporter, cancellationToken, outputFilePath);

                        return new FileInfo(outputFilePath);
                    }
                }
            }

            throw new InvalidOperationException("All file downloads failed!");
        }

        #endregion

        [NotNull]
        private async Task<string> DownloadFile([NotNull] IProgress<IDownloadProgress> progressReporter, CancellationToken cancellationToken,
            [NotNull] HttpResponseMessage response, [NotNull] DefaultDownloadProgress progressStatus, [NotNull] Uri uri)
        {
            var buffer = new byte[BufferSize];
            var length = response.Content.Headers.ContentLength;

            long currentPosition = 0;
            progressStatus.CurrentBytes = currentPosition;

            progressStatus.TotalBytes = length.HasValue ? length.Value : 0;
            progressStatus.Speed = 0;

            var outputFilePath = GetFileOutputPath(Path.GetFileName(uri.LocalPath));
            progressReporter.Report(progressStatus);

            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));

            using (var fileStream = File.Open(outputFilePath, FileMode.Create, FileAccess.Write))
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    int bytesRead;
                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);

                        var elapsed = stopwatch.Elapsed.TotalSeconds;
                        stopwatch.Restart();

                        progressStatus.Speed = bytesRead / elapsed;

                        currentPosition += bytesRead;
                        progressStatus.CurrentBytes = currentPosition;

                        progressReporter.Report(progressStatus);
                    }
                }
            }

            return outputFilePath;
        }

        [NotNull]
        private static async Task VerifyDownloadedFile([NotNull] IFileInformation fileInfo, [NotNull] IProgress<IDownloadProgress> progressReporter,
            CancellationToken cancellationToken, [NotNull] string outputFilePath)
        {
            Debug.Assert(fileInfo.FileVerifiers != null);

            var verificationProgress = new DefaultDownloadProgress {VerifyingFile = true, VerificationProgress = 0.0};

            var verifierCountInv = 1.0 / fileInfo.FileVerifiers.Count();
            var completed = 0;
            foreach (var verifier in fileInfo.FileVerifiers)
            {
                var verificationHandler = new Progress<double>(p =>
                {
                    verificationProgress.VerificationProgress = completed * verifierCountInv + p;
                    progressReporter.Report(verificationProgress);
                });

                if (!await verifier.VerifyFilePathAsync(outputFilePath, cancellationToken, verificationHandler))
                {
                    throw new FileVerificationFailedException(outputFilePath);
                }

                ++completed;
            }
        }

        [NotNull]
        private string GetFileOutputPath([NotNull] string filename)
        {
            return Path.Combine(_downloadRoot, string.Format("{0:yyyy-MM-dd}", DateTime.Today), filename);
        }
    }
}
