#region Usings

using FSOManagement.URLHandler;
using NUnit.Framework;

#endregion

namespace FSOManagement.Tests.URLHandler
{
    [TestFixture]
    public class ProtocolActionTests
    {
        [Test]
        public void TestParseUrl()
        {
            {
                var action = ProtocolAction.ParseUrl("fso://focus");

                Assert.AreEqual(ProtocolActionType.Focus, action.Action);
                CollectionAssert.IsEmpty(action.Arguments);
            }
            {
                var action = ProtocolAction.ParseUrl("fso://run/test");

                Assert.AreEqual(ProtocolActionType.Run, action.Action);
                CollectionAssert.AreEqual(action.Arguments, new[] {"test"});
            }
            {
                var action = ProtocolAction.ParseUrl("fso://install/test");

                Assert.AreEqual(ProtocolActionType.Install, action.Action);
                CollectionAssert.AreEqual(action.Arguments, new[] {"test"});
            }
            {
                var action = ProtocolAction.ParseUrl("fso://settings");

                Assert.AreEqual(ProtocolActionType.Settings, action.Action);
                CollectionAssert.IsEmpty(action.Arguments);
            }
            {
                var action = ProtocolAction.ParseUrl("fso://settings/test");

                Assert.AreEqual(ProtocolActionType.Settings, action.Action);
                CollectionAssert.AreEqual(action.Arguments, new[] { "test" });
            }
            {
                var action = ProtocolAction.ParseUrl("fso://add_repo/http%3A%2F%2Fexample.org%2Fmods.json");

                Assert.AreEqual(ProtocolActionType.AddRepo, action.Action);
                CollectionAssert.AreEqual(action.Arguments, new[] { "http://example.org/mods.json" });
            }
        }
    }
}
