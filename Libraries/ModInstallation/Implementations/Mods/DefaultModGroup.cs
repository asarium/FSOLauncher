using System.Collections.Generic;
using ModInstallation.Interfaces.Mods;
using Semver;

namespace ModInstallation.Implementations.Mods
{
    public class DefaultModGroup<T> : IModGroup<T> where T : IModification
    {
        #region Implementation of IModGroup

        public DefaultModGroup(IEnumerable<T> mods)
        {
            Versions = new Dictionary<SemVersion, T>();
            foreach (var modification in mods)
            {
                Id = modification.Id;

                Versions.Add(modification.Version, modification);
            }
        }

        public string Id { get; private set; }

        public IDictionary<SemVersion, T> Versions { get; private set; }

        #endregion
    }
}