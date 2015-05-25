using System.Collections.Generic;
using ModInstallation.Interfaces.Mods;
using Semver;

namespace ModInstallation.Implementations.Mods
{
    public class DefaultModGroup<T> : IModGroup<T> where T : IModification
    {
        private readonly Dictionary<SemVersion, T> _versions;

        #region Implementation of IModGroup

        public DefaultModGroup(IEnumerable<T> mods)
        {
            _versions = new Dictionary<SemVersion, T>();
            foreach (var modification in mods)
            {
                Id = modification.Id;

                _versions.Add(modification.Version, modification);
            }
        }

        public string Id { get; private set; }

        public IReadOnlyDictionary<SemVersion, T> Versions
        {
            get { return _versions; }
        }

        #endregion
    }
}