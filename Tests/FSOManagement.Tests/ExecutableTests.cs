#region Usings

using NUnit.Framework;

#endregion

namespace FSOManagement.Tests
{
    [TestFixture]
    public class ExecutableTests
    {
        [Test]
        public void TestGetFromPath()
        {
            {
                var exe = new Executable("/fs2_open_3_7_1_20140629_r10856.exe");

                Assert.AreEqual(ExecutableType.FreeSpace, exe.Type);
                Assert.AreEqual(ExecutableMode.Release, exe.Mode);
                Assert.AreEqual(ExecutableFeatureSet.None, exe.FeatureSet);

                Assert.AreEqual(3, exe.Major);
                Assert.AreEqual(7, exe.Minor);
                Assert.AreEqual(1, exe.Release);
                Assert.AreEqual(10856, exe.Revision);

                CollectionAssert.IsEmpty(exe.AdditionalTags);
            }
            {
                var exe = new Executable("/fs2_open_3_7_0-DEBUG.exe");

                Assert.AreEqual(ExecutableType.FreeSpace, exe.Type);
                Assert.AreEqual(ExecutableMode.Debug, exe.Mode);
                Assert.AreEqual(ExecutableFeatureSet.None, exe.FeatureSet);

                Assert.AreEqual(3, exe.Major);
                Assert.AreEqual(7, exe.Minor);
                Assert.AreEqual(0, exe.Release);
                Assert.AreEqual(-1, exe.Revision);

                CollectionAssert.IsEmpty(exe.AdditionalTags);
            }
            {
                var exe = new Executable("/fs2_open_3_7_1_SSE2_BP.exe");

                Assert.AreEqual(ExecutableType.FreeSpace, exe.Type);
                Assert.AreEqual(ExecutableMode.Release, exe.Mode);
                Assert.AreEqual(ExecutableFeatureSet.SSE2, exe.FeatureSet);

                Assert.AreEqual(3, exe.Major);
                Assert.AreEqual(7, exe.Minor);
                Assert.AreEqual(1, exe.Release);
                Assert.AreEqual(-1, exe.Revision);

                CollectionAssert.AreEqual(new[] {"BP"}, exe.AdditionalTags);
            }
            {
                var exe = new Executable("/fred2_open_3_7_1_20140629_r10856.exe");

                Assert.AreEqual(ExecutableType.FRED, exe.Type);
                Assert.AreEqual(ExecutableMode.Release, exe.Mode);
                Assert.AreEqual(ExecutableFeatureSet.None, exe.FeatureSet);

                Assert.AreEqual(3, exe.Major);
                Assert.AreEqual(7, exe.Minor);
                Assert.AreEqual(1, exe.Release);
                Assert.AreEqual(10856, exe.Revision);

                CollectionAssert.IsEmpty(exe.AdditionalTags);
            }
            {
                var exe = new Executable("/fred2_open_3_7_0-DEBUG.exe");

                Assert.AreEqual(ExecutableType.FRED, exe.Type);
                Assert.AreEqual(ExecutableMode.Debug, exe.Mode);
                Assert.AreEqual(ExecutableFeatureSet.None, exe.FeatureSet);

                Assert.AreEqual(3, exe.Major);
                Assert.AreEqual(7, exe.Minor);
                Assert.AreEqual(0, exe.Release);
                Assert.AreEqual(-1, exe.Revision);

                CollectionAssert.IsEmpty(exe.AdditionalTags);
            }
            {
                var exe = new Executable("/fred2_open_3_7_1_SSE2_BP.exe");

                Assert.AreEqual(ExecutableType.FRED, exe.Type);
                Assert.AreEqual(ExecutableMode.Release, exe.Mode);
                Assert.AreEqual(ExecutableFeatureSet.SSE2, exe.FeatureSet);

                Assert.AreEqual(3, exe.Major);
                Assert.AreEqual(7, exe.Minor);
                Assert.AreEqual(1, exe.Release);
                Assert.AreEqual(-1, exe.Revision);

                CollectionAssert.AreEqual(new[] {"BP"}, exe.AdditionalTags);
            }
            {
                var exe = new Executable("/fs2_open_3_7_2_RC3-DEBUG.exe");

                Assert.AreEqual(ExecutableType.FreeSpace, exe.Type);
                Assert.AreEqual(ExecutableMode.Debug, exe.Mode);
                Assert.AreEqual(ExecutableFeatureSet.None, exe.FeatureSet);

                Assert.AreEqual(3, exe.Major);
                Assert.AreEqual(7, exe.Minor);
                Assert.AreEqual(2, exe.Release);
                Assert.AreEqual(-1, exe.Revision);

                CollectionAssert.AreEqual(new[] {"RC3"}, exe.AdditionalTags);
            }
        }
    }
}
