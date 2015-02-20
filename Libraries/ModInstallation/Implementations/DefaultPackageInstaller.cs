#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Implementations.Util;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Util;
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
        private readonly IArchiveExtractor _extractor;

        [NotNull]
        private readonly IFileDownloader _fileDownloader;

        private readonly IFileSystem _fileSystem;

        private string _installationDirectory;

        [ImportingConstructor]
        public DefaultPackageInstaller([NotNull] IFileDownloader downloader, [NotNull] IArchiveExtractor extractor, IFileSystem system)
        {
            _extractor = extractor;
            _fileDownloader = downloader;
            _fileSystem = system;
        }

        #region IPackageInstaller Members

        public string InstallationDirectory
        {
            get { return _installationDirectory; }
            set
            {
                _installationDirectory = value;
                _fileDownloader.DownloadDirectory = _fileSystem.Path.Combine(_installationDirectory, "downloads");
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
                await DownloadAndInstallFile(package, fileInformation, installationProgress, token).ConfigureAwait(false);

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
            await package.ContainingModification.PostInstallActions.ExecuteActionsAsync(GetInstallationDirectory(package)).ConfigureAwait(false);
        }

        public async Task UninstallPackageAsync(IPackage package,
            bool uninstallMod,
            IProgress<IInstallationProgress> progressReporter,
            CancellationToken token)
        {
            var modDirectory = GetInstallationDirectory(package);

            if (uninstallMod)
            {
                // Uninstall the whole mod
                progressReporter.Report(new DefaultInstallationProgress
                {
                    Message = "Searching for files...",
                    OverallProgress = -1.0,
                    SubProgress = -1.0
                });
                var files =
                    await
                        Task.Run(() => _fileSystem.Directory.EnumerateFiles(modDirectory, "*", SearchOption.AllDirectories).ToList(), token)
                            .ConfigureAwait(false);

                token.ThrowIfCancellationRequested();

                var current = 0;
                foreach (var file in files)
                {
                    var file1 = file;
                    var progress = (double) current / files.Count;
                    var fileName = file.Substring(modDirectory.Length + 1); // The path is prepended and there is a leading slash

                    progressReporter.Report(new DefaultInstallationProgress
                    {
                        Message = string.Format("Deleting {0}", fileName),
                        OverallProgress = progress,
                        SubProgress = -1.0
                    });

                    token.ThrowIfCancellationRequested();

                    await Task.Run(() => _fileSystem.File.Delete(file1), token).ConfigureAwait(false);

                    ++current;
                }

                // Now delete the directory
                progressReporter.Report(new DefaultInstallationProgress
                {
                    Message = string.Format("Deleting {0}", modDirectory),
                    OverallProgress = 1.0,
                    SubProgress = -1.0
                });
                await Task.Run(() => _fileSystem.Directory.Delete(modDirectory, true), token).ConfigureAwait(false);

                return;
            }

            if (package.FileList == null)
            {
                throw new InvalidOperationException("Insufficient file information!");
            }

            var fileList = package.FileList.ToList();

            token.ThrowIfCancellationRequested();

            var numDeleted = 0;
            // Now check the files of the package
            foreach (var fileInfo in fileList)
            {
                var file = _fileSystem.Path.Combine(modDirectory, fileInfo.Filename);
                progressReporter.Report(new DefaultInstallationProgress
                {
                    Message = string.Format("Deleting {0}", fileInfo.Filename),
                    OverallProgress = (double)numDeleted/fileList.Count,
                    SubProgress = -1.0
                });

                token.ThrowIfCancellationRequested();

                await Task.Run(() => _fileSystem.File.Delete(file), token).ConfigureAwait(false);

                ++numDeleted;
            }
        }

        #endregion

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

            var downloadedFile =
                await
                    _fileDownloader.DownloadFileAsync(fileInformation, new DownloadProgressTranslator(delegatingProgress), token)
                        .ConfigureAwait(false);

            delegatingProgress = new Progress<IInstallationProgress>(progress => progressReporter.Report(new DefaultInstallationProgress
            {
                Message = progress.Message,
                OverallProgress = 0.5 + progress.OverallProgress * 0.5,
                SubProgress = progress.SubProgress
            }));
            await InstallFile(package, downloadedFile, delegatingProgress, token).ConfigureAwait(false);
        }

        [NotNull]
        private async Task InstallFile([NotNull] IPackage package,
            [NotNull] FileInfoBase downloadedFile,
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
                        token).ConfigureAwait(false);
            }
            else
            {
                if (downloadedFile.DirectoryName != null && !_fileSystem.Directory.Exists(downloadedFile.DirectoryName))
                {
                    _fileSystem.Directory.CreateDirectory(downloadedFile.DirectoryName);
                }

                try
                {
                    var fileName = _fileSystem.Path.Combine(installationDirectory, downloadedFile.Name);
                    if (_fileSystem.File.Exists(fileName))
                    {
                        _fileSystem.File.Delete(fileName);
                    }
                    _fileSystem.File.Move(downloadedFile.FullName, fileName);
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
            return _fileSystem.Path.Combine(InstallationDirectory, "mods", package.ContainingModification.GetInstallationPath());
        }
    }
}
