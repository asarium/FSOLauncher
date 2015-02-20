#region Usings

using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Implementations;
using ModInstallation.Implementations.DataClasses;
using ModInstallation.Implementations.Mods;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Tests.Util;
using Moq;
using NUnit.Framework;
using Semver;

#endregion

namespace ModInstallation.Tests.Implementations
{
    [TestFixture]
    public class DefaultPackageInstallerTests
    {
        private IDictionary<string, MockFileData> _fileDatas;

        [TestFixtureSetUp]
        public void Initialize()
        {
            _fileDatas = new Dictionary<string, MockFileData>
            {
                {@"C:\TestDir\mods\Test\1.2.3\mod.json", new MockFileData("")},
                {@"C:\TestDir\mods\Test\1.2.3\root.vp", new MockFileData("")},
                {@"C:\TestDir\mods\Test\1.2.3\assets.vp", new MockFileData("")},
                {@"C:\TestDir\mods\Test\1.2.3\data\test.txt", new MockFileData("")},
                {@"C:\TestDir\mods\Test\1.2.3\data\tables\abc.tbl", new MockFileData("")},
            };
        }

        [Test]
        public async Task TestUninstallPackageAsyncWholeMod()
        {
            var mockFileSystem = new MockFileSystem(_fileDatas);
            var instance = new DefaultPackageInstaller(new Mock<IFileDownloader>().Object, null, mockFileSystem)
            {
                InstallationDirectory = @"C:\TestDir"
            };

            var modMock = new Mock<IModification>();
            modMock.Setup(x => x.Title).Returns("TestMod");
            modMock.Setup(x => x.Id).Returns("test");
            modMock.Setup(x => x.FolderName).Returns("Test");
            modMock.Setup(x => x.Version).Returns(new SemVersion(1, 2, 3));

            var packageMock = new Mock<IPackage>();
            packageMock.Setup(x => x.ContainingModification).Returns(modMock.Object);
            packageMock.Setup(x => x.FileList).Returns((IEnumerable<IFileListItem>) null);

            await
                instance.UninstallPackageAsync(packageMock.Object,
                    true,
                    new Progress<IInstallationProgress>(p => Console.WriteLine(p.Message)),
                    CancellationToken.None).ConfigureAwait(false);

            Assert.IsFalse(mockFileSystem.FileExists(@"C:\TestDir\mods\Test\1.2.3\mod.json"));
            Assert.IsFalse(mockFileSystem.FileExists(@"C:\TestDir\mods\Test\1.2.3\root.vp"));
            Assert.IsFalse(mockFileSystem.FileExists(@"C:\TestDir\mods\Test\1.2.3\assets.vp"));
            Assert.IsFalse(mockFileSystem.FileExists(@"C:\TestDir\mods\Test\1.2.3\data\test.txt"));
            Assert.IsFalse(mockFileSystem.FileExists(@"C:\TestDir\mods\Test\1.2.3\data\tables\abc.tbl"));

            CollectionAssert.DoesNotContain(mockFileSystem.AllDirectories, @"C:\TestDir\mods\Test\1.2.3\");
        }

        [Test]
        public async Task TestUninstallPackageAsyncNoFilelist()
        {
            var mockFileSystem = new MockFileSystem(_fileDatas);
            var instance = new DefaultPackageInstaller(new Mock<IFileDownloader>().Object, null, mockFileSystem)
            {
                InstallationDirectory = @"C:\TestDir"
            };

            var modMock = new Mock<IModification>();
            modMock.Setup(x => x.Title).Returns("TestMod");
            modMock.Setup(x => x.Id).Returns("test");
            modMock.Setup(x => x.FolderName).Returns("Test");
            modMock.Setup(x => x.Version).Returns(new SemVersion(1, 2, 3));

            var packageMock = new Mock<IPackage>();
            packageMock.Setup(x => x.ContainingModification).Returns(modMock.Object);
            packageMock.Setup(x => x.FileList).Returns((IEnumerable<IFileListItem>) null);

            await
                AssertEx.ThrowsAsync<InvalidOperationException>(
                    () =>
                        instance.UninstallPackageAsync(packageMock.Object,
                            false,
                            new Progress<IInstallationProgress>(p => Console.WriteLine(p.Message)),
                            CancellationToken.None)).ConfigureAwait(false);

            Assert.IsTrue(mockFileSystem.FileExists(@"C:\TestDir\mods\Test\1.2.3\mod.json"));
            Assert.IsTrue(mockFileSystem.FileExists(@"C:\TestDir\mods\Test\1.2.3\root.vp"));
            Assert.IsTrue(mockFileSystem.FileExists(@"C:\TestDir\mods\Test\1.2.3\assets.vp"));
            Assert.IsTrue(mockFileSystem.FileExists(@"C:\TestDir\mods\Test\1.2.3\data\test.txt"));
            Assert.IsTrue(mockFileSystem.FileExists(@"C:\TestDir\mods\Test\1.2.3\data\tables\abc.tbl"));

            CollectionAssert.Contains(mockFileSystem.AllDirectories, @"C:\TestDir\mods\Test\1.2.3\");
        }

        [Test]
        public async Task TestUninstallPackageAsync()
        {
            var mockFileSystem = new MockFileSystem(_fileDatas);
            var instance = new DefaultPackageInstaller(new Mock<IFileDownloader>().Object, null, mockFileSystem)
            {
                InstallationDirectory = @"C:\TestDir"
            };

            var modMock = new Mock<IModification>();
            modMock.Setup(x => x.Title).Returns("TestMod");
            modMock.Setup(x => x.Id).Returns("test");
            modMock.Setup(x => x.FolderName).Returns("Test");
            modMock.Setup(x => x.Version).Returns(new SemVersion(1, 2, 3));

            var packageMock = new Mock<IPackage>();
            packageMock.Setup(x => x.ContainingModification).Returns(modMock.Object);
            packageMock.Setup(x => x.FileList).Returns(GetFileList());

            await
                instance.UninstallPackageAsync(packageMock.Object,
                    false,
                    new Progress<IInstallationProgress>(p => Console.WriteLine(p.Message)),
                    CancellationToken.None).ConfigureAwait(false);

            Assert.IsTrue(mockFileSystem.FileExists(@"C:\TestDir\mods\Test\1.2.3\mod.json"));
            Assert.IsFalse(mockFileSystem.FileExists(@"C:\TestDir\mods\Test\1.2.3\root.vp"));
            Assert.IsFalse(mockFileSystem.FileExists(@"C:\TestDir\mods\Test\1.2.3\assets.vp"));
            Assert.IsTrue(mockFileSystem.FileExists(@"C:\TestDir\mods\Test\1.2.3\data\test.txt"));
            Assert.IsTrue(mockFileSystem.FileExists(@"C:\TestDir\mods\Test\1.2.3\data\tables\abc.tbl"));

            CollectionAssert.Contains(mockFileSystem.AllDirectories, @"C:\TestDir\mods\Test\1.2.3\");
        }

        private static IEnumerable<IFileListItem> GetFileList()
        {
            yield return DefaultFileListItem.InitializeFromData(new FileListItem
            {
                archive = "test.zip",
                filename = "root.vp",
                orig_name = "root.vp"
            });

            yield return DefaultFileListItem.InitializeFromData(new FileListItem
            {
                archive = "test.zip",
                filename = "assets.vp",
                orig_name = "assets.vp"
            });
        }
    }
}
