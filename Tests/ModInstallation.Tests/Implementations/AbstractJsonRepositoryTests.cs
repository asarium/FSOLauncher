#region Usings

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Tests.TestClasses;
using NUnit.Framework;

#endregion

namespace ModInstallation.Tests.Implementations
{
    public class AbstractJsonRepositoryTests
    {
        [Test]
        public void TestName()
        {
            var repo = new TestRepository("Test");
            Assert.AreEqual("Test", repo.Name);
        }

        [NotNull,Test]
        public async Task TestRetrieveRepositoryInformationAsync()
        {
            var repo = new TestRepository("Test");

            await repo.RetrieveRepositoryInformationAsync(new Progress<string>(), CancellationToken.None);

            var mods = repo.Modifications;

            Assert.IsNotNull(mods);
            Assert.AreEqual(1, mods.Count());

            var fsoMod = mods.First();
            Assert.AreEqual("FSO", fsoMod.Id);
            Assert.IsTrue(fsoMod.Version == "3.7.2-rc4");
            Assert.AreEqual("FreeSpace Open", fsoMod.Title);
            Assert.AreEqual("Recent builds of FreeSpace Open (the engine)", fsoMod.Description);

            Assert.AreEqual(4, fsoMod.Packages.Count());
        }
    }
}
