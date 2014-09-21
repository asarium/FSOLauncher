#region Usings

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Implementations.Extractors;
using ModInstallation.Interfaces;
using SevenZip;

#endregion

namespace ModInstallation.Windows.Implementations.Extractors
{
    public class SevenZipArchiveExtractor : IArchiveExtractor
    {
        public SevenZipArchiveExtractor()
        {
            var builder = new StringBuilder(255);
            var size = GetDllDirectory(builder.Capacity, builder);
            if (size > builder.Capacity)
            {
                size = builder.EnsureCapacity(size + 1);
                GetDllDirectory(builder.Capacity, builder);
            }

            var dir = builder.ToString(0, size);

            SevenZipBase.SetLibraryPath(Path.Combine(dir, IntPtr.Size == 8 ? "64" : "32", "7z.dll"));
        }

        #region IArchiveExtractor Members

        public async Task ExtractArchiveAsync(string archiveName, string destination, IProgress<IExtractionProgress> progress, CancellationToken token)
        {
            using (var tmp = new SevenZipExtractor(archiveName))
            {
                var tcs = new TaskCompletionSource<bool>();
                var extractVal = 0;
                string lastFileName = null;

                tmp.FileExtractionStarted += (sender, args) =>
                {
                    lastFileName = args.FileInfo.FileName;
                    args.Cancel = token.IsCancellationRequested;
                };

                tmp.Extracting += (sender, args) =>
                {
                    extractVal += args.PercentDelta;
                    progress.Report(new DefaultExtractionProgress {FileName = lastFileName ?? "", OverallProgress = extractVal / 100.0});
                    args.Cancel = token.IsCancellationRequested;
                };
                tmp.ExtractionFinished += (sender, args) => tcs.TrySetResult(true);

                tmp.BeginExtractArchive(destination);

                // await our own task, Yay!!
                await tcs.Task;
            }
        }

        public bool IsArchive(string filePath)
        {
            var ext = Path.GetExtension(filePath);

            return ext == "7z" || ext == "rar" || ext == "zip" || ext == "tar.gz";
        }

        #endregion

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetDllDirectory(int length, StringBuilder data);
    }
}
