using UI.WPF.Launcher.Common.Services;

namespace UI.WPF.Modules.Update.Services
{
    internal class UpdateProgress : IUpdateProgress
    {
        public UpdateProgress(long currentBytes, long totalBytes, UpdateState state)
        {
            CurrentBytes = currentBytes;
            TotalBytes = totalBytes;
            State = state;
        }

        #region IUpdateProgress Members

        public long TotalBytes { get; private set; }

        public long CurrentBytes { get; private set; }

        public UpdateState State { get; private set; }

        #endregion
    }
}