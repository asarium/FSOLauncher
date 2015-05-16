#region Usings

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Implementations;
using ModInstallation.Implementations.DataClasses;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Tests.TestData;
using ModInstallation.Util;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Semver;

#endregion

namespace ModInstallation.Tests.Implementations
{
    [TestFixture]
    public class LocalModManagerTests
    {
        private MockFileSystem _fileSystem;

        [SetUp]
        public void Setup()
        {
            var files = new Dictionary<string, MockFileData>
            {
                {
                    @"C:\mods\test1\1.0.0\mod.json",
                    new MockFileData(TestResourceUtil.GetTestResource("ModManager.mod1.json"))
                },
                {
                    @"C:\mods\test2\1.0.0\mod.json",
                    new MockFileData(TestResourceUtil.GetTestResource("ModManager.mod2.json"))
                }
            };

            _fileSystem = new MockFileSystem(files);
        }

        [Test, NotNull]
        public async Task TestAddPackageAsync()
        {
            var testInstance = new LocalModManager(_fileSystem)
            {
                PackageDirectory = @"C:\mods"
            };

            var modMock = new Mock<IModification>();
            var packageMock = new Mock<IPackage>();
            {

                modMock.Setup(x => x.Id).Returns("test");
                modMock.Setup(x => x.Version).Returns(new SemVersion(1, 2, 3));

                packageMock.Setup(x => x.ContainingModification).Returns(modMock.Object);
                packageMock.Setup(x => x.Name).Returns("testPackage");

                modMock.Setup(x => x.Packages).Returns(packageMock.Object.AsEnumerable());
            }

            await testInstance.AddPackageAsync(packageMock.Object).ConfigureAwait(false);

            {
                Assert.IsTrue(_fileSystem.FileExists(@"C:\mods\test\1.2.3\mod.json"));

                var modData = await _fileSystem.ParseJSONFile<Modification>(@"C:\mods\test\1.2.3\mod.json");
                Assert.AreEqual("test", modData.id);
                Assert.AreEqual("1.2.3", modData.version);
                Assert.AreEqual("1.2.3", modData.version);

                CollectionAssert.IsNotEmpty(modData.packages);

                Assert.AreEqual(1, modData.packages.Count());
                var package = modData.packages.First();

                Assert.AreEqual("testPackage", package.name);

                CollectionAssert.IsNotEmpty(testInstance.Modifications);
            }
        }

        [Test, NotNull]
        public async Task TestParseLocalModDataAsync()
        {
            var testInstance = new LocalModManager(_fileSystem)
            {
                PackageDirectory = @"C:\mods"
            };

            await testInstance.ParseLocalModDataAsync().ConfigureAwait(false);

            Assert.IsNotNull(testInstance.Modifications);

            {
                var mods = testInstance.Modifications.ToList();

                Assert.AreEqual(2, mods.Count);

                Assert.AreEqual("test1", mods[0].Id);
                CollectionAssert.IsNotEmpty(mods[0].Packages);

                Assert.AreEqual("test2", mods[1].Id);
                CollectionAssert.IsNotEmpty(mods[1].Packages);
            }
        }

        [Test, NotNull]
        public async Task TestRemovePackageAsync()
        {
            var testInstance = new LocalModManager(_fileSystem)
            {
                PackageDirectory = @"C:\mods"
            };

            await testInstance.ParseLocalModDataAsync().ConfigureAwait(false);

            Assert.IsNotNull(testInstance.Modifications);

            var package = testInstance.Modifications.First(x => x.Id == "test1").Packages.First();

            await testInstance.RemovePackageAsync(package).ConfigureAwait(false);

            {
                var mods = testInstance.Modifications.ToList();

                Assert.AreEqual(1, mods.Count);

                Assert.AreEqual("test2", mods[0].Id);
                CollectionAssert.IsNotEmpty(mods[0].Packages);

                Assert.IsFalse(_fileSystem.FileExists(@"C:\mods\test1\1.0.0\mod.json"));
            }
        }
    }
}
