#region Usings

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using UI.WPF.Launcher.Common.Services;

#endregion

namespace UI.WPF.Modules.Update.Services
{
    /// <summary>
    ///     IUpdateService implementation that can be used to test if the UI works correctly.
    /// </summary>
    public class DummyUpdateService : IUpdateService
    {
        #region IUpdateService Members

        public bool IsUpdatePossible
        {
            get { return true; }
        }

        public async Task<IUpdateStatus> CheckForUpdateAsync()
        {
            await Task.Delay(1000);

            return new UpdateStatus(new UpdateVersion(0, 1, 1, 0), true, false);
        }

        public async Task DoUpdateAsync(IProgress<IUpdateProgress> progressReporter)
        {
            progressReporter.Report(new UpdateProgress(0, 0, UpdateState.Preparing));

            await Task.Delay(5000);

            const long totalBytes = 10000000;
            progressReporter.Report(new UpdateProgress(0, totalBytes, UpdateState.Downloading));

            long current = 0;
            while (current < totalBytes)
            {
                await Task.Delay(100);

                current += 100000;

                progressReporter.Report(new UpdateProgress(current, totalBytes, UpdateState.Downloading));
            }

            progressReporter.Report(new UpdateProgress(0, totalBytes, UpdateState.Installing));

            await Task.Delay(10000);
        }

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
    }
}
