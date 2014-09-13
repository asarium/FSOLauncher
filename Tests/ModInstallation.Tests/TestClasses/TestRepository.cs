#region Usings

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Implementations;

#endregion

namespace ModInstallation.Tests.TestClasses
{
    public class TestRepository : AbstractJsonRepository
    {
        public TestRepository([NotNull] string name) : base(name)
        {
        }

        protected override async Task<string> GetRepositoryJsonAsync(IProgress<string> reporter, CancellationToken token)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream("ModInstallation.Tests.TestData.testRepository.json"))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException("Couldn't find test resource in assembly!");
                }

                using (var reader = new StreamReader(stream))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}
