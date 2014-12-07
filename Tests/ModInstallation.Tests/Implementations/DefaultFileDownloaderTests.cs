﻿#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Exceptions;
using ModInstallation.Implementations;
using ModInstallation.Implementations.Net;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Tests.TestClasses;
using ModInstallation.Tests.Util;
using ModInstallation.Util;
using Moq;
using NUnit.Framework;

#endregion

namespace ModInstallation.Tests.Implementations
{
    [TestFixture]
    public class DefaultFileDownloaderTests
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            var path = TestExtensions.GetTestDirectory();

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        #endregion

        private static readonly List<string> TestUrls = new List<string>
        {
            "http://swc.fs2downloads.com/builds/WIN/fs2_open_3.7.2_RC4_SSE.7z",
            "http://scp.fsmods.net/builds/WIN/fs2_open_3.7.2_RC4_SSE.7z"
        };

        [NotNull, Test]
        public async Task TestDownloadFileAsync()
        {
            var fileInfoMock = new Mock<IFileInformation>();
            fileInfoMock.Setup(x => x.DownloadUris).Returns(TestUrls.Select(x => new Uri(x)));

            var progressMock = new Mock<IProgress<IDownloadProgress>>();

            var instance = new DefaultFileDownloader
            {
                DownloadDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, TestContext.CurrentContext.Test.FullName)
            };

            var testClient = new TestWebClient();
            instance.WebClient = testClient;

            testClient.RespondWith("TestContent");

            var outFile = await instance.DownloadFileAsync(fileInfoMock.Object, new Progress<IDownloadProgress>(p =>
            {
                var remaining = p.TotalBytes - p.CurrentBytes;
                var remainingTime = TimeSpan.FromSeconds(remaining / p.Speed);

                var remainingTimeString = remainingTime.ToString(remainingTime.Hours > 0 ? "h\\:mm\\:ss" : "m\\:ss");

                var msg = string.Format("{0} of {1} ({2}/s) {3} remaining", p.CurrentBytes.HumanReadableByteCount(true),
                    p.TotalBytes.HumanReadableByteCount(true), ((long)p.Speed).HumanReadableByteCount(true), remainingTimeString);

                Console.WriteLine(msg);
            }), CancellationToken.None);

            await AssertEx.FileContent(outFile.FullName, "TestContent");

            // Verify that the connection status was fired
            progressMock.Verify(x => x.Report(It.Is<IDownloadProgress>(val => val.TotalBytes < 0)), Times.Once);

            // Verify that the progress status was fired
            progressMock.Verify(x => x.Report(It.Is<IDownloadProgress>(val => val.TotalBytes >= 0)), Times.AtLeast(1));
        }

        [NotNull, Test]
        public async Task TestDownloadFileAsyncErrors()
        {
            var fileInfoMock = new Mock<IFileInformation>();
            fileInfoMock.Setup(x => x.DownloadUris).Returns(TestUrls.Select(x => new Uri(x)));

            var progressMock = new Mock<IProgress<IDownloadProgress>>();

            var instance = new DefaultFileDownloader
            {
                DownloadDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, TestContext.CurrentContext.Test.FullName)
            };

            var testClient = new TestWebClient();
            instance.WebClient = testClient;

