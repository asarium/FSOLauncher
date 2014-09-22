#region Usings

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using ModInstallation.Annotations;
using ModInstallation.Exceptions;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;

#endregion

namespace ModInstallation.Implementations
{
    public class DefaultDownloadProgress : IDownloadProgress
    {
        private DefaultDownloadProgress()
        {
        }

        #region IDownloadProgress Members

        public Uri CurrentUri { get; private set; }

        public long CurrentBytes { get; private set; }

        public long TotalBytes { get; private set; }

        public double Speed { get; private set; }

        public bool VerifyingFile { get; private set; }

        public double VerificationProgress { get; private set; }

        #endregion

        [NotNull]
        public static DefaultDownloadProgress Connecting([NotNull] Uri uri)
        {
            return new DefaultDownloadProgress {CurrentUri = uri, TotalBytes = -1};
        }

        [NotNull]
        public static DefaultDownloadProgress Downloading([NotNull] Uri uri, long current, long total, double speed)
        {
            return new DefaultDownloadProgress {CurrentUri = uri, TotalBytes = total, CurrentBytes = current, Speed = speed};
        }

        [NotNull]
        public static DefaultDownloadProgress Verify(double progress)
        {
            return new DefaultDownloadProgress {VerifyingFile = true, VerificationProgress = progress};
        }
    }

    [Export(typeof(IFileDownloader))]
    public class DefaultFileDownloader : IFileDownloader
    {
        private const long BufferSize = 81920L;

        #region IFileDownloader Members

        public string DownloadDirectory { get; set; }

        public async Task<FileInfo> DownloadFileAsync(IFileInformation fileInfo, IProgress<IDownloadProgress> progressReporter,
            CancellationToken cancellationToken)
        {
            if (!Directory.Exists(DownloadDirectory))
            {
                Directory.CreateDirectory(DownloadDirectory);
            }

            foreach (var uri in fileInfo.DownloadUris)
            {
                progressReporter.Report(DefaultDownloadProgress.Connecting(uri));

                try
                {
                    var flurlClient = new FlurlClient(uri.AbsoluteUri);
                    using (
                        var response =
                            await flurlClient.HttpClient.GetAsync(uri.AbsoluteUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return null;
                        }

                        if (!response.IsSuccessStatusCode)
                        {
                            continue;
                        }

                        var outputFilePath = await DownloadFile(progressReporter, cancellationToken, response, uri);

                        if (fileInfo.FileVerifiers == null)
                        {
                            return new FileInfo(outputFilePath);
                        }

                        await VerifyDownloadedFile(fileInfo, progressReporter, cancellationToken, outputFilePath);

                        return new FileInfo(outputFilePath);
                    }
                }
                catch (HttpRequestException)
                {
                    // Ignore exception, probably a timeout...
                }
            }

            throw new InvalidOperationException("All file downloads failed!");
        }

        #endregion

        [NotNull]
        private async Task<string> DownloadFile([NotNull] IProgress<IDownloadProgress> progressReporter, CancellationToken cancellationToken,
            [NotNull] HttpResponseMessage response, [NotNull] Uri uri)
        {
            var buffer = new byte[BufferSize];
            var length = response.Content.Headers.ContentLength;

            var currentPosition = 0L;
            var total = length.HasValue ? length.Value : 0;

            var outputFilePath = GetFileOutputPath(Path.GetFileName(uri.LocalPath));
            progressReporter.Report(DefaultDownloadProgress.Downloading(uri, currentPosition, total, 0.0));

            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));

            if (cancellationToken.IsCancellationRequested)
            {
                return outputFilePath;
            }

            using (var fileStream = File.Open(outputFilePath, FileMode.Create, FileAccess.Write))
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    int bytesRead;
                    long lastReport = 0;
                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);

                        if (cancellationToken.IsCancellationRequested)
                        {
                            return outputFilePath;
                        }

                        currentPosition += bytesRead;

                        var elapsed = stopwatch.Elapsed.TotalSeconds;

                        if (!(elapsed > 0.1))
                        {
                            continue;
                        }

                        stopwatch.Restart();

                        progressReporter.Report(DefaultDownloadProgress.Downloading(uri, currentPosition, total,
                            (currentPosition - lastReport) / elapsed));

                        lastReport = currentPosition;
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

            var verifierCountInv = 1.0 / fileInfo.FileVerifiers.Count();
            var completed = 0;
            foreach (var verifier in fileInfo.FileVerifiers)
            {
                var verificationHandler =
                    new Progress<double>(p => progressReporter.Report(DefaultDownloadProgress.Verify(completed * verifierCountInv + p)));

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
            return Path.Combine(DownloadDirectory, filename);
        }
    }
}
