#region Usings

using System;
using System.Linq;
using FSOManagement.Implementations;
using NUnit.Framework;

#endregion

namespace FSOManagement.Tests.Implementations
{
    [TestFixture, Platform("Win")]
    public class WindowsSpeechHandlerTests
    {
        [Test]
        public void TestInstalledVoices()
        {
            Assert.DoesNotThrow(() =>
            {
                using (var handler = new WindowsSpeechHandler())
                {
                    Console.WriteLine(string.Join(",", handler.InstalledVoices.Select(voice => voice.Name)));
                }
            });
        }
    }
}
