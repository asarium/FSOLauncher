﻿#region Usings

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

        [NotNull, Test]
        public async Task TestRetrieveRepositoryInformationAsync()
        {
            var repo = new TestRepository("Test");

            await repo.RetrieveRepositoryInformationAsync(new Progress<string>(), CancellationToken.None);

            var mods = repo.Modifications;

            Assert.IsNotNull(mods);
            Assert.AreEqual(1, mods.Count());

            var fsoMod = mods.First();
            Assert.AreEqual("mediavps", fsoMod.Id);
            Assert.IsTrue(fsoMod.Version == "2.0.1");
            Assert.AreEqual("MediaVPs", fsoMod.Title);
            Assert.AreEqual("MediaVPs modification", fsoMod.Description);
        }
    }
}