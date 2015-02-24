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

            var result = await modManager.GetModGroupsAsync(new Progress<string>(), false, CancellationToken.None).ConfigureAwait(false);

            Assert.IsNotNull(result);

            CollectionAssert.IsNotEmpty(result);
            Assert.AreEqual(1, result.Count());

            Assert.AreEqual("mediavps", result.First().Id);
        }
    }
}
