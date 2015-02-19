#region Usings

using ModInstallation.Interfaces.Mods;
using UI.WPF.Launcher.Common.Classes;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels.Dependencies
{
    public class DependencyPackageViewModel : ReactiveObjectBase
    {
        public DependencyPackageViewModel(IPackage package)
        {
            Package = package;
        }

        public IPackage Package { get; private set; }
    }
}
