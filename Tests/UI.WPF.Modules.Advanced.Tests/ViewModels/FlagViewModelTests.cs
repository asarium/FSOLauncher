#region Usings

using FSOManagement;
using FSOManagement.Interfaces;
using FSOManagement.Profiles;
using Moq;
using NUnit.Framework;
using UI.WPF.Modules.Advanced.ViewModels;

#endregion

namespace UI.WPF.Modules.Advanced.Tests.ViewModels
{
    [TestFixture]
    public class FlagViewModelTests
    {
        [Test]
        public void TestDisplayString()
        {
            {
                var viewModel = new FlagViewModel(new Flag("TestTest", false, "Test", 0, 0, null, null), null);

                Assert.AreEqual("TestTest", viewModel.DisplayString);
            }
            {
                var viewModel = new FlagViewModel(new Flag(null, false, "Test", 0, 0, null, null), null);

                Assert.AreEqual("Test", viewModel.DisplayString);
            }
        }

        [Test]
        public void TestEnabled()
        {
            {
                var flagManagerMock = new Mock<IFlagManager>();
                flagManagerMock.Setup(x => x.SetFlag(It.IsAny<string>(), true)).Verifiable();

                new FlagViewModel(new Flag(null, false, null, 0, 0, "Test", null), flagManagerMock.Object) {Enabled = true};

                flagManagerMock.Verify();
            }
            {
                var flagManagerMock = new Mock<IFlagManager>();
                flagManagerMock.Setup(x => x.SetFlag(It.IsAny<string>(), false)).Verifiable();

                var viewModel = new FlagViewModel(new Flag(null, false, null, 0, 0, "Test", null), flagManagerMock.Object) {Enabled = true};

                viewModel.Enabled = false;

                flagManagerMock.Verify();
            }
        }

        [Test]
        public void TestType()
        {
            var viewModel = new FlagViewModel(new Flag(null, false, null, 0, 0, "Test", null), null);

            Assert.AreEqual("Test", viewModel.Type);
        }
    }
}
