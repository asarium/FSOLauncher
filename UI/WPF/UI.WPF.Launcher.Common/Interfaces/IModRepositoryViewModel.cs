using FSOManagement.Annotations;
using ModInstallation.Interfaces;

namespace UI.WPF.Launcher.Common.Interfaces
{
    public interface IModRepositoryViewModel
    {
        [NotNull]
        string Name { get; }

        [NotNull]
        IModRepository Repository { get; }
    }
}