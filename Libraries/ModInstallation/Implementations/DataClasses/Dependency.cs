using System.Collections.Generic;
using ModInstallation.Annotations;

namespace ModInstallation.Implementations.DataClasses
{
    [UsedImplicitly]
    public class Dependency
    {
        [NotNull]
        public string id { get; set; }

        [NotNull]
        public string version { get; set; }

        [CanBeNull]
        public IEnumerable<string> packages { get; set; }
    }
}