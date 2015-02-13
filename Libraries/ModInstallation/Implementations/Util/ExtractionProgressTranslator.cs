#region Usings

using System;
using ModInstallation.Annotations;
using ModInstallation.Interfaces;

#endregion

namespace ModInstallation.Implementations.Util
{
    public class ExtractionProgressTranslator : IProgress<IExtractionProgress>
    {
        private readonly IProgress<IInstallationProgress> _outProgress;

        public ExtractionProgressTranslator([NotNull] IProgress<IInstallationProgress> outProgress)
        {
            _outProgress = outProgress;
        }

        #region IProgress<IExtractionProgress> Members

        public void Report([NotNull] IExtractionProgress value)
        {
            _outProgress.Report(new DefaultInstallationProgress
            {
                Message = "Extracting " + value.FileName,
                OverallProgress = value.OverallProgress,
                SubProgress = value.OverallProgress
            });
        }

        #endregion
    }
}
