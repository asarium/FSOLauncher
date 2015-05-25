#region Usings

using FSOManagement.Annotations;
using FSOManagement.Interfaces.Mod;
using ModInstallation.Interfaces.Mods;

#endregion

namespace ModInstallation.Implementations.Management
{
    public class InstalledModification : ILocalModification
    {
        public InstalledModification([NotNull] IInstalledModification installedModification)
        {
            Modification = installedModification;
            ModRootPath = Modification.InstallPath;
        }

        [NotNull]
        public IInstalledModification Modification { get; private set; }

        #region ILocalModification Members

        public string ModRootPath { get; private set; }

        #endregion
    }
}
