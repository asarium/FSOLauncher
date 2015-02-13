#region Usings

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Interfaces;
using SharpCompress.Archive;
using SharpCompress.Common;

#endregion

namespace ModInstallation.Implementations.Extractors
{
    public class SharpCompressExtractor : IArchiveExtractor
    {
        #region IArchiveExtractor Members

        public Task ExtractArchiveAsync(string archiveFile, string destination, IProgress<IExtractionProgress> progress, CancellationToken token)
        {
            return Task.Run(() =>
            {
                using (var archive = ArchiveFactory.Open(new FileInfo(archiveFile)))
                {
                    long currentEntryTotal = 0;
                    var completed = 0;
                    var stepIncrement = 1.0 / archive.Entries.Count();
                    string currentFileName = null;

                    var lastProgress = 0.0;

                    archive.EntryExtractionBegin += (sender, args) =>
                    {
                        currentEntryTotal = args.Item.Size;
                        currentFileName = args.Item.FilePath;
                        lastProgress = 0.0;
                    };
                    archive.EntryExtractionEnd += (sender, args) => completed++;

                    archive.CompressedBytesRead += (sender, args) =>
                    {
                        var fileProgress = 0.0;
                        if (currentEntryTotal != 0)
                        {
                            fileProgress = args.CompressedBytesRead / (double) currentEntryTotal;
                        }

                        var progressVal = completed * stepIncrement + fileProgress * stepIncrement;

                        if (progressVal - lastProgress < 0.01)
                        {
                            return;
                        }

                        progress.Report(new DefaultExtractionProgress {FileName = currentFileName, OverallProgress = progressVal});

                        lastProgress = progressVal;
                    };

                    foreach (var entry in archive.Entries.Where(x => !x.IsDirectory))
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }

                        entry.WriteToDirectory(destination, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                    }
                }
            }, token);
        }

        public bool IsArchive(string filePath)
        {
            var ext = Path.GetExtension(filePath);

            return ext == "7z" || ext == "rar" || ext == "zip" || ext == "tar.gz";
        }

        #endregion
    }
}
