using System.IO;
using FSOManagement.Profiles;
using FSOManagement.Tests.Util;
using NUnit.Framework;

namespace FSOManagement.Tests.Profiles
{
    [TestFixture]
    public class ProfileTests
    {
        [Test]
        public void TestSerializable()
        {
            var profile = new Profile("Test");

            SerializationAssert.IsSerializable(profile);
        }
    }
}