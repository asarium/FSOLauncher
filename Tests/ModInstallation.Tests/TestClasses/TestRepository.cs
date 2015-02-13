#region Usings

using System;
using System.Threading;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using ModInstallation.Implementations;
using ModInstallation.Tests.TestData;

#endregion

namespace ModInstallation.Tests.TestClasses
{
    public class TestRepository : AbstractJsonRepository
    {
        public TestRepository([NotNull] string name) : base(name)
        {
        }

        protected override async Task<string> GetRepositoryJsonAsync(Uri location, IProgress<string> reporter, CancellationToken token)
        {
            return await TestResourceUtil.GetTestResource("testRepository.json");
        }
    }
}
