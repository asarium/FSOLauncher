#region Usings

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ModInstallation.Interfaces.Mods;
using UI.WPF.Modules.Installation.ViewModels.Installation;

#endregion

namespace UI.WPF.Modules.Installation.Interfaces
{
    public interface IInstallationManager : INotifyPropertyChanged
    {
        ICommand UpdatePackageListCommand { get; }

        IEnumerable<IModGroup<IModification>> ModGroups { get; }

        Task UpdatePackageList();

        Task ExecuteChanges(IEnumerable<InstallationItem> installItems, IEnumerable<InstallationItem> uninstallItems);
    }
}
