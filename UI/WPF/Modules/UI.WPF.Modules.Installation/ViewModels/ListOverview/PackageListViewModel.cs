using System.Threading.Tasks;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Modules.Installation.Interfaces;
using UI.WPF.Modules.Installation.ViewModels.Mods;

namespace UI.WPF.Modules.Installation.ViewModels.ListOverview
{
    public class PackageListViewModel : ReactiveObjectBase, IInstallationState
    {
        private InstallationTabViewModel TabViewModel { get; set; }

        public IReadOnlyReactiveList<ModGroupViewModel> ModGroupViewModels { get; private set; }

        public PackageListViewModel(InstallationTabViewModel tabViewModel)
        {
            TabViewModel = tabViewModel;

            ModGroupViewModels = tabViewModel.ModGroups.CreateDerivedCollection(x => new ModGroupViewModel(x, tabViewModel));
        }

        #region Implementation of IInstallationState

        public Task<StateResult> GetResult()
        {
            return null;
        }

        #endregion
    }
}