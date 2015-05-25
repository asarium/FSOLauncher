using System.Threading.Tasks;
using ModInstallation.Interfaces;
using ModInstallation.Interfaces.Mods;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Modules.Installation.Interfaces;
using UI.WPF.Modules.Installation.ViewModels.Mods;

namespace UI.WPF.Modules.Installation.ViewModels.ListOverview
{
    public class PackageListViewModel : ReactiveObjectBase, IInstallationState
    {
        public IReadOnlyReactiveList<ModGroupViewModel> ModGroupViewModels { get; private set; }

        public PackageListViewModel(IInstallationManager manager, IModInstallationManager modInstallation)
        {
            ModGroupViewModels = manager.ModGroups.CreateDerivedCollection(x => new ModGroupViewModel(x, modInstallation));
        }

        #region Implementation of IInstallationState

        public Task<StateResult> GetResult()
        {
            return null;
        }

        #endregion
    }
}