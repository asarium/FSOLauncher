#region Usings

using System;
using ModInstallation.Annotations;
using ModInstallation.Interfaces;
using ModInstallation.Util;

#endregion

namespace ModInstallation.Implementations.Util
{
    public class DownloadProgressTranslator : IProgress<IDownloadProgress>
    {
        private const double SmoothingFactor = 0.02;

        private double _previousSpeed;

        private readonly IProgress<IInstallationProgress> _outputProgress;

        public DownloadProgressTranslator([NotNull] IProgress<IInstallationProgress> outputProgress)
        {
            _outputProgress = outputProgress;
        }

        #region IProgress<IDownloadProgress> Members

        public void Report([NotNull] IDownloadProgress value)
        {
            // This is how much of the download should take up in the overall progress
            const double downloadProgressPart = 0.8;
            const double verifyingProgressPart = 1 - downloadProgressPart;

            if (value.WaitingForSlot)
            {
                _outputProgress.Report(new DefaultInstallationProgress
                {
                    Message = "Waiting for download slot...",
                    OverallProgress = 0.0,
                    SubProgress = -1.0f
                });
                return;
            }

            if (value.VerifyingFile)
            {
                _outputProgress.Report(new DefaultInstallationProgress
                {
                    Message = "Verifying downloaded file...",
                    OverallProgress = downloadProgressPart + verifyingProgressPart * value.VerificationProgress,
                    SubProgress = value.VerificationProgress
                });
                return;
            }

            if (value.TotalBytes < 0)
            {
                _outputProgress.Report(new DefaultInstallationProgress
                {
                    Message = string.Format("Connecting to {0}", value.CurrentUri),
                    OverallProgress = 0.0f,
                    SubProgress = -1.0f
                });
                return;
            }

            var currentSpeed = SmoothingFactor * value.Speed + (1 - SmoothingFactor) * _previousSpeed;
            _previousSpeed = currentSpeed;

            string message;
            if (currentSpeed < 0.1)
            {
                message = string.Format("{0} of {1} (0B/s)",
                    value.CurrentBytes.HumanReadableByteCount(true),
                    value.TotalBytes.HumanReadableByteCount(true));
            }
            else
            {
                var remaining = value.TotalBytes - value.CurrentBytes;
                var remainingTime = TimeSpan.FromSeconds(remaining / currentSpeed);

                message = string.Format("{0} of {1} ({2}/s) {3} remaining",
                    value.CurrentBytes.HumanReadableByteCount(true),
                    value.TotalBytes.HumanReadableByteCount(true),
                    ((long) currentSpeed).HumanReadableByteCount(true),
                    GetRemainingTimeString(remainingTime));
            }

            var downloadProgress = (double) value.CurrentBytes / value.TotalBytes;

            _outputProgress.Report(new DefaultInstallationProgress
            {
                Message = message,
                OverallProgress = downloadProgress * downloadProgressPart,
                SubProgress = downloadProgress
            });
        }

        private static string GetRemainingTimeString(TimeSpan span)
        {
            if (span.Days > 0)
            {
                return string.Format("{0} day{1}", span.Days, span.Days == 1 ? "" : "s");
            }
            if (span.Hours > 0)
            {
                return string.Format("{0} hour{1}", span.Hours, span.Hours == 1 ? "" : "s");
            }
            if (span.Minutes > 0)
            {
                return string.Format("{0} minute{1}", span.Minutes, span.Minutes == 1 ? "" : "s");
            }
            if (span.Seconds < 10)
            {
                return "a few seconds";
            }

            return string.Format("{0} second{1}", span.Seconds, span.Seconds == 1 ? "" : "s");
        }

        #endregion
    }
}
