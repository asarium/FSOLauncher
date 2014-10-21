#region Usings

using System.Threading.Tasks;
using FSOManagement.URLHandler.Interfaces;
using Launcher.Shared.Startup;
using Moq;
using NUnit.Framework;

#endregion

namespace Launcher.Shared.Tests.Startup
{
    [TestFixture]
    public class CommandlineHandlerTests
    {
        [Test]
        public void TestHandleCommandLineEmpty()
        {
            Assert.IsTrue(CommandlineHandler.HandleCommandLine(new string[0]));
        }

        [Test]
        public void TestHandleCommandLineInstallHandler()
        {
            var installerMock = new Mock<IProtocolInstaller>();
            installerMock.Setup(x => x.InstallHandlerAsync()).Returns(Task.FromResult<object>(null));

            CommandlineHandler.ProtocolInstaller = installerMock.Object;

            Assert.IsFalse(CommandlineHandler.HandleCommandLine(new[] {"--install-url-handler"}));

            installerMock.Verify(p => p.InstallHandlerAsync(), Times.Once);
        }

        [Test]
        public void TestHandleCommandLineInvalid()
        {
            Assert.IsTrue(CommandlineHandler.HandleCommandLine(new[] {"--invalid"}));
        }
    }
}
