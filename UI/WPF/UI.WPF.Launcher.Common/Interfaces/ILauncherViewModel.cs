#region Usings

using System.Collections.Generic;
using System.ComponentModel;
using Caliburn.Micro;
using FSOManagement;
using ReactiveUI;

#endregion

namespace UI.WPF.Launcher.Common.Interfaces
{
    public interface ILauncherViewModel : INotifyPropertyChanged
    {
        IReactiveList<TotalConversion> TotalConversions { get; }

        IProfileManager ProfileManager { get; }
    }
}
