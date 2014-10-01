using System.Threading.Tasks;
using NUnit.Framework;
using UI.WPF.Modules.Update.Services;

namespace UI.WPF.Modules.Update.Tests.Services
{
    [TestFixture]
    public class SquirrelUpdateServiceTests
    {
        [Test]
        public void TestIsUpdatePossible()
        {
            var instance = new SquirrelUpdateService();

            Assert.IsFalse(instance.IsUpdatePossible);
        }

        [Test]
        public async Task TestCheckForUpdateAsync()
        {
            var instance = new SquirrelUpdateService();

            await instance.CheckForUpdateAsync();
        }
    }
}