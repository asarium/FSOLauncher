#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Caliburn.Micro;
using FSOManagement.Interfaces;
using ReactiveUI;
using UI.WPF.Launcher.Common.Interfaces;

#endregion

namespace UI.WPF.Launcher.ViewModels
{
    public class ProfileInputViewModel : PropertyChangedBase, IDataErrorInfo
    {
        private string _clonedName;

        private bool _cloningProfile;

        private string _profileName;

        public ProfileInputViewModel(IProfileManager profileManager)
        {
            ProfileNames = profileManager.Profiles.CreateDerivedCollection(profile => profile.Name);
            ClonedName = profileManager.CurrentProfile == null ? null : profileManager.CurrentProfile.Name;

            CanAcceptObservable = this.WhenAny(x => x.ProfileName, name => !string.IsNullOrEmpty(name.Value));
        }

        public IReactiveCollection<string> ProfileNames { get; private set; }

        public string ProfileName
        {
            get { return _profileName; }
            set
            {
                if (value == _profileName)
                {
                    return;
                }
                _profileName = value;
                NotifyOfPropertyChange();
            }
        }

        public bool CloningProfile
        {
            get { return _cloningProfile; }
            set
            {
                if (value.Equals(_cloningProfile))
                {
                    return;
                }
                _cloningProfile = value;
                NotifyOfPropertyChange();
            }
        }

        public string ClonedName
        {
            get { return _clonedName; }
            set
            {
                if (value == _clonedName)
                {
                    return;
                }
                _clonedName = value;
                NotifyOfPropertyChange();
            }
        }

        public IObservable<bool> CanAcceptObservable { get; private set; }

        public string this[string columnName]
        {
            get
            {
                if (columnName == "ProfileName")
                {
                    if (string.IsNullOrEmpty(ProfileName))
                        return "A name is required.";
                }

                return null;
            }
        }

        public string Error {
            get { return string.Empty; }
        }
    }
}
