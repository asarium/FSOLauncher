using System.Collections.Generic;
using ModInstallation.Annotations;

namespace ModInstallation.Implementations.DataClasses
{
    [UsedImplicitly]
    public class Repository
    {
        [CanBeNull]
        public IEnumerable<string> includes { get; set; }

        [CanBeNull]
        public List<Modification> mods { get; set; }
    }
}