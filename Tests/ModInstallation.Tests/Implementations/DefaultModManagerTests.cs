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
            var modManager = new RemoteModManager
            {
                Repositories = new[] {new TestRepository("test://host")}
            };

            await modManager.GetModGroupsAsync(new Progress<string>(), false, CancellationToken.None).ConfigureAwait(false);

            Assert.IsNotNull(modManager.ModGroups);

            CollectionAssert.IsNotEmpty(modManager.ModGroups);
            Assert.AreEqual(1, modManager.ModGroups.Count());

            Assert.AreEqual("mediavps", modManager.ModGroups.First().Id);
        }
    }
}
