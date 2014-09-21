#region Usings

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Implementations.Extractors;
using ModInstallation.Interfaces;
using ModInstallation.Tests.TestData;
using NUnit.Framework;

#endregion

namespace ModInstallation.Tests.Implementations.Extractors
{
    [TestFixture]
    public class SharpCompressExtractorTests
    {
        [NotNull, Test]
        public async Task TestExtractArchiveAsync()
        {
            var instance = new SharpCompressExtractor();

            var testArchive = await TestResourceUtil.ExtractTestResource("Test.lzma2.7z");
            var testFile = await TestResourceUtil.ExtractTestResource("Test.txt");

            var outDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, TestContext.CurrentContext.Test.Name, "out");

            await instance.ExtractArchiveAsync(testArchive.FullName, outDir, new Progress<IExtractionProgress>(), CancellationToken.None);

            FileAssert.AreEqual(testFile, new FileInfo(Path.Combine(outDir, testFile.Name)));
        }

        [NotNull, Test]
        public async Task TestExtractArchiveAsync2()
        {
            var instance = new SharpCompressExtractor();

            await
                instance.ExtractArchiveAsync(@"F:\Downloads\Spiele\FreeSpace\Exes\fs2_open_3.7.2_RC4_SSE.7z",
                    @"F:\Downloads\Spiele\FreeSpace\Exes\test",
                    new Progress<IExtractionProgress>(p => Console.WriteLine("{0}: {1}", p.FileName, p.OverallProgress)), CancellationToken.None);
        }
    }
}
