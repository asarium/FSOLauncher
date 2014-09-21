#region Usings

using System;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Interfaces.Mods;

#endregion

namespace ModInstallation.Interfaces
{
    public interface IPackageInstaller
    {
        [NotNull]
        Task InstallPackageAsync([NotNull] IPackage package, [NotNull] IProgress<IInstallationProgress> progressReporter, CancellationToken token);
    }

    public interface IInstallationProgress
    {
        [NotNull]
        string Message { get; }

        double OverallProgress { get; }

        double SubProgress { get; }
    }
}
