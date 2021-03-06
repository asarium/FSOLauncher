﻿#region Usings

using System.Collections.Generic;
using ModInstallation.Annotations;

#endregion

namespace ModInstallation.Implementations.DataClasses
{
    [UsedImplicitly]
    public class FileInformation
    {
        [CanBeNull]
        public string filename { get; set; }

        [CanBeNull]
        public string md5sum { get; set; }

        public bool is_archive { get; set; }

        [CanBeNull]
        public string dest { get; set; }

        public long filesize { get; set; }

        [CanBeNull]
        public IDictionary<string, string> contents { get; set; }

        [NotNull]
        public IEnumerable<string> urls { get; set; }
    }
}
