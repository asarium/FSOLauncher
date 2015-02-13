#region Usings

using ModInstallation.Annotations;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Util;
using Moq;
using NUnit.Framework;
using Semver;

#endregion

namespace ModInstallation.Tests.Util
{
    [TestFixture]
    public class DataClassExtensionsTests
    {
        private class TestConstraint : IVersionConstraint
        {
            public TestConstraint(ConstraintType type, [NotNull] SemVersion version)
            {
                Type = type;
                Version = version;
            }

            #region IVersionConstraint Members

            public ConstraintType Type { get; private set; }

            public SemVersion Version { get; private set; }

            #endregion
        }

        [Test]
        public void TestGetConstraintString()
        {
            Assert.AreEqual("*", new TestConstraint(ConstraintType.Any, new SemVersion(1)).GetConstraintString());

            Assert.AreEqual("<1.0.0", new TestConstraint(ConstraintType.LessThan, new SemVersion(1)).GetConstraintString());

            Assert.AreEqual("<=1.0.0", new TestConstraint(ConstraintType.LessThanEqual, new SemVersion(1)).GetConstraintString());

            Assert.AreEqual("==1.0.0", new TestConstraint(ConstraintType.Equal, new SemVersion(1)).GetConstraintString());

            Assert.AreEqual(">1.0.0", new TestConstraint(ConstraintType.GreaterThan, new SemVersion(1)).GetConstraintString());

            Assert.AreEqual(">=1.0.0", new TestConstraint(ConstraintType.GreaterThanEqual, new SemVersion(1)).GetConstraintString());

            Assert.AreEqual("!=1.0.0", new TestConstraint(ConstraintType.NotEqual, new SemVersion(1)).GetConstraintString());
        }
    }
}
