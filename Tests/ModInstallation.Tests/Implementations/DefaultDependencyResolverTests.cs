#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Implementations;
using ModInstallation.Implementations.DataClasses;
using ModInstallation.Implementations.Mods;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Tests.TestData;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Semver;

#endregion

namespace ModInstallation.Tests.Implementations
{
    [TestFixture]
    public class DefaultDependencyResolverTests
    {
        [NotNull]
        private static async Task<IList<IModification>> GenerateTestMods([NotNull] string name)
        {
            var testData = await TestResourceUtil.GetTestResource("Dependencies." + name);

            var jsonData = JsonConvert.DeserializeObject<Repository>(testData);

            if (jsonData.mods != null)
            {
                return jsonData.mods.Select(mod => DefaultModification.InitializeFromData(mod)).Cast<IModification>().ToList();
            }

            throw new InvalidOperationException();
        }

        [NotNull, Test]
        public async Task TestGetPackageDependenciesErrors()
        {
            var testData = await GenerateTestMods("data4.json");

            CollectionAssert.IsNotEmpty(testData);
            CollectionAssert.IsNotEmpty(testData.First().Packages);

            var firstPackage = testData.First().Packages.First();

            var numCalls = 0;
            var errorHandler = new ErrorHandler((context, message) =>
            {
                numCalls++;
                return false;
            });

            var dependPackages = DefaultDependencyResolver.GetPackageDependencies(firstPackage, testData, errorHandler).ToList();

            Assert.AreEqual(2, numCalls);

            CollectionAssert.IsEmpty(dependPackages);
        }

        [NotNull, Test]
        public async Task TestGetPackageDependenciesMultipleVersions()
        {
            var testData = await GenerateTestMods("data2.json");

            CollectionAssert.IsNotEmpty(testData);
            CollectionAssert.IsNotEmpty(testData.First().Packages);

            var firstPackage = testData.First().Packages.First();

            var dependPackages = DefaultDependencyResolver.GetPackageDependencies(firstPackage, testData, null).ToList();

            CollectionAssert.IsNotEmpty(dependPackages);
            Assert.AreEqual(1, dependPackages.Count());

            var dependency = dependPackages.First();
            Assert.AreEqual("package4", dependency.Name);
            Assert.AreEqual(new SemVersion(2), dependency.ContainingModification.Version);
        }

        [Test]
        public void TestGetPackageDependenciesNoDependencies()
        {
            var packageMock = new Mock<IPackage>();
            packageMock.Setup(x => x.Dependencies).Returns(Enumerable.Empty<IModDependency>());
            CollectionAssert.IsEmpty(DefaultDependencyResolver.GetPackageDependencies(packageMock.Object, Enumerable.Empty<IModification>().ToList(),
                null));

            packageMock.Setup(x => x.Dependencies).Returns((IEnumerable<IModDependency>) null);
            CollectionAssert.IsEmpty(DefaultDependencyResolver.GetPackageDependencies(packageMock.Object, Enumerable.Empty<IModification>().ToList(),
                null));
        }

        [NotNull, Test]
        public async Task TestGetPackageDependenciesRequiredPackages()
        {
            var testData = await GenerateTestMods("data3.json");

            CollectionAssert.IsNotEmpty(testData);
            CollectionAssert.IsNotEmpty(testData.First().Packages);

            var firstPackage = testData.First().Packages.First();

            var dependPackages = DefaultDependencyResolver.GetPackageDependencies(firstPackage, testData, null).ToList();

            CollectionAssert.IsNotEmpty(dependPackages);
            Assert.AreEqual(2, dependPackages.Count());

            var dependency = dependPackages.First();
            Assert.AreEqual("test2", dependency.ContainingModification.Id);
            Assert.AreEqual("package1", dependency.Name);

            dependency = dependPackages.Skip(1).First();
            Assert.AreEqual("test2", dependency.ContainingModification.Id);
            Assert.AreEqual("package2", dependency.Name);
        }

        [NotNull, Test]
        public async Task TestGetPackageDependenciesSingle()
        {
            var testData = await GenerateTestMods("data1.json");

            CollectionAssert.IsNotEmpty(testData);
            CollectionAssert.IsNotEmpty(testData.First().Packages);

            var firstPackage = testData.First().Packages.First();

            var dependPackages = DefaultDependencyResolver.GetPackageDependencies(firstPackage, testData, null).ToList();

            CollectionAssert.IsNotEmpty(dependPackages);
            Assert.AreEqual(1, dependPackages.Count());

            var dependency = dependPackages.First();
            Assert.AreEqual("package2", dependency.Name);
        }

        [NotNull, Test]
        public async Task TestResolveDependencies()
        {
            var testData = await GenerateTestMods("data5.json");

            var testPackage = testData.First().Packages.First();

            var dependencyResolver = new DefaultDependencyResolver();
            var result = dependencyResolver.ResolveDependencies(testPackage, testData, null);

            var resultList = result as IList<IPackage> ?? result.ToList();

            CollectionAssert.IsNotEmpty(resultList);
            Assert.AreEqual(4, resultList.Count);

            Assert.AreEqual("test1", resultList[0].ContainingModification.Id);
            Assert.AreEqual("package3", resultList[0].Name);

            Assert.AreEqual("test2", resultList[1].ContainingModification.Id);
            Assert.AreEqual("package1", resultList[1].Name);

            Assert.AreEqual("test2", resultList[2].ContainingModification.Id);
            Assert.AreEqual("package2", resultList[2].Name);

            Assert.AreEqual("test3", resultList[3].ContainingModification.Id);
            Assert.AreEqual("package1", resultList[3].Name);
        }

        [NotNull, Test]
        public async Task TestResolveDependenciesCyclic()
        {
            var testData = await GenerateTestMods("data6.json");

            var testPackage = testData.First().Packages.First();

            var dependencyResolver = new DefaultDependencyResolver();
            Assert.Throws<InvalidOperationException>(() => dependencyResolver.ResolveDependencies(testPackage, testData, null));
        }
    }
}
