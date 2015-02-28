#region Usings

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Caliburn.Micro;
using FSOManagement;
using FSOManagement.Annotations;
using ModInstallation.Interfaces;
using ReactiveUI;

#endregion

namespace UI.WPF.Launcher.Common.Interfaces
{
    public interface ILauncherViewModel : IConductor
    {
        [NotNull]
        IReactiveList<TotalConversion> TotalConversions { get; }

        [NotNull]
        IReactiveList<IModRepositoryViewModel> ModRepositories { get; }

        [NotNull]
        IEnumerable<ILauncherTab> LauncherTabs { set; }

        [NotNull]
        IProfileManager ProfileManager { get; }

        ICommand AddProfileCommand { get; }

        ICommand LaunchGameCommand { get; }

        ICommand AddGameRootCommand { get; }
    }
}
