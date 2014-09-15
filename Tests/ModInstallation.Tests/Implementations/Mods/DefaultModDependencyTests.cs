#region Usings

using System.Collections.Generic;
using ModInstallation.Annotations;
using ModInstallation.Implementations.DataClasses;
using ModInstallation.Implementations.Mods;
using NUnit.Framework;
using Semver;

#endregion

namespace ModInstallation.Tests.Implementations.Mods
{
    [TestFixture]
    public class DefaultModDependencyTests
    {
        [NotNull]
        private static IEnumerable<T> GetEnumerable<T>([NotNull] params T[] p)
        {
            return p;
        }

        [Test]
        public void TestParseVersionConstraint()
        {
            Assert.AreEqual(new VersionConstraint(ConstraintType.Any, null), DefaultModDependency.ParseVersionConstraint("*"));
            Assert.AreEqual(new VersionConstraint(ConstraintType.Equal, new SemVersion(1)), DefaultModDependency.ParseVersionConstraint("1.0"));

            Assert.AreEqual(new VersionConstraint(ConstraintType.GreaterThan, new SemVersion(1)), DefaultModDependency.ParseVersionConstraint(">1.0"));
            Assert.AreEqual(new VersionConstraint(ConstraintType.GreaterThanEqual, new SemVersion(1)),
                DefaultModDependency.ParseVersionConstraint(">=1.0"));

            Assert.AreEqual(new VersionConstraint(ConstraintType.Equal, new SemVersion(1)), DefaultModDependency.ParseVersionConstraint("==1.0"));
            Assert.AreEqual(new VersionConstraint(ConstraintType.NotEqual, new SemVersion(1)), DefaultModDependency.ParseVersionConstraint("!=1.0"));

            Assert.AreEqual(new VersionConstraint(ConstraintType.LessThan, new SemVersion(1)), DefaultModDependency.ParseVersionConstraint("<1.0"));
            Assert.AreEqual(new VersionConstraint(ConstraintType.LessThanEqual, new SemVersion(1)),
                DefaultModDependency.ParseVersionConstraint("<=1.0"));
        }

        [Test]
        public void TestParseVersionString()
        {
            CollectionAssert.AreEqual(
                GetEnumerable(new VersionConstraint(ConstraintType.GreaterThan, new SemVersion(1)),
                    (new VersionConstraint(ConstraintType.LessThan, new SemVersion(2)))), DefaultModDependency.ParseVersionString(">1.0, <2.0"));

            CollectionAssert.AreEqual(
                GetEnumerable(new VersionConstraint(ConstraintType.GreaterThanEqual, new SemVersion(1)),
                    (new VersionConstraint(ConstraintType.LessThanEqual, new SemVersion(2)))), DefaultModDependency.ParseVersionString(">=1.0, <=2.0"));
        }

        [Test]
        public void TestVersionMatches()
        {
            var dependency = DefaultModDependency.InitializeFromData(new Dependency() {version = ">1.0,<2.0"});
            Assert.IsTrue(dependency.VersionMatches(new SemVersion(1, 1)));
            Assert.IsFalse(dependency.VersionMatches(new SemVersion(1)));
            Assert.IsFalse(dependency.VersionMatches(new SemVersion(2)));

            dependency = DefaultModDependency.InitializeFromData(new Dependency() {version = "*"});
            Assert.IsTrue(dependency.VersionMatches(new SemVersion(1)));

            dependency = DefaultModDependency.InitializeFromData(new Dependency() {version = "1.0"});
            Assert.IsTrue(dependency.VersionMatches(new SemVersion(1)));
            Assert.IsFalse(dependency.VersionMatches(new SemVersion(2)));

            dependency = DefaultModDependency.InitializeFromData(new Dependency() {version = "==1.0"});
            Assert.IsTrue(dependency.VersionMatches(new SemVersion(1)));
            Assert.IsFalse(dependency.VersionMatches(new SemVersion(2)));

            dependency = DefaultModDependency.InitializeFromData(new Dependency() {version = "!=1.0"});
            Assert.IsFalse(dependency.VersionMatches(new SemVersion(1)));
            Assert.IsTrue(dependency.VersionMatches(new SemVersion(2)));
        }
    }
}