//            testClient.RespondWith(404, "Not found!").RespondWith(200, "TestContent");

            var outFile = await instance.DownloadFileAsync(fileInfoMock.Object, progressMock.Object, CancellationToken.None);

            await AssertEx.FileContent(outFile.FullName, "TestContent");

            // Verify that the connection status was fired
            progressMock.Verify(x => x.Report(It.Is<IDownloadProgress>(val => val.TotalBytes < 0)), Times.Exactly(2));

            // Verify that the progress status was fired
            progressMock.Verify(x => x.Report(It.Is<IDownloadProgress>(val => val.TotalBytes >= 0)), Times.AtLeast(1));
        }

        [NotNull, Test]
        public async Task TestDownloadFileAsyncTimeout()
        {
            var fileInfoMock = new Mock<IFileInformation>();
            fileInfoMock.Setup(x => x.DownloadUris).Returns(TestUrls.Select(x => new Uri(x)));

            var progressMock = new Mock<IProgress<IDownloadProgress>>();

            var instance = new DefaultFileDownloader
            {
                DownloadDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, TestContext.CurrentContext.Test.FullName)
            };

            var testClient = new TestWebClient();
            instance.WebClient = testClient;

            testClient.SimulateTimeout().SimulateTimeout();

            await
                AssertEx.ThrowsAsync<InvalidOperationException>(
                    async () => await instance.DownloadFileAsync(fileInfoMock.Object, progressMock.Object, CancellationToken.None));

            // Verify that the connection status was fired
            progressMock.Verify(x => x.Report(It.Is<IDownloadProgress>(val => val.TotalBytes < 0)), Times.Exactly(2));
        }

        [NotNull, Test]
        public async Task TestDownloadFileAsyncVerifyFailure()
        {
            var fileVerifierMock = new Mock<IFileVerifier>();
            fileVerifierMock.Setup(x => x.VerifyFilePathAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<IProgress<double>>()))
                .Returns(Task.FromResult(false));

            var fileInfoMock = new Mock<IFileInformation>();
            fileInfoMock.Setup(x => x.DownloadUris).Returns(TestUrls.Select(x => new Uri(x)));
            fileInfoMock.Setup(x => x.FileVerifiers).Returns(Enumerable.Repeat(fileVerifierMock.Object, 1));

            var progressMock = new Mock<IProgress<IDownloadProgress>>();

            var instance = new DefaultFileDownloader
            {
                DownloadDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, TestContext.CurrentContext.Test.FullName)
            };

            var testClient = new TestWebClient();
            instance.WebClient = testClient;

//            testClient.RespondWith(200, "TestContent");

            await
                AssertEx.ThrowsAsync<FileVerificationFailedException>(
                    () => instance.DownloadFileAsync(fileInfoMock.Object, progressMock.Object, CancellationToken.None));

            // Verify that the connection status was fired
            progressMock.Verify(x => x.Report(It.Is<IDownloadProgress>(val => val.TotalBytes < 0)), Times.Once);

            // Verify that the progress status was fired
            progressMock.Verify(x => x.Report(It.Is<IDownloadProgress>(val => val.TotalBytes >= 0)), Times.AtLeast(1));
        }

        [NotNull, Test]
        public async Task TestDownloadFileAsyncVerifySuccess()
        {
            var fileVerifierMock = new Mock<IFileVerifier>();
            fileVerifierMock.Setup(x => x.VerifyFilePathAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<IProgress<double>>()))
                .Returns(Task.FromResult(true));

            var fileInfoMock = new Mock<IFileInformation>();
            fileInfoMock.Setup(x => x.DownloadUris).Returns(TestUrls.Select(x => new Uri(x)));
            fileInfoMock.Setup(x => x.FileVerifiers).Returns(Enumerable.Repeat(fileVerifierMock.Object, 1));

            var progressMock = new Mock<IProgress<IDownloadProgress>>();

            var instance = new DefaultFileDownloader
            {
                DownloadDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, TestContext.CurrentContext.Test.FullName)
            };

            var testClient = new TestWebClient();
            instance.WebClient = testClient;

//            testClient.RespondWith(200, "TestContent");

            var outFile = await instance.DownloadFileAsync(fileInfoMock.Object, progressMock.Object, CancellationToken.None);

            await AssertEx.FileContent(outFile.FullName, "TestContent");


            // Verify that the connection status was fired
            progressMock.Verify(x => x.Report(It.Is<IDownloadProgress>(val => val.TotalBytes < 0)), Times.Once);

            // Verify that the progress status was fired
            progressMock.Verify(x => x.Report(It.Is<IDownloadProgress>(val => val.TotalBytes >= 0)), Times.AtLeast(1));
        }
    }
}
