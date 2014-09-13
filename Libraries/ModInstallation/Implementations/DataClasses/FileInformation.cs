using System.Collections.Generic;
using ModInstallation.Annotations;

namespace ModInstallation.Implementations.DataClasses
{
    [UsedImplicitly]
    public class FileInformation
    {
        [CanBeNull]
        public string md5sum { get; set; }

        public bool is_archive { get; set; }

        [CanBeNull]
        public string dest { get; set; }

        [NotNull]
        public IEnumerable<string> urls { get; set; }
    }
}