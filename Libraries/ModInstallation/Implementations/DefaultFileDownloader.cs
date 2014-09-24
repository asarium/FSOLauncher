#region Usings

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Exceptions;
using ModInstallation.Implementations.Util;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Interfaces.Net;

#endregion

namespace ModInstallation.Implementations
{
    [Export(typeof(IFileDownloader))]
    public class DefaultFileDownloader : IFileDownloader
    {
        private const long BufferSize = 81920L;

        private const int DefaultMaxConcurrentDownloads = 1;

        private SemaphoreSlim _downloadSemaphore;

        private int _maxConcurrentDownloads;

        public DefaultFileDownloader()
        {
            MaxConcurrentDownloads = DefaultMaxConcurrentDownloads;
        }

        [NotNull, Import]
        public IWebClient WebClient { private get; set; }

        #region IFileDownloader Members

        public string DownloadDirectory { get; set; }

        public int MaxConcurrentDownloads
        {
            get { return _maxConcurrentDownloads; }
            set
            {
                _maxConcurrentDownloads = value;

                if (_downloadSemaphore != null)
                {
                    _downloadSemaphore.Dispose();
                }

                _downloadSemaphore = new SemaphoreSlim(value);
            }
        }

        public async Task<FileInfo> DownloadFileAsync(IFileInformation fileInfo, IProgress<IDownloadProgress> progressReporter,
            CancellationToken cancellationToken)
        {
            progressReporter.Report(DefaultDownloadProgress.Waiting());

            await _downloadSemaphore.WaitAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            try
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
                        using (var response = await WebClient.GetAsync(uri, cancellationToken))
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var intStatus = (int) response.StatusCode;
                            if (intStatus < 200 || intStatus >= 300)
                            {
                                // Not successfull
                                continue;
                            }

                            var outputFilePath = await DownloadFile(progressReporter, cancellationToken, response, uri);
                            if (outputFilePath == null)
                            {
                                return null;
                            }

                            if (fileInfo.FileVerifiers == null)
                            {
                                return new FileInfo(outputFilePath);
                            }

                            try
                            {
                                await VerifyDownloadedFile(fileInfo, progressReporter, cancellationToken, outputFilePath);
                            }
                            catch (OperationCanceledException)
                            {
                                if (File.Exists(outputFilePath))
                                {
                                    File.Delete(outputFilePath);
                                }
                                throw;
                            }

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
            finally
            {
                _downloadSemaphore.Release();
            }
        }

        #endregion

        [NotNull]
        private async Task<string> DownloadFile([NotNull] IProgress<IDownloadProgress> progressReporter, CancellationToken cancellationToken,
            [NotNull] IResponse response, [NotNull] Uri uri)
        {
            var buffer = new byte[BufferSize];
            var length = response.Length;

            var currentPosition = 0L;
            var total = length.HasValue ? length.Value : 0;

            var outputFilePath = GetFileOutputPath(Path.GetFileName(uri.LocalPath));
            progressReporter.Report(DefaultDownloadProgress.Downloading(uri, currentPosition, total, 0.0));

            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using (var fileStream = File.Open(outputFilePath, FileMode.Create, FileAccess.Write))
                {
                    using (var stream = await response.OpenStreamAsync())
                    {
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        int bytesRead;
                        long lastReport = 0;
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);

                            cancellationToken.ThrowIfCancellationRequested();

                            currentPosition += bytesRead;

                            var elapsed = stopwatch.Elapsed.TotalSeconds;

                            // Only update 10 times per second
                            if (elapsed < 0.1)
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
            }
            catch (OperationCanceledException)
            {
                if (File.Exists(outputFilePath))
                {
                    File.Delete(outputFilePath); // Delete the downloaded file
                }

                throw;
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
                cancellationToken.ThrowIfCancellationRequested();

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
