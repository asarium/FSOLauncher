#region Usings

using System.Collections.Generic;
using System.ComponentModel;
using Caliburn.Micro;
using FSOManagement.Interfaces;
using FSOManagement.Profiles;
using ReactiveUI;

#endregion

namespace UI.WPF.Launcher.Common.Interfaces
{
    public interface IProfileManager : INotifyPropertyChanged
    {
        IEnumerable<IProfile> Profiles { get; }

        IProfile CurrentProfile { get; set; }
    }
}
