using System.Collections.Generic;
using FSOManagement.Annotations;
using FSOManagement.Profiles.DataClass;

namespace UI.WPF.Modules.Implementations.Implementations.DataClasses
{
    internal class SettingsData
    {
        [CanBeNull]
        public IEnumerable<ProfileData> Profiles { get; set; }

        [CanBeNull]
        public IEnumerable<RepositoryData> Repositories { get; set; }

        [CanBeNull]
        public string SelectedProfile { get; set; }

        [CanBeNull]
        public IEnumerable<TcData> TotalConversoins { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public bool CheckForUpdates { get; set; }
    }
}