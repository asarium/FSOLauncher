#region Usings

using FSOManagement;
using NUnit.Framework;
using UI.WPF.Modules.General.ViewModels.Internal;

#endregion

namespace UI.WPF.Modules.General.Tests.ViewModels.Internal
{
    [TestFixture]
    public class ExecutableViewModelTests
    {
        [Test]
        public void TestDisplayString()
        {
            var viewModel = new ExecutableViewModel();

            Assert.AreEqual("Unknown build", viewModel.DisplayString);

            viewModel.Debug = new Executable(@"C:\fred2_open_3_7_0-DEBUG");

            Assert.AreEqual("FRED2 Open 3.7.0 SSE2", viewModel.DisplayString);

            viewModel.Release = new Executable(@"C:\fred2_open_3_7_0");

            Assert.AreEqual("FRED2 Open 3.7.0 SSE2", viewModel.DisplayString);
        }

        [Test]
        public void TestHasBothVersions()
        {
            var viewModel = new ExecutableViewModel();

            Assert.IsFalse(viewModel.HasBothVersions);

            viewModel.Debug = new Executable(@"C:\fred2_open_3_7_0-DEBUG");

            Assert.IsFalse(viewModel.HasBothVersions);

            viewModel.Release = new Executable(@"C:\fred2_open_3_7_0");

            Assert.IsTrue(viewModel.HasBothVersions);
        }

        [Test]
        public void TestSelectedExecutable()
        {
            var viewModel = new ExecutableViewModel
            {
                Release = new Executable(@"C:\fred2_open_3_7_0"),
                Debug = new Executable(@"C:\fred2_open_3_7_0-DEBUG")
            };

            Assert.AreEqual(viewModel.Release, viewModel.SelectedExecutable);

            viewModel.ReleaseSelected = true;

            Assert.AreEqual(viewModel.Release, viewModel.SelectedExecutable);

            viewModel.ReleaseSelected = false;

            Assert.AreEqual(viewModel.Debug, viewModel.SelectedExecutable);
        }
    }
}
