#region Usings

using FSOManagement.Profiles;
using FSOManagement.Tests.Util;
using NUnit.Framework;

#endregion

namespace FSOManagement.Tests.Profiles
{
    [TestFixture]
    public class ProfileTests
    {
        [Test]
        public void TestClone()
        {
            {
                var profile = new Profile{Name = "Test"};

                var clone = profile.Clone();

                Assert.IsInstanceOf<Profile>(clone);
                Assert.AreEqual("Test", ((Profile) clone).Name);

                Assert.AreNotSame(profile, clone);
            }
            {
                var profile = new Profile
                {
                    SelectedAudioDevice = "TestDevice",
                    SelectedExecutable = new Executable("/fs2_open_3_7_1_20140629_r10856.exe"),
                     Name = "Test" 
                };

                var clone = profile.Clone();

                Assert.IsInstanceOf<Profile>(clone);

                var clonedProfile = (Profile) clone;
                Assert.AreEqual("Test", clonedProfile.Name);
                Assert.AreEqual(profile.SelectedAudioDevice, clonedProfile.SelectedAudioDevice);

                Assert.AreEqual(profile.SelectedExecutable, clonedProfile.SelectedExecutable);
                Assert.AreNotSame(profile.SelectedExecutable, clonedProfile.SelectedExecutable);

                Assert.AreNotSame(profile, clone);
            }
        }
    }
}
