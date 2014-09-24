#region Usings

using ModInstallation.Implementations.DataClasses;
using ModInstallation.Implementations.Mods;
using NUnit.Framework;

#endregion

namespace ModInstallation.Tests.Implementations.Mods
{
    [TestFixture]
    public class DefaultPackageTests
    {
        [Test]
        public void TestGetEnvironmentContraint()
        {
            Assert.IsInstanceOf<CpuFeatureEnvironmentConstraint>(
                DefaultPackage.GetEnvironmentContraint(new EnvironmentConstraint() {type = EnvironmentType.Cpu_feature, value = ValueTypes.AVX}));

            Assert.IsInstanceOf<OsEnvironmentConstraint>(
                DefaultPackage.GetEnvironmentContraint(new EnvironmentConstraint() { type = EnvironmentType.Os, value = ValueTypes.Macos }));
        }
    }
}
