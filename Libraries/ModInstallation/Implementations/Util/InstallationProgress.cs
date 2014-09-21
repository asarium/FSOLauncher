using System;
using ModInstallation.Annotations;
using ModInstallation.Interfaces;

namespace ModInstallation.Implementations.Util
{
    public class InstallationProgress : IProgress<IInstallationProgress>
    {
        private readonly IProgress<IInstallationProgress> _mainProgress;

        public InstallationProgress([NotNull] IProgress<IInstallationProgress> mainProgress)
        {
            _mainProgress = mainProgress;
        }

        public int Completed { get; set; }

        public int Total { get; set; }

        #region IProgress<IInstallationProgress> Members

        public void Report([NotNull] IInstallationProgress value)
        {
            var singleIncrement = 1.0 / Total;

            var current = singleIncrement * Completed;

            _mainProgress.Report(new DefaultInstallationProgress
            {
                Message = value.Message,
                OverallProgress = current + singleIncrement * value.OverallProgress,
                SubProgress = value.SubProgress
            });
        }

        #endregion
    }
}