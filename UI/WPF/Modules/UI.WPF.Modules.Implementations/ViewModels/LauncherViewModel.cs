#region Usings

using System;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using FSOManagement;
using ReactiveUI;
using UI.WPF.Launcher.Common.Interfaces;

#endregion

namespace UI.WPF.Modules.Implementations.ViewModels
{
    [Export(typeof(ILauncherViewModel))]
    public class LauncherViewModel : PropertyChangedBase, ILauncherViewModel
    {
        private readonly ReactiveList<TotalConversion> _totalConversions;

        [ImportingConstructor]
        public LauncherViewModel(ISettings settings)
        {
            _totalConversions = new ReactiveList<TotalConversion>();

            settings.SettingsLoaded.Subscribe(newSettings =>
            {
                if (newSettings == null)
                {
                    return;
                }

                using (_totalConversions.SuppressChangeNotifications())
                {
                    _totalConversions.Clear();
                    _totalConversions.AddRange(newSettings.TotalConversions);
                }
            });

            _totalConversions.Changed.Subscribe(_ => settings.TotalConversions = _totalConversions);
        }

        #region ILauncherViewModel Members

        public IReactiveList<TotalConversion> TotalConversions
        {
            get { return _totalConversions; }
        }

        [Import]
        public IProfileManager ProfileManager { get; private set; }

        #endregion
    }
}
