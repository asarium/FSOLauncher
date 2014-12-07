#region Usings

using System;
using ModInstallation.Implementations;
using ModInstallation.Implementations.Util;
using ModInstallation.Interfaces;
using Moq;
using NUnit.Framework;

#endregion

namespace ModInstallation.Tests.Implementations.Util
{
    [TestFixture]
    public class DownloadProgressTranslatorTests
    {
        [Test]
        public void TestReportConnecting()
        {
            var testUri = new Uri("http://swc.fs2downloads.com/builds/WIN/fs2_open_3.7.2_RC4_SSE.7z");

            IInstallationProgress testProgress = null;

            var mainProgressMock = new Mock<IProgress<IInstallationProgress>>();
            mainProgressMock.Setup(x => x.Report(It.IsAny<IInstallationProgress>())).Callback((IInstallationProgress p) => testProgress = p);

            var testInstance = new DownloadProgressTranslator(mainProgressMock.Object);

            testInstance.Report(DefaultDownloadProgress.Connecting(testUri));

            Assert.NotNull(testProgress);
            Assert.NotNull(testProgress.Message);
            StringAssert.Contains(testUri.ToString(), testProgress.Message);
            Assert.AreEqual(0.0, testProgress.OverallProgress, 0.001);
            Assert.AreEqual(-1.0, testProgress.SubProgress, 0.001);
        }
        [Test]
        public void TestReportWaiting()
        {
            IInstallationProgress testProgress = null;

            var mainProgressMock = new Mock<IProgress<IInstallationProgress>>();
            mainProgressMock.Setup(x => x.Report(It.IsAny<IInstallationProgress>())).Callback((IInstallationProgress p) => testProgress = p);

            var testInstance = new DownloadProgressTranslator(mainProgressMock.Object);

            testInstance.Report(DefaultDownloadProgress.Waiting());

            Assert.NotNull(testProgress);
            Assert.NotNull(testProgress.Message);
            Assert.AreEqual(0.0, testProgress.OverallProgress, 0.001);
            Assert.AreEqual(-1.0, testProgress.SubProgress, 0.001);
        }
        
        [Test]
        public void TestReportDownloading()
        {
            var testUri = new Uri("http://swc.fs2downloads.com/builds/WIN/fs2_open_3.7.2_RC4_SSE.7z");

            IInstallationProgress testProgress = null;

            var mainProgressMock = new Mock<IProgress<IInstallationProgress>>();
            mainProgressMock.Setup(x => x.Report(It.IsAny<IInstallationProgress>())).Callback((IInstallationProgress p) => testProgress = p);

            var testInstance = new DownloadProgressTranslator(mainProgressMock.Object);

            testInstance.Report(DefaultDownloadProgress.Downloading(testUri, 0, 1024 * 1024, 250));

            Assert.NotNull(testProgress);
            Assert.NotNull(testProgress.Message);
            Assert.AreEqual("0 B of 1,0 MB (5 B/s) 2 days remaining", testProgress.Message);
            Assert.AreEqual(0.0, testProgress.OverallProgress, 0.001);
            Assert.AreEqual(0.0, testProgress.SubProgress, 0.001);
            
            testInstance.Report(DefaultDownloadProgress.Downloading(testUri, 512 * 1024, 1024 * 1024, 2000));

            Assert.NotNull(testProgress);
            Assert.NotNull(testProgress.Message);
            Assert.AreEqual("524,3 kB of 1,0 MB (44 B/s) 3 hours remaining", testProgress.Message);
            Assert.AreEqual(0.4, testProgress.OverallProgress, 0.001);
            Assert.AreEqual(0.5, testProgress.SubProgress, 0.001);
        }

        [Test]
        public void TestReportVerify()
        {
            IInstallationProgress testProgress = null;

            var mainProgressMock = new Mock<IProgress<IInstallationProgress>>();
            mainProgressMock.Setup(x => x.Report(It.IsAny<IInstallationProgress>())).Callback((IInstallationProgress p) => testProgress = p);

            var testInstance = new DownloadProgressTranslator(mainProgressMock.Object);

            testInstance.Report(DefaultDownloadProgress.Verify(0.0));

            Assert.NotNull(testProgress);
            Assert.NotNull(testProgress.Message);
            Assert.AreEqual(0.8, testProgress.OverallProgress, 0.001);
            Assert.AreEqual(0.0, testProgress.SubProgress, 0.001);

            testInstance.Report(DefaultDownloadProgress.Verify(0.5));

            Assert.NotNull(testProgress);
            Assert.NotNull(testProgress.Message);
            Assert.AreEqual(0.9, testProgress.OverallProgress, 0.001);
            Assert.AreEqual(0.5, testProgress.SubProgress, 0.001);

            testInstance.Report(DefaultDownloadProgress.Verify(1.0));

            Assert.NotNull(testProgress);
            Assert.NotNull(testProgress.Message);
            Assert.AreEqual(1.0, testProgress.OverallProgress, 0.001);
            Assert.AreEqual(1.0, testProgress.SubProgress, 0.001);
        }
    }
}
