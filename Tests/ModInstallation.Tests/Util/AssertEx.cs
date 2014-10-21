#region Usings

using System;
using System.IO;
using System.Threading.Tasks;
using ModInstallation.Annotations;
using NUnit.Framework;

#endregion

namespace ModInstallation.Tests.Util
{
    public static class AssertEx
    {
        [NotNull]
        public static async Task FileContent([NotNull] string filePath, [NotNull] string expectedContent)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(stream))
                {
                    Assert.AreEqual(expectedContent, await reader.ReadToEndAsync());
                }
            }
        }

        [NotNull]
        public static async Task ThrowsAsync<TException>([NotNull] Func<Task> func)
        {
            var expected = typeof(TException);
            Type actual = null;
            try
            {
                await func();
            }
            catch (Exception e)
            {
                actual = e.GetType();
            }
            Assert.AreEqual(expected, actual);
        }
    }
}
