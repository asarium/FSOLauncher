using System;
using ModInstallation.Annotations;
using ModInstallation.Interfaces;
using ModInstallation.Util;

namespace ModInstallation.Implementations.Util
{
    public class DownloadProgressTranslator : IProgress<IDownloadProgress>
    {
        private const double SmoothingFactor = 0.7;

        private readonly IProgress<IInstallationProgress> _outputProgress;

        private double _previousSpeed;

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

            var remaining = value.TotalBytes - value.CurrentBytes;
            var remainingTime = TimeSpan.FromSeconds(remaining / currentSpeed);

            var remainingTimeString = remainingTime.ToString(remainingTime.Hours > 0 ? "h\\:mm\\:ss" : "m\\:ss");

            var message = string.Format("{0} of {1} ({2}/s) {3} remaining", value.CurrentBytes.HumanReadableByteCount(true),
                value.TotalBytes.HumanReadableByteCount(true), ((long)currentSpeed).HumanReadableByteCount(true), remainingTimeString);

            var downloadProgress = (double) value.CurrentBytes / value.TotalBytes;

            _outputProgress.Report(new DefaultInstallationProgress
            {
                Message = message,
                OverallProgress = downloadProgress * downloadProgressPart,
                SubProgress = downloadProgress
            });
        }

        #endregion
    }
}