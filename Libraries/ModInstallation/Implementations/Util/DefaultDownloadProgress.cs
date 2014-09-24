#region Usings

using System;
using ModInstallation.Annotations;
using ModInstallation.Interfaces;

#endregion

namespace ModInstallation.Implementations.Util
{
    public class DefaultDownloadProgress : IDownloadProgress
    {
        private DefaultDownloadProgress()
        {
        }

        #region IDownloadProgress Members

        public Uri CurrentUri { get; private set; }

        public long CurrentBytes { get; private set; }

        public long TotalBytes { get; private set; }

        public double Speed { get; private set; }

        public bool VerifyingFile { get; private set; }

        public double VerificationProgress { get; private set; }

        public bool WaitingForSlot { get; private set; }

        #endregion

        [NotNull]
        public static DefaultDownloadProgress Connecting([NotNull] Uri uri)
        {
            return new DefaultDownloadProgress {CurrentUri = uri, TotalBytes = -1};
        }

        [NotNull]
        public static DefaultDownloadProgress Waiting()
        {
            return new DefaultDownloadProgress {WaitingForSlot = true};
        }

        [NotNull]
        public static DefaultDownloadProgress Downloading([NotNull] Uri uri, long current, long total, double speed)
        {
            return new DefaultDownloadProgress {CurrentUri = uri, TotalBytes = total, CurrentBytes = current, Speed = speed};
        }

        [NotNull]
        public static DefaultDownloadProgress Verify(double progress)
        {
            return new DefaultDownloadProgress {VerifyingFile = true, VerificationProgress = progress};
        }
    }
}
