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
        string InstallationDirectory { get; set; }

        [NotNull]
        Task InstallPackageAsync([NotNull] IPackage package, [NotNull] IProgress<IInstallationProgress> progressReporter, CancellationToken token);

        /// <summary>
        ///     Uninstalles a package. This should delete the files associated with the specified package. If
        ///     <paramref name="uninstallMod" /> is <c>true</c> the mod associated with <paramref name="package" /> will be
        ///     uninstalled.
        /// </summary>
        /// <param name="package">The package to be uninstalled</param>
        /// <param name="uninstallMod"><c>true</c> if uninstallation should be forced</param>
        /// <param name="progressReporter">A progress reporter</param>
        /// <param name="token">Token to cancel the operation</param>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if <paramref name="package" /> has insufficient file informations and
        ///     <paramref name="uninstallMod" /> is <c>false</c>.
        /// </exception>
        /// <returns>An async Task</returns>
        Task UninstallPackageAsync(IPackage package, bool uninstallMod, IProgress<IInstallationProgress> progressReporter, CancellationToken token);
    }

    public interface IInstallationProgress
    {
        [NotNull]
        string Message { get; }

        double OverallProgress { get; }

        double SubProgress { get; }
    }
}
