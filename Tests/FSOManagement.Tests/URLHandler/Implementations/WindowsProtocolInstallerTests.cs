using System.Threading.Tasks;
using FSOManagement.Annotations;
using FSOManagement.URLHandler.Implementations;
using NUnit.Framework;

namespace FSOManagement.Tests.URLHandler.Implementations
{
    [TestFixture, Platform("Win")]
    public class WindowsProtocolInstallerTests
    {
        [NotNull,Test]
        public async Task TestInstallHandlerAsync()
        {
            var installer = new WindowsProtocolInstaller();
            await installer.InstallHandlerAsync();
        }
    }
}