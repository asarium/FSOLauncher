#region Usings

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO.Abstractions;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using FSOManagement.Annotations;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Net;
using ReactiveUI;

#endregion

namespace ModInstallation.Implementations
{
    [Export(typeof(IModInstallationManager))]
    public class ModinstallationManager : IModInstallationManager
    {
        private readonly DefaultFileDownloader _fileDownloader;

        private readonly IFileSystem _fileSystem;

        private readonly LocalModManager _localModManager;

        private readonly DefaultPackageInstaller _packageInstaller;

        private readonly RemoteModManager _remoteModManager;

        private string _rootPath;

        [ImportingConstructor]
        public ModinstallationManager(IFileSystem fs, IArchiveExtractor archiveExtractor, IWebClient webClient, ILocalModEnumerator localModEnumerator)
        {
            _fileSystem = fs;
            _localModManager = new LocalModManager(_fileSystem, localModEnumerator);
            _remoteModManager = new RemoteModManager();
            _fileDownloader = new DefaultFileDownloader(webClient);
            _packageInstaller = new DefaultPackageInstaller(_fileDownloader, archiveExtractor, _fileSystem);

            this.WhenAnyValue(x => x.RootPath).Where(x => x != null).Subscribe(path =>
            {
                _localModManager.PackageDirectory = _fileSystem.Path.Combine(path, "mods");
                _fileDownloader.DownloadDirectory = _fileSystem.Path.Combine(path, "downloads");
                _packageInstaller.InstallationDirectory = _fileSystem.Path.Combine(path, "mods");
            });
        }

        #region IModInstallationManager Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region Implementation of IModInstallationManager

        public string RootPath
        {
            get { return _rootPath; }
            set
            {
                if (value == _rootPath)
                {
                    return;
                }
                _rootPath = value;
                OnPropertyChanged();
            }
        }

        public ILocalModManager LocalModManager
        {
            get { return _localModManager; }
        }

        public IRemoteModManager RemoteModManager
        {
            get { return _remoteModManager; }
        }

        public IPackageInstaller PackageInstaller
        {
            get { return _packageInstaller; }
        }

        public IFileDownloader FileDownloader
        {
            get { return _fileDownloader; }
        }

        #endregion
    }
}
