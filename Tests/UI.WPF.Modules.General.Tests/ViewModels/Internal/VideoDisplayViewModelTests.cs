#region Usings

using System;
using NUnit.Framework;
using UI.WPF.Modules.General.ViewModels.Internal;

#endregion

namespace UI.WPF.Modules.General.Tests.ViewModels.Internal
{
    [TestFixture]
    public class VideoDisplayViewModelTests
    {
        [Test]
        public void TestComputeAspectRatio()
        {
            Assert.AreEqual(new Tuple<int, int>(16, 10), VideoDisplayViewModel.ComputeAspectRatio(1920, 1200));
            Assert.AreEqual(new Tuple<int, int>(16, 9), VideoDisplayViewModel.ComputeAspectRatio(1920, 1080));
            Assert.AreEqual(new Tuple<int, int>(4, 3), VideoDisplayViewModel.ComputeAspectRatio(1600, 1200));
        }
    }
}
