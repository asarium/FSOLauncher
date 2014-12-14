#region Usings

using System;
using System.Threading;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using ModInstallation.Implementations;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels.Installation
{
    public class PackageInstallationItem : InstallationItem
    {
        private readonly ILocalModManager _modManager;

        private readonly IPackage _package;

        private readonly IPackageInstaller _packageInstaller;

        public PackageInstallationItem(IPackage package, IPackageInstaller packageInstaller, ILocalModManager modManager)
        {
            _package = package;
            _packageInstaller = packageInstaller;
            _modManager = modManager;
            Title = package.Name;
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
                var reporter = new Progress<IInstallationProgress>(ProgressHandler);

                try
                {
                    await _packageInstaller.InstallPackageAsync(_package, reporter, CancellationTokenSource.Token);

                    await _modManager.AddPackageAsync(_package);

                    OperationMessage = null;
                }
                catch (OperationCanceledException)
                {
                }
                catch (InvalidOperationException)
                {
                }

                if (CancellationTokenSource.IsCancellationRequested)
                {
                    // Report that this operation was canceled
                    ((IProgress<IInstallationProgress>) reporter).Report(new DefaultInstallationProgress
                    {
                        Message = "Canceled",
                        OverallProgress = 1.0,
                        SubProgress = 1.0
                    });
                }
                else
                {
                    // Report that this operation is finished
                    ((IProgress<IInstallationProgress>) reporter).Report(new DefaultInstallationProgress
                    {
                        Message = "Finished",
                        OverallProgress = 1.0,
                        SubProgress = 1.0
                    });
                }
            }
        }
    }
}
