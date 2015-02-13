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
    public class InstallationProgressTests
    {
        [Test]
        public void TestReport()
        {
            IInstallationProgress testProgress = null;

            var mainProgressMock = new Mock<IProgress<IInstallationProgress>>();
            mainProgressMock.Setup(x => x.Report(It.IsAny<IInstallationProgress>())).Callback((IInstallationProgress p) => testProgress = p);

            var testInstance = new InstallationProgress(mainProgressMock.Object) {Completed = 0, Total = 2};

            testInstance.Report(new DefaultInstallationProgress {Message = "TestMessage", OverallProgress = 0.0, SubProgress = -1.0f});

            Assert.NotNull(testProgress);
            Assert.AreEqual("TestMessage", testProgress.Message);
            Assert.AreEqual(0.0, testProgress.OverallProgress, 0.001);
            Assert.AreEqual(-1.0, testProgress.SubProgress, 0.001);

            testInstance.Report(new DefaultInstallationProgress { Message = "TestMessage", OverallProgress = 0.5, SubProgress = 0.5 });

            Assert.NotNull(testProgress);
            Assert.AreEqual("TestMessage", testProgress.Message);
            Assert.AreEqual(0.25, testProgress.OverallProgress, 0.001);
            Assert.AreEqual(0.5, testProgress.SubProgress, 0.001);

            testInstance.Completed = 1;
            testInstance.Report(new DefaultInstallationProgress { Message = "TestMessage", OverallProgress = 0.5, SubProgress = 0.25 });

            Assert.NotNull(testProgress);
            Assert.AreEqual("TestMessage", testProgress.Message);
            Assert.AreEqual(0.75, testProgress.OverallProgress, 0.001);
            Assert.AreEqual(0.25, testProgress.SubProgress, 0.001);

            testInstance.Report(new DefaultInstallationProgress { Message = "TestMessage", OverallProgress = 1.0, SubProgress = 0.25 });

            Assert.NotNull(testProgress);
            Assert.AreEqual("TestMessage", testProgress.Message);
            Assert.AreEqual(1.0, testProgress.OverallProgress, 0.001);
            Assert.AreEqual(0.25, testProgress.SubProgress, 0.001);
        }
    }
}
