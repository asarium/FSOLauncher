using FSOManagement.Annotations;
using FSOManagement.Interfaces.Mod;
using ModInstallation.Interfaces.Mods;

namespace ModInstallation.Implementations.Management
{
    public class InstalledModification : ILocalModification
    {
        public InstalledModification([NotNull] IInstalledModification installedModification)
        {
            Modification = installedModification;
        }

        [NotNull]
        public IInstalledModification Modification { get; private set; }

        public string ModRootPath { get; set; }

        public IModDependencies Dependencies { get; set; }
    }
}