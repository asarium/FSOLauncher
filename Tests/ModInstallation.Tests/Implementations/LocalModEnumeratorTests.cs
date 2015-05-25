using System.Collections.Generic;
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
using NUnit.Framework;
using Semver;

namespace ModInstallation.Tests.Implementations
{
    [TestFixture]
    public class LocalModEnumeratorTests
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
        public async Task TestFindMods()
        {
            var testInstance = new LocalModEnumerator(_fileSystem);

            var result = await testInstance.FindMods(@"C:\mods").ConfigureAwait(false);

            Assert.IsNotNull(result);

            {
                var mods = result.ToList();

                Assert.AreEqual(2, mods.Count);

                Assert.AreEqual("test1", mods[0].Id);
                CollectionAssert.IsNotEmpty(mods[0].Packages);

                Assert.AreEqual("test2", mods[1].Id);
                CollectionAssert.IsNotEmpty(mods[1].Packages);
            }
        }
    }
}