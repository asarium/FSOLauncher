using ModInstallation.Interfaces;
using UI.WPF.Launcher.Common.Interfaces;

namespace UI.WPF.Modules.Implementations.Implementations
{
    public class ModRepositoryViewModel : IModRepositoryViewModel
    {
        public string Name { get; set; }

        public IModRepository Repository { get; set; }
    }
}