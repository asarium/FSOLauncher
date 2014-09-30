#region Usings

using System.IO;
using ModInstallation.Annotations;
using NUnit.Framework;

#endregion

namespace ModInstallation.Tests.Util
{
    public static class TestExtensions
    {
        [NotNull]
        public static string GetTestDirectory()
        {
            return Path.Combine(TestContext.CurrentContext.WorkDirectory, TestContext.CurrentContext.Test.Name);
        }
    }
}
