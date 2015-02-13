#region Usings

using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using ModInstallation.Implementations.DataClasses;
using ModInstallation.Implementations.Mods;
using NUnit.Framework;

#endregion

namespace ModInstallation.Tests.Implementations.Mods
{
    [TestFixture]
    public class PostInstallActionsTests
    {
        [Test]
        public void TestGetPaths()
        {
            var fileSys = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                {@"c:\dir\wrong", new MockFileData(string.Empty)},
                {@"c:\dir\MediaVPs_2014\mod.ini", new MockFileData(string.Empty)},
                {@"c:\dir\MediaVPs_2014\test.vp", new MockFileData(string.Empty)},
                {@"c:\dir\MediaVPs_2014\test1.vp", new MockFileData(string.Empty)},
                {@"c:\dir\MediaVPs_2014\test2.vp", new MockFileData(string.Empty)},
                {@"c:\dir\MediaVPs_2014\data\", new MockDirectoryData()},
                {@"c:\dir\MediaVPs_2014\data\test.txt", new MockFileData(string.Empty)},
                {@"c:\dir\MediaVPs_2014\data\tables\", new MockDirectoryData()},
            });

            var actions = new PostInstallActions(fileSys, Enumerable.Empty<ActionData>());
            var files = actions.GetPaths(new ActionData
            {
                dest = "",
                glob = true,
                paths = new[] {@"MediaVPs_2014/*"},
                type = ActionType.Move
            },
                @"c:\dir\");

            CollectionAssert.AreEqual(
                new[]
                {
                    @"c:\dir\MediaVPs_2014\",
                    @"c:\dir\MediaVPs_2014\data\",
                    @"c:\dir\MediaVPs_2014\mod.ini",
                    @"c:\dir\MediaVPs_2014\test.vp",
                    @"c:\dir\MediaVPs_2014\test1.vp",
                    @"c:\dir\MediaVPs_2014\test2.vp",
                },
                files);
        }
    }
}
