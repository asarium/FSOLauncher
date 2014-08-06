#region Usings

using System;
using System.Collections.Generic;
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
            if (settings.TotalConversions != null)
            {
                _totalConversions.AddRange(settings.TotalConversions);
            }

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
