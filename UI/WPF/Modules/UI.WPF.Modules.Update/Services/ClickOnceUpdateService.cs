#region Usings

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Deployment.Application;
using System.Threading.Tasks;
using Caliburn.Micro;
using UI.WPF.Launcher.Common.Services;

#endregion

namespace UI.WPF.Modules.Update.Services
{
    [Export(typeof(IUpdateService))]
    public class ClickOnceUpdateService : PropertyChangedBase, IUpdateService
    {
        #region IUpdateService Members

        public bool IsUpdatePossible
        {
            get { return ApplicationDeployment.IsNetworkDeployed; }
        }

        public async Task<IUpdateStatus> CheckForUpdateAsync()
        {
            if (!IsUpdatePossible)
            {
                return new UpdateStatus(null);
            }

            var completionSource = new TaskCompletionSource<IUpdateStatus>();

            var checkHandler = new CheckForUpdateCompletedEventHandler((sender, args) =>
            {
                try
                {
                    if (args.Error != null)
                    {
                        completionSource.TrySetException(args.Error);
                    }
                    else
                    {
                        completionSource.TrySetResult(!args.UpdateAvailable ? new UpdateStatus(null) : new UpdateStatus(args));
                    }
                }
                catch (Exception exception)
                {
                    completionSource.TrySetException(exception);
                }
            });

            var deployment = ApplicationDeployment.CurrentDeployment;
            deployment.CheckForUpdateCompleted += checkHandler;

            deployment.CheckForUpdateAsync();

            var status = await completionSource.Task;
            deployment.CheckForUpdateCompleted -= checkHandler;

            return status;
        }

        public async Task DoUpdateAsync(IProgress<IUpdateProgress> progressReporter)
        {
            var deployment = ApplicationDeployment.CurrentDeployment;

            var source = new TaskCompletionSource<bool>();

            var completionListener = new AsyncCompletedEventHandler((sender, args) =>
            {
                if (args.Error != null)
                {
                    source.TrySetException(args.Error);
                }
                else
                {
                    source.TrySetResult(true);
                }
            });

            var progressListener = new DeploymentProgressChangedEventHandler((sender, args) =>
            {
                UpdateState state;
                switch (args.State)
                {
                    case DeploymentProgressState.DownloadingDeploymentInformation:
                        state = UpdateState.Preparing;
                        break;
                    case DeploymentProgressState.DownloadingApplicationInformation:
                        state = UpdateState.Preparing;
                        break;
                    case DeploymentProgressState.DownloadingApplicationFiles:
                        state = UpdateState.Downloading;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var total = args.BytesTotal;
                var current = args.BytesCompleted;

                if (args.State == DeploymentProgressState.DownloadingApplicationFiles && args.BytesTotal == args.BytesCompleted)
                {
                    state = UpdateState.Installing;
                }

                progressReporter.Report(new UpdateProgress(current, total, state));
            });

            deployment.UpdateCompleted += completionListener;
            deployment.UpdateProgressChanged += progressListener;

            deployment.UpdateAsync();
            await source.Task;

            deployment.UpdateCompleted -= completionListener;
            deployment.UpdateProgressChanged -= progressListener;
        }

        #endregion
    }
}
