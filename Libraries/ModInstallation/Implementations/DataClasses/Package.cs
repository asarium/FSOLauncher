﻿#region Usings

using System.Collections.Generic;
using ModInstallation.Annotations;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#endregion

namespace ModInstallation.Implementations.DataClasses
{
    [UsedImplicitly]
    public class Package
    {
        [CanBeNull]
        public string notes { get; set; }

        [NotNull]
        public string name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PackageStatus status { get; set; }

        [NotNull]
        public IEnumerable<Dependency> dependencies { get; set; }

        [NotNull]
        public IDictionary<string, FileInformation> files { get; set; }

        [CanBeNull]
        public IEnumerable<EnvironmentConstraint> environment { get; set; }
    }
}
