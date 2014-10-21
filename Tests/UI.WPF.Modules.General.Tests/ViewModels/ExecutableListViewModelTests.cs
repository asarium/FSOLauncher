#region Usings

using System.Linq;
using Caliburn.Micro;
using FSOManagement;
using FSOManagement.Profiles;
using Moq;
using NUnit.Framework;
using ReactiveUI;
using UI.WPF.Modules.General.ViewModels;

#endregion

namespace UI.WPF.Modules.General.Tests.ViewModels
{
    [TestFixture]
    public class ExecutableListViewModelTests
    {
        [Test]
        public void TestExecutableViewModels()
        {
            var executables = new ReactiveList<Executable>{
                new Executable("/fs2_open_3_7_0.exe")
            };

            var exeManager = new Mock<ExecutableManager>();
            exeManager.Setup(x => x.Executables).Returns(executables);
            exeManager.Setup(x => x.StartFileSystemWatcher());
            exeManager.Setup(x => x.StopFileSystemWatcher());

            var tcMock = new Mock<TotalConversion>();
            tcMock.Setup(x => x.ExecutableManager).Returns(exeManager.Object);

            var testProfile = new Profile
            {
                SelectedTotalConversion = tcMock.Object,
                Name = "Test"
            };

            var tabViewModel = new ExecutableListViewModel(testProfile);

            Assert.AreEqual(1, tabViewModel.Executables.Count);
            var executableVm = tabViewModel.Executables.First();
            Assert.AreEqual(executables[0], executableVm.Release);

            executables.Add(new Executable("/fs2_open_3_7_0-DEBUG.exe"));

            Assert.AreEqual(1, tabViewModel.Executables.Count);
            executableVm = tabViewModel.Executables.First();
            Assert.AreEqual(executables[0], executableVm.Release);
            Assert.AreEqual(executables[1], executableVm.Debug);

            executables.Add(new Executable("/fs2_open_3_7_1_20140629_r10856.exe"));

            Assert.AreEqual(2, tabViewModel.Executables.Count);
            executableVm = tabViewModel.Executables[1];
            Assert.AreEqual(executables[2], executableVm.Release);
            Assert.IsNull(executableVm.Debug);

            executables.Remove(new Executable("/fs2_open_3_7_0-DEBUG.exe"));

            Assert.AreEqual(2, tabViewModel.Executables.Count);
            executableVm = tabViewModel.Executables[0];
            Assert.AreEqual(executables[0], executableVm.Release);
            Assert.IsNull(executableVm.Debug);

            executables.Clear();

            CollectionAssert.IsEmpty(tabViewModel.Executables);
        }

        [Test]
        public void TestSelectedExecutableChanged()
        {
            var executables = new ReactiveList<Executable>
            {
                new Executable("/fs2_open_3_7_0.exe"),
                new Executable("/fs2_open_3_7_0-DEBUG.exe"),
                new Executable("/fs2_open_3_7_1_20140629_r10856.exe")
            };

            var exeManager = new Mock<ExecutableManager>();
            exeManager.Setup(x => x.Executables).Returns(executables);
            exeManager.Setup(x => x.StartFileSystemWatcher());
            exeManager.Setup(x => x.StopFileSystemWatcher());

            var tcMock = new Mock<TotalConversion>();
            tcMock.Setup(x => x.ExecutableManager).Returns(exeManager.Object);

            var testProfile = new Profile
            {
                SelectedTotalConversion = tcMock.Object,
                Name = "Test"
            };

            var tabViewModel = new ExecutableListViewModel(testProfile);

            tabViewModel.SelectedExecutableViewModel = tabViewModel.Executables[0];
            tabViewModel.Executables[0].ReleaseSelected = true;
            Assert.AreSame(executables[0], testProfile.SelectedExecutable);

            tabViewModel.SelectedExecutableViewModel = tabViewModel.Executables[0];
            tabViewModel.Executables[0].ReleaseSelected = false;
            Assert.AreSame(executables[1], testProfile.SelectedExecutable);

            tabViewModel.SelectedExecutableViewModel = tabViewModel.Executables[1];
            tabViewModel.Executables[1].ReleaseSelected = false;
            Assert.IsNull(testProfile.SelectedExecutable);

            tabViewModel.SelectedExecutableViewModel = tabViewModel.Executables[1];
            tabViewModel.Executables[1].ReleaseSelected = true;
            Assert.AreSame(executables[2], testProfile.SelectedExecutable);
        }
    }
}
