using System;
using System.Threading;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using ModInstallation.Implementations;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using Splat;

namespace UI.WPF.Modules.Installation.ViewModels.Installation
{
    public class PackageUninstallationItem : InstallationItem
    {
        #region Overrides of InstallationItem

        private readonly IPackage _package;

        private readonly IPackageInstaller _installer;

        private readonly ILocalModManager _localModManager;

        public PackageUninstallationItem(IPackage package, IPackageInstaller installer, ILocalModManager localModManager)
        {
            _package = package;
            _installer = installer;
            _localModManager = localModManager;
        }

        private void ProgressHandler([NotNull] IInstallationProgress installationProgress)
        {
            OperationMessage = installationProgress.Message;
            Progress = installationProgress.OverallProgress;

            if (installationProgress.OverallProgress < 0.01 && installationProgress.SubProgress < 0.0)
            {
                // If the operations is just starting and we get an indeterminate sub progress then reflect that in the UI
                Indeterminate = true;
            }
            else
            {
                Indeterminate = false;
            }
        }

        public override async Task Install()
        {
            using (CancellationTokenSource = new CancellationTokenSource())
            {
                Result = InstallationResult.Pending;

                var reporter = new Progress<IInstallationProgress>(ProgressHandler);

                try
                {
                    await _localModManager.RemovePackageAsync(_package).ConfigureAwait(false);

                    await _installer.UninstallPackageAsync(_package, true, reporter, CancellationTokenSource.Token).ConfigureAwait(false);

                    OperationMessage = null;
                    Result = InstallationResult.Successful;
                }
                catch (OperationCanceledException)
                {
                    Result = InstallationResult.Canceled;
                }
                catch (Exception e)
                {
                    this.Log().WarnException("Uninstallation failed!", e);
                    Result = InstallationResult.Failed;
                    ResultMessage = e.Message;
                }

                if (CancellationTokenSource.IsCancellationRequested)
                {
                    // Report that this operation was canceled
                    ((IProgress<IInstallationProgress>)reporter).Report(new DefaultInstallationProgress
                    {
                        Message = "Canceled",
                        OverallProgress = 1.0,
                        SubProgress = 1.0
                    });
                }
                else
                {
                    // Report that this operation is finished
                    ((IProgress<IInstallationProgress>)reporter).Report(new DefaultInstallationProgress
                    {
                        Message = "Finished",
                        OverallProgress = 1.0,
                        SubProgress = 1.0
                    });
                }
            }
        }

        #endregion
    }
}