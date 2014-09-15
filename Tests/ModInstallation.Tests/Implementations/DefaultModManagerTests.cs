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
            var modManager = new DefaultModManager();
            modManager.AddModRepository(new TestRepository("Test"));

            await modManager.RetrieveInformationAsync(new Progress<string>(), CancellationToken.None);

            Assert.IsNotNull(modManager.RemoteModifications);

            CollectionAssert.IsNotEmpty(modManager.RemoteModifications);
            Assert.AreEqual(1, modManager.RemoteModifications.Count());

            Assert.AreEqual("FSO", modManager.RemoteModifications.First().Id);
        }
    }
}
