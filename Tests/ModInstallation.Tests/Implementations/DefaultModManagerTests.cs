#region Usings

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Implementations;
using ModInstallation.Tests.TestClasses;
using NUnit.Framework;

#endregion

namespace ModInstallation.Tests.Implementations
{
    [TestFixture]
    public class DefaultModManagerTests
    {
        [Test, NotNull]
        public async Task TestRetrieveInformationAsync()
        {
            var modManager = new DefaultRemoteModManager
            {
                Repositories = new[] {new TestRepository("test://host")}
            };

            await modManager.RetrieveInformationAsync(new Progress<string>(), CancellationToken.None);

            Assert.IsNotNull(modManager.ModificationGroups);

            CollectionAssert.IsNotEmpty(modManager.ModificationGroups);
            Assert.AreEqual(1, modManager.ModificationGroups.Count());

            Assert.AreEqual("mediavps", modManager.ModificationGroups.First().Id);
        }
    }
}
