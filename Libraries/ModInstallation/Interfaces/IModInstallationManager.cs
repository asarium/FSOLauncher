using System.ComponentModel;

namespace ModInstallation.Interfaces
{
    public interface IModInstallationManager : INotifyPropertyChanged
    {
        string RootPath { get; set; }

        ILocalModManager LocalModManager { get; }

        IRemoteModManager RemoteModManager { get; }

        IPackageInstaller PackageInstaller { get; }

        IFileDownloader FileDownloader { get; }
    }
}