#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using FSOManagement.Annotations;
using FSOManagement.Interfaces;
using FSOManagement.Profiles;
using ReactiveUI;
using UI.WPF.Launcher.Common.Interfaces;

#endregion

namespace UI.WPF.Modules.Implementations.Implementations
{
    [Export(typeof(IProfileManager))]
    public sealed class ProfileManager : IProfileManager
    {
        private IProfile _currentProfile;

        private readonly IReactiveList<IProfile> _profiles;

        [ImportingConstructor]
        public ProfileManager(ILauncherSettings settings)
        {
            _profiles = new ReactiveList<IProfile>();
            _profiles.Changed.Subscribe(_ => settings.Profiles = _profiles);

            this.WhenAnyValue(x => x.CurrentProfile).BindTo(settings, x => x.SelectedProfile);

            if (settings.Profiles != null)
            {
                _profiles.AddRange(settings.Profiles);
            }
            else
            {
                var defaultProfile = new Profile("Default");
                defaultProfile.PullConfigurationAsync(CancellationToken.None);

                _profiles.Add(defaultProfile);
            }

            CurrentProfile = settings.SelectedProfile ?? _profiles.FirstOrDefault();
        }

        #region IProfileManager Members

        public IEnumerable<IProfile> Profiles
        {
            get { return _profiles; }
        }

        public IProfile CurrentProfile
        {
            get { return _currentProfile; }
            set
            {
                if (Equals(value, _currentProfile))
                {
                    return;
                }
                _currentProfile = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
