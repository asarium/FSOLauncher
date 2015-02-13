#region Usings

using System.Collections.Generic;
using System.ComponentModel;
using Caliburn.Micro;
using FSOManagement;
using FSOManagement.Annotations;
using ModInstallation.Interfaces;
using ReactiveUI;

#endregion

namespace UI.WPF.Launcher.Common.Interfaces
{
    public interface ILauncherViewModel : INotifyPropertyChanged
    {
        [NotNull]
        IReactiveList<TotalConversion> TotalConversions { get; }

        [NotNull]
        IReactiveList<IModRepositoryViewModel> ModRepositories { get; }

        [NotNull]
        IProfileManager ProfileManager { get; }
    }
}
