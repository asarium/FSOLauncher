#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
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
    public sealed class ProfileManager : ReactiveObject, IProfileManager
    {
        private readonly IReactiveList<IProfile> _profiles;

        private IProfile _currentProfile;

        [ImportingConstructor]
        public ProfileManager([NotNull] ISettings settings)
        {
            _profiles = new ReactiveList<IProfile>();
            _profiles.Changed.Subscribe(_ => settings.Profiles = _profiles);

            CurrentProfileObservable = this.WhenAny(x => x.CurrentProfile, x => x.Value);

            this.WhenAnyValue(x => x.CurrentProfile).BindTo(settings, x => x.SelectedProfile);

            settings.SettingsLoaded.Subscribe(newSettings =>
            {
                if (newSettings == null)
                {
                    return;
                }

                using (_profiles.SuppressChangeNotifications())
                {
                    _profiles.Clear();
                    if (newSettings.Profiles.Any())
                    {
                        _profiles.AddRange(newSettings.Profiles);
                    }
                    else
                    {
                        var defaultProfile = CreateNewProfile("Default");
                        defaultProfile.PullConfigurationAsync(CancellationToken.None);

                        _profiles.Add(defaultProfile);
                    }

                    CurrentProfile = newSettings.SelectedProfile ?? _profiles.FirstOrDefault();
                }
            });
        }

        #region IProfileManager Members

        public IEnumerable<IProfile> Profiles
        {
            get { return _profiles; }
        }

        public IProfile CurrentProfile
        {
            get { return _currentProfile; }
            set { this.RaiseAndSetIfChanged(ref _currentProfile, value); }
        }

        public IObservable<IProfile> CurrentProfileObservable { get; private set; }

        public void AddProfile(IProfile profile)
        {
            _profiles.Add(profile);
        }

        public IProfile CreateNewProfile(string name)
        {
            return new Profile
            {
                Name = name
            };
        }

        #endregion
    }
}
