#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using FSOManagement;
using FSOManagement.Interfaces;
using FSOManagement.Profiles;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Properties;

#endregion

namespace UI.WPF.Launcher.Implementations
{
    [Export(typeof(ISettings)), Export(typeof(ILauncherSettings))]
    public class SettingsAdapter : PropertyChangedBase, ISettings
    {
        private readonly Settings _settings;

        public SettingsAdapter()
        {
            _settings = Settings.Default;

            _settings.PropertyChanged += SettingsOnPropertyChanged;
        }

        #region ISettings Members

        public int Height
        {
            get { return _settings.Height; }
            set { _settings.Height = value; }
        }

        public bool CheckForUpdates
        {
            get { return _settings.CheckForUpdates; }
            set { _settings.CheckForUpdates = value; }
        }

        public void Save()
        {
            _settings.Save();
        }

        public IEnumerable<TotalConversion> TotalConversions
        {
            get { return _settings.TotalConversions; }
            set { _settings.TotalConversions = value.ToArray(); }
        }

        public int Width
        {
            get { return _settings.Width; }
            set { _settings.Width = value; }
        }

        public IProfile SelectedProfile
        {
            get { return Profiles == null ? null : Profiles.FirstOrDefault(p => p.Name == _settings.SelectedProfileName); }
            set { _settings.SelectedProfileName = value == null ? null : value.Name; }
        }

        public IEnumerable<IProfile> Profiles
        {
            get { return _settings.Profiles; }
            set
            {
                if (!value.All(p => p is Profile))
                {
                    throw new InvalidOperationException("Only profiles of type Profile are allowed!");
                }

                _settings.Profiles = value.Cast<Profile>().ToArray();
            }
        }

        #endregion

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                default:
                    // Assume everything else is named like our own 
                    NotifyOfPropertyChange(propertyChangedEventArgs.PropertyName);
                    break;
            }
        }
    }
}
