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
