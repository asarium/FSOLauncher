using FSOManagement.Annotations;

namespace ModInstallation.Interfaces.Mods
{
    public interface IInstalledModification : IModification
    {
        [NotNull]
        string InstallPath { get; }
    }
}