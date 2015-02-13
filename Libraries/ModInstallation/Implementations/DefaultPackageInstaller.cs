﻿#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Implementations.Util;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using Splat;

#endregion

namespace ModInstallation.Implementations
{
    public class DefaultInstallationProgress : IInstallationProgress
    {
        #region IInstallationProgress Members

        public string Message { get; set; }

        public double OverallProgress { get; set; }

        public double SubProgress { get; set; }

        #endregion
    }

    [Export(typeof(IPackageInstaller))]
    public class DefaultPackageInstaller : IPackageInstaller, IEnableLogger
    {
        private string _installationDirectory;

        private readonly IArchiveExtractor _extractor;

        [NotNull]
        private readonly IFileDownloader _fileDownloader;

        [ImportingConstructor]
        public DefaultPackageInstaller([NotNull] IFileDownloader downloader, [NotNull] IArchiveExtractor extractor)
        {
            _extractor = extractor;
            _fileDownloader = downloader;
        }

        [NotNull]
        private async Task DownloadAndInstallFile([NotNull] IPackage package,
            [NotNull] IFileInformation fileInformation,
            [NotNull] IProgress<IInstallationProgress> progressReporter,
            CancellationToken token)
        {
            // Downloading is only a part of the installation process
            var delegatingProgress = new Progress<IInstallationProgress>(progress => progressReporter.Report(new DefaultInstallationProgress
            {
                Message = progress.Message,
                OverallProgress = progress.OverallProgress * 0.5,
                SubProgress = progress.SubProgress
            }));

            var downloadedFile = await _fileDownloader.DownloadFileAsync(fileInformation, new DownloadProgressTranslator(delegatingProgress), token);

            delegatingProgress = new Progress<IInstallationProgress>(progress => progressReporter.Report(new DefaultInstallationProgress
            {
                Message = progress.Message,
                OverallProgress = 0.5 + progress.OverallProgress * 0.5,
                SubProgress = progress.SubProgress
            }));
            await InstallFile(package, downloadedFile, delegatingProgress, token);
        }

        [NotNull]
        private async Task InstallFile([NotNull] IPackage package,
            [NotNull] FileInfo downloadedFile,
            [NotNull] IProgress<IInstallationProgress> delegatingProgress,
            CancellationToken token)
        {
            var installationDirectory = GetInstallationDirectory(package);

            token.ThrowIfCancellationRequested();

            if (_extractor.IsArchive(downloadedFile.FullName))
            {
                await
                    _extractor.ExtractArchiveAsync(downloadedFile.FullName,
                        installationDirectory,
                        new ExtractionProgressTranslator(delegatingProgress),
                        token);
            }
            else
            {
                if (downloadedFile.DirectoryName != null && !Directory.Exists(downloadedFile.DirectoryName))
                {
                    Directory.CreateDirectory(downloadedFile.DirectoryName);
                }

                try
                {
                    var fileName = Path.Combine(installationDirectory, downloadedFile.Name);
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                    File.Move(downloadedFile.FullName, fileName);
                }
                catch (IOException e)
                {
                    // Make sure not to crash the application if this fails
                    this.Log().WarnException("IO-Exception while moving downloaded file!", e);
                }
            }
        }

        [NotNull]
        private string GetInstallationDirectory([NotNull] IPackage package)
        {
            return Path.Combine(InstallationDirectory, "mods", package.ContainingModification.Id, package.ContainingModification.Version.ToString());
        }

        #region IPackageInstaller Members

        public string InstallationDirectory
        {
            get { return _installationDirectory; }
            set
            {
                _installationDirectory = value;
                _fileDownloader.DownloadDirectory = Path.Combine(_installationDirectory, "downloads");
            }
        }

        public async Task InstallPackageAsync(IPackage package, IProgress<IInstallationProgress> progressReporter, CancellationToken token)
        {
            progressReporter.Report(new DefaultInstallationProgress
            {
                Message = "Preparing installation...",
                OverallProgress = 0.0f,
                SubProgress = -1.0f
            });

            var fileList = package.Files as IList<IFileInformation> ?? package.Files.ToList();

            if (fileList.Count <= 0)
            {
                return;
            }

            var installationProgress = new InstallationProgress(progressReporter)
            {
                Total = fileList.Count
            };

            foreach (var fileInformation in fileList)
            {
                await DownloadAndInstallFile(package, fileInformation, installationProgress, token);

                ++installationProgress.Completed;
            }

            if (package.ContainingModification.PostInstallActions == null)
            {
                return;
            }

            progressReporter.Report(new DefaultInstallationProgress
            {
                Message = "Executing post install steps",
                OverallProgress = 1.0,
                SubProgress = -1.0f
            });

            // Now execute the post-install actions
            await package.ContainingModification.PostInstallActions.ExecuteActionsAsync(GetInstallationDirectory(package));
        }

        #endregion
    }
}
