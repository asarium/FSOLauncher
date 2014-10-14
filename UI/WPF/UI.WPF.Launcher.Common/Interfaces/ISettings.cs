#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using FSOManagement;
using FSOManagement.Annotations;
using FSOManagement.Interfaces;

#endregion

namespace UI.WPF.Launcher.Common.Interfaces
{
    public interface ISettings : INotifyPropertyChanged, ILauncherSettings
    {
        [NotNull]
        IEnumerable<TotalConversion> TotalConversions { get; set; }

        int Width { get; set; }

        int Height { get; set; }

        bool CheckForUpdates { get; set; }

        [NotNull]
        IObservable<ISettings> SettingsLoaded { get; }

        [NotNull]
        Task LoadAsync();

        [NotNull]
        Task SaveAsync();
    }
}
