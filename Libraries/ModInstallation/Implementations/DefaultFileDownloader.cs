#region Usings

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Exceptions;
using ModInstallation.Implementations.Util;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Interfaces.Net;
using Splat;

#endregion

namespace ModInstallation.Implementations
{
    [Export(typeof(IFileDownloader))]
    public class DefaultFileDownloader : IFileDownloader, IEnableLogger
    {
        private const long BufferSize = 64L * 1024L;

        private const int DefaultMaxConcurrentDownloads = 1;

        private SemaphoreSlim _downloadSemaphore;

        private readonly IWebClient _webclient;

        [ImportingConstructor]
        public DefaultFileDownloader(IWebClient webclient)
        {
            _webclient = webclient;
            MaxConcurrentDownloads = DefaultMaxConcurrentDownloads;
        }

        [NotNull]
        private SemaphoreSlim DownloadSemaphore
        {
            get { return _downloadSemaphore ?? (_downloadSemaphore = new SemaphoreSlim(MaxConcurrentDownloads)); }
        }

        [NotNull]
        private async Task<string> DownloadFile([NotNull] IProgress<IDownloadProgress> progressReporter,
            CancellationToken cancellationToken,
            [NotNull] Uri uri)
        {
            var outputFilePath = GetFileOutputPath(Path.GetFileName(uri.LocalPath));
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                long[] lastReport = {-1L};
                await _webclient.DownloadAsync(uri,
                    outputFilePath,
                    progress =>
                    {
                        var elapsed = stopwatch.Elapsed.TotalSeconds;

                        // Only update 10 times per second
                        if (elapsed < 0.1)
                        {
                            return;
                        }

                        stopwatch.Restart();

                        if (lastReport[0] < 0)
                        {
                            // First report
                            progressReporter.Report(DefaultDownloadProgress.Downloading(uri, progress.Current, progress.Total, 0.0));
                        }
                        else
                        {
                            progressReporter.Report(DefaultDownloadProgress.Downloading(uri,
                                progress.Current,
                                progress.Total,
                                (progress.Current - lastReport[0]) / elapsed));
                        }

                        lastReport[0] = progress.Current;
                    },
                    cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                var directoryName = Path.GetDirectoryName(outputFilePath);
                if (directoryName != null)
                {
                    Directory.CreateDirectory(directoryName);
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
        private static async Task VerifyDownloadedFile([NotNull] IFileInformation fileInfo,
            [NotNull] IProgress<IDownloadProgress> progressReporter,
            CancellationToken cancellationToken,
            [NotNull] string outputFilePath)
        {
            Debug.Assert(fileInfo.FileVerifiers != null);

            var verifierCountInv = 1.0 / fileInfo.FileVerifiers.Count();
            int[] completed = {0};
            foreach (var verifier in fileInfo.FileVerifiers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var verificationHandler =
                    new Progress<double>(p => progressReporter.Report(DefaultDownloadProgress.Verify(completed[0] * verifierCountInv + p)));

                if (!await verifier.VerifyFilePathAsync(outputFilePath, cancellationToken, verificationHandler))
                {
                    throw new FileVerificationFailedException(outputFilePath);
                }

                ++completed[0];
            }
        }

        [NotNull]
        private string GetFileOutputPath([NotNull] string filename)
        {
            return Path.Combine(DownloadDirectory, filename);
        }

        #region IFileDownloader Members

        public string DownloadDirectory { get; set; }

        public int MaxConcurrentDownloads { get; set; }

        public async Task<FileInfo> DownloadFileAsync(IFileInformation fileInfo,
            IProgress<IDownloadProgress> progressReporter,
            CancellationToken cancellationToken)
        {
            progressReporter.Report(DefaultDownloadProgress.Waiting());

            await DownloadSemaphore.WaitAsync(cancellationToken);

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
                        cancellationToken.ThrowIfCancellationRequested();

                        var outputFilePath = await DownloadFile(progressReporter, cancellationToken, uri);
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
                    catch (FileVerificationFailedException)
                    {
                        // Rethrow this exception
                        throw;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        // Ignore exception, probably a timeout...
                        this.Log().InfoException("Exception while downloading file", e);
                    }
                }

                throw new InvalidOperationException("All file downloads failed!");
            }
            finally
            {
                DownloadSemaphore.Release();
            }
        }

        #endregion
    }
}
