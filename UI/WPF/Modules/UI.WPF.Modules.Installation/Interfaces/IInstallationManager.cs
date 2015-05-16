#region Usings

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ModInstallation.Interfaces.Mods;
using ReactiveUI;

#endregion

namespace UI.WPF.Modules.Installation.Interfaces
{
    public interface IInstallationManager : INotifyPropertyChanged
    {
        IInstallationState CurrentState { get; }

        ICommand UpdatePackageListCommand { get; }

        Task UpdatePackageList();

        Task InstallPackages(IEnumerable<IPackage> packages);
    }
}
