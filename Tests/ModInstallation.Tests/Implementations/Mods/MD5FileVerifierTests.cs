#region Usings

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Implementations.Mods;
using NUnit.Framework;

#endregion

namespace ModInstallation.Tests.Implementations.Mods
{
    [TestFixture]
    public class MD5FileVerifierTests
    {
        #region Setup/Teardown

        [SetUp]
        public void ExtractTestFile()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream("ModInstallation.Tests.TestData.random.txt"))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException("Couldn't find test resource in assembly!");
                }

                using (var outStream = File.Open(_outPath, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(outStream);
                }
            }
        }

        [TearDown]
        public void CleanUp()
        {
            if (File.Exists(_outPath))
            {
                File.Delete(_outPath);
            }
        }

        #endregion

        private readonly string _outPath = Path.Combine(Path.GetTempPath(), "random.txt");

        [NotNull, Test]
        public async Task TestVerifyFilePathAsync()
        {
            var md5Verifier = new Md5FileVerifier("F887CEE5C513F2C93D76BF47F7AD71C9");

            Assert.IsTrue(await md5Verifier.VerifyFilePathAsync(_outPath, CancellationToken.None, null));
            Assert.IsFalse(await md5Verifier.VerifyFilePathAsync(Assembly.GetExecutingAssembly().Location, CancellationToken.None, null));
        }
    }
}
