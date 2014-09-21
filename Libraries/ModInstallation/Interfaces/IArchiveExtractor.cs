#region Usings

using System;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;

#endregion

namespace ModInstallation.Interfaces
{
    public interface IExtractionProgress
    {
        [NotNull]
        string FileName { get; }

        double OverallProgress { get; }
    }

    public interface IArchiveExtractor
    {
        [NotNull]
        Task ExtractArchiveAsync([NotNull] string archiveFile, [NotNull] string destination, [NotNull] IProgress<IExtractionProgress> progress,
            CancellationToken token);

        bool IsArchive([NotNull] string filePath);
    }
}
