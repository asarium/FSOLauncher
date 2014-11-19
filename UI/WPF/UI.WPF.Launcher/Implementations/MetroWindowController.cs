using System;
using System.Windows;
using FSOManagement.Annotations;
using MahApps.Metro.Controls;

namespace UI.WPF.Launcher.Implementations
{
    public class MetroWindowController
    {
        [CanBeNull]
        protected static MetroWindow Window
        {
            get
            {
                if (Application.Current.MainWindow == null)
                {
                    return null;
                }

                var metroWindow = Application.Current.MainWindow as MetroWindow;

                if (metroWindow != null)
                {
                    return metroWindow;
                }

                throw new InvalidOperationException("ApplicationMain window is not a metro window!");
            }
        } 
    }
}