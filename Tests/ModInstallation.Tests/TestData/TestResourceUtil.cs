#region Usings

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using NUnit.Framework;

#endregion

namespace ModInstallation.Tests.TestData
{
    public static class TestResourceUtil
    {
        [NotNull]
        public static string GetTestResource([NotNull] string name)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream("ModInstallation.Tests.TestData." + name))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException("Couldn't find test resource " + name);
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        [NotNull]
        public static async Task<FileInfo> ExtractTestResource([NotNull] string name)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream("ModInstallation.Tests.TestData." + name))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException("Couldn't find test resource " + name);
                }

                var outName = Path.Combine(TestContext.CurrentContext.WorkDirectory, TestContext.CurrentContext.Test.Name, name);

                var dirName = Path.GetDirectoryName(outName);
                if (dirName != null)
                {
                    Directory.CreateDirectory(dirName);
                }

                using (var outStream = new FileStream(outName, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(outStream);
                    return new FileInfo(outName);
                }
            }
        }
    }
}
