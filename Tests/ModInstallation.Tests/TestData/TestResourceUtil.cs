#region Usings

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ModInstallation.Annotations;

#endregion

namespace ModInstallation.Tests.TestData
{
    public static class TestResourceUtil
    {
        [NotNull]
        public static async Task<string> GetTestResource([NotNull] string name)
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
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}
