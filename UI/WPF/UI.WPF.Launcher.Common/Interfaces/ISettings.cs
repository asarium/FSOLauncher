using System;
using System.Collections.Generic;
using System.ComponentModel;
using FSOManagement;
using FSOManagement.Interfaces;

namespace UI.WPF.Launcher.Common.Interfaces
{
    public interface ISettings : INotifyPropertyChanged, ILauncherSettings
    {
        IEnumerable<TotalConversion> TotalConversions { get; set; }

        int Width { get; set; }

        int Height { get; set; }

        bool CheckForUpdates { get; set; }
    }
}