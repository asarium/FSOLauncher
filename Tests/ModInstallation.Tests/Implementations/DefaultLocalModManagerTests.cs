#region Usings

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Implementations;
using ModInstallation.Implementations.DataClasses;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Tests.Util;
using ModInstallation.Util;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Semver;

#endregion

namespace ModInstallation.Tests.Implementations
{
    [TestFixture]
    public class DefaultLocalModManagerTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _packageDirectory = TestExtensions.GetTestDirectory();

            // Clean the package directory
            if (Directory.Exists(_packageDirectory))
            {
                Directory.Delete(_packageDirectory, true);
            }

            Directory.CreateDirectory(_packageDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_packageDirectory))
            {
                Directory.Delete(_packageDirectory, true);
            }
        }

        #endregion

        private string _packageDirectory;

        [Test, NotNull]
        public async Task TestAddPackageAsync()
        {
            var testInstance = new DefaultLocalModManager
            {
                PackageDirectory = _packageDirectory
            };

            var modMock = new Mock<IModification>();
            var packageMock = new Mock<IPackage>();

            modMock.Setup(x => x.Id).Returns("test");
            modMock.Setup(x => x.Version).Returns(new SemVersion(1, 2, 3));
            modMock.Setup(x => x.Packages).Returns(packageMock.Object.AsEnumerable());

            packageMock.Setup(x => x.ContainingModification).Returns(modMock.Object);
            packageMock.Setup(x => x.Name).Returns("testPackage");

            await testInstance.AddPackageAsync(packageMock.Object);

            {
                var filePath = Path.Combine(_packageDirectory, "test", "1.2.3", "mod.json");

                Assert.IsTrue(File.Exists(filePath));

                var modification = JsonConvert.DeserializeObject<Modification>(File.ReadAllText(filePath));

                Assert.AreEqual("test", modification.id);
                Assert.AreEqual("1.2.3", modification.version);

                CollectionAssert.IsNotEmpty(modification.packages);
                Assert.AreEqual(1, modification.packages.Count());

                var first = modification.packages.First();
                Assert.AreEqual("testPackage", first.name);
                CollectionAssert.IsEmpty(first.dependencies);
            }

            CollectionAssert.IsNotEmpty(testInstance.Modifications);
        }
    }
}
