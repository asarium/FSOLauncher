using ModInstallation.Implementations.Mods;
using NUnit.Framework;
using Semver;

namespace ModInstallation.Tests.Implementations.Mods
{
    [TestFixture]
    public class DefaultModDependencyTests
    {
        [Test]
        public void TestParseVersionConstraint()
        {
            Assert.AreEqual(new VersionConstraint(ConstraintType.Any, null), DefaultModDependency.ParseVersionConstraint("*"));
            Assert.AreEqual(new VersionConstraint(ConstraintType.Equal, new SemVersion(1)), DefaultModDependency.ParseVersionConstraint("1.0"));

            Assert.AreEqual(new VersionConstraint(ConstraintType.GreaterThan, new SemVersion(1)), DefaultModDependency.ParseVersionConstraint(">1.0"));
            Assert.AreEqual(new VersionConstraint(ConstraintType.GreaterThanEqual, new SemVersion(1)), DefaultModDependency.ParseVersionConstraint(">=1.0"));

            Assert.AreEqual(new VersionConstraint(ConstraintType.Equal, new SemVersion(1)), DefaultModDependency.ParseVersionConstraint("==1.0"));
            Assert.AreEqual(new VersionConstraint(ConstraintType.NotEqual, new SemVersion(1)), DefaultModDependency.ParseVersionConstraint("!=1.0"));

            Assert.AreEqual(new VersionConstraint(ConstraintType.LessThan, new SemVersion(1)), DefaultModDependency.ParseVersionConstraint("<1.0"));
            Assert.AreEqual(new VersionConstraint(ConstraintType.LessThanEqual, new SemVersion(1)), DefaultModDependency.ParseVersionConstraint("<=1.0"));
        }
    }
}