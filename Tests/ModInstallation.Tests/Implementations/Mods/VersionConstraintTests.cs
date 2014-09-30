#region Usings

using ModInstallation.Implementations.Mods;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Util;
using NUnit.Framework;
using Semver;

#endregion

namespace ModInstallation.Tests.Implementations.Mods
{
    [TestFixture]
    public class VersionConstraintTests
    {
        [Test]
        public void TestVersionMatches()
        {
            var constraint = new VersionConstraint(ConstraintType.Any, null);

            Assert.IsTrue(constraint.VersionMatches(new SemVersion(1)));

            constraint = new VersionConstraint(ConstraintType.Equal, new SemVersion(1));

            Assert.IsTrue(constraint.VersionMatches(new SemVersion(1)));
            Assert.IsFalse(constraint.VersionMatches(new SemVersion(2)));

            constraint = new VersionConstraint(ConstraintType.GreaterThan, new SemVersion(1));

            Assert.IsTrue(constraint.VersionMatches(new SemVersion(2)));
            Assert.IsFalse(constraint.VersionMatches(new SemVersion(1)));

            constraint = new VersionConstraint(ConstraintType.GreaterThanEqual, new SemVersion(1));

            Assert.IsTrue(constraint.VersionMatches(new SemVersion(2)));
            Assert.IsTrue(constraint.VersionMatches(new SemVersion(1)));
            Assert.IsFalse(constraint.VersionMatches(new SemVersion(0, 9)));

            constraint = new VersionConstraint(ConstraintType.LessThan, new SemVersion(3));

            Assert.IsTrue(constraint.VersionMatches(new SemVersion(2)));
            Assert.IsFalse(constraint.VersionMatches(new SemVersion(3)));

            constraint = new VersionConstraint(ConstraintType.LessThanEqual, new SemVersion(3));

            Assert.IsTrue(constraint.VersionMatches(new SemVersion(2)));
            Assert.IsTrue(constraint.VersionMatches(new SemVersion(3)));
            Assert.IsFalse(constraint.VersionMatches(new SemVersion(4)));

            constraint = new VersionConstraint(ConstraintType.NotEqual, new SemVersion(3));

            Assert.IsTrue(constraint.VersionMatches(new SemVersion(2)));
            Assert.IsFalse(constraint.VersionMatches(new SemVersion(3)));
            Assert.IsTrue(constraint.VersionMatches(new SemVersion(4)));
        }
    }
}
