using System.Collections.Generic;
using ModInstallation.Interfaces.Mods;
using Semver;

namespace ModInstallation.Implementations.Mods
{
    public class DefaultModGroup : IModGroup
    {
        #region Implementation of IModGroup

        public DefaultModGroup(IEnumerable<IModification> mods)
        {
            Versions = new Dictionary<SemVersion, IModification>();
            foreach (var modification in mods)
            {
                Id = modification.Id;

                Versions.Add(modification.Version, modification);
            }
        }

        public string Id { get; private set; }

        public IDictionary<SemVersion, IModification> Versions { get; private set; }

        #endregion
    }
}