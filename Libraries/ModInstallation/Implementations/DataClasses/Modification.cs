#region Usings

using System.Collections.Generic;
using ModInstallation.Annotations;

#endregion

namespace ModInstallation.Implementations.DataClasses
{
    [UsedImplicitly]
    public class Modification
    {
        [CanBeNull]
        public string description { get; set; }

        [NotNull]
        public string title { get; set; }

        [NotNull]
        public string version { get; set; }

        [NotNull]
        public string id { get; set; }

        [CanBeNull]
        public string logo { get; set; }

        [NotNull]
        public IEnumerable<Package> packages { get; set; }

        [CanBeNull]
        public IEnumerable<ActionData> actions { get; set; }
    }
}
